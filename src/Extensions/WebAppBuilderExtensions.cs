using FluentValidation.Results;
using Kurrent.Implementation;
using Kurrent.Implementation.API;
using Kurrent.Implementation.Git;
using Kurrent.Implementation.Notifications;
using Kurrent.Implementation.Notifications.Notifiers;
using Kurrent.Implementation.Polling;
using Kurrent.Implementation.Polling.Pollers;
using Kurrent.Interfaces;
using Kurrent.Interfaces.API;
using Kurrent.Interfaces.Git;
using Kurrent.Interfaces.Notifications;
using Kurrent.Interfaces.Polling;
using Kurrent.Utils;
using Kurrent.Utils.ConfigValidation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace Kurrent.Extensions;

public static class WebAppBuilderExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        RegisterConfig(builder, configuration);
        RegisterAppServices(builder);
        RegisterPollerFactory(builder);
        RegisterNotifierFactory(builder);
        
        builder.Services.AddHostedService<PollerManager>();
        builder.Services.AddMemoryCache();
    }

    private static void RegisterConfig(WebApplicationBuilder builder, IConfiguration configuration)
    {
        if (builder.Environment.EnvironmentName.Contains("k8s"))
        {
            var reloadOnChange = Environment.GetEnvironmentVariable("ReloadConfigOnChange")?.ToLower() == "true";
            builder.Configuration.AddJsonFile(ConfigMapFileProvider.FromRelativePath("config"), "appsettings.k8s.json", optional: true, reloadOnChange: reloadOnChange);
        }

#if DEBUG
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
#endif

        builder.Services.Configure<AppConfig>(
            configuration.GetSection("Kurrent")
        );
        builder.Services.Configure<SystemConfig>(
            configuration.GetSection("System")
        );
    }

    private static void RegisterAppServices(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ISubscriptionHandler, SubscriptionHandler>();
        builder.Services.AddTransient<IWebhookHandler, WebhookHandler>();
        builder.Services.AddTransient<IRepositoryUpdater, RepositoryUpdater>();
        builder.Services.AddTransient<IFileUpdater, FileUpdater>();
        builder.Services.AddTransient<IGitService, GitService>();
        builder.Services.AddTransient<INotificationHandler, NotificationHandler>();
        builder.Services.AddTransient<IDockerHubWrapper, DockerHubWrapper>();
        builder.Services.AddTransient<IAcrWrapper, AcrWrapper>();
    }
    
    private static void RegisterPollerFactory(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<AcrPoller>();
        builder.Services.AddTransient<DockerHubPoller>();
        
        builder.Services.AddSingleton<IPollerFactory>(ctx =>
        {
            var factories = new Dictionary<string, Func<IPoller>>()
            {
                [KurrentStrings.Acr] = () => ctx.GetService<AcrPoller>(),
                [KurrentStrings.Docker] = () => ctx.GetService<DockerHubPoller>(),
            };
            var logger = ctx.GetService<ILogger<PollerFactory>>();
            return new PollerFactory(factories, logger);
        });
    }

    private static void RegisterNotifierFactory(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<SlackNotifier>();
        
        builder.Services.AddSingleton<INotifierFactory>(ctx =>
        {
            var factories = new Dictionary<string, Func<INotifier>>()
            {
                [KurrentStrings.Slack] = () => ctx.GetService<SlackNotifier>(),
            };
            var logger = ctx.GetService<ILogger<NotifierFactory>>();
            return new NotifierFactory(factories, logger);
        });
    }
    
    public static void AddWebhooks(this WebApplication app)
    {
        var kurrentConfig = app.Services.GetRequiredService<IOptions<AppConfig>>().Value;
        
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(WebAppBuilderExtensions));
        
        if ((kurrentConfig?.Webhooks == null || !kurrentConfig.Webhooks.Any()) && 
            (kurrentConfig?.Pollers == null || !kurrentConfig.Pollers.Any()))
            throw new InvalidOperationException("No pollers or webhooks are not configured");
        
        if(kurrentConfig.Webhooks == null) return;
        
        foreach (var webhook in kurrentConfig.Webhooks)
        {
            logger.LogInformation("Adding webhook: {WebhookName} of type: {WebhookType} with path: {WebhookPath}", 
                webhook.EventName, webhook.Type, webhook.Path);

            app.MapPost(webhook.Path, async (
                HttpContext context, 
                IWebhookHandler webhookHandler) =>
            {
                await webhookHandler.ProcessRequestAsync(context, webhook);
            }); 
        }
    }

    public static void ValidateConfiguration(this WebApplication app)
    {
        var appConfigValidator = new RootConfigValidator();
        var appConfig = app.Services.GetService<IOptions<AppConfig>>().Value ?? throw new InvalidConfigurationException("App configuration settings are missing.");
        ValidationResult result = appConfigValidator.Validate(appConfig);
        
        if (!result.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Configuration validation failed with the following errors:");

            foreach (var failure in result.Errors)
            {
                Console.WriteLine($"- {failure.ErrorMessage}");
            }

            Console.ResetColor();
            Environment.Exit(1);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Configuration validated successfully.");
            Console.ResetColor();
        }
    }
}