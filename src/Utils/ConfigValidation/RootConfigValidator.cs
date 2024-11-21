using FluentValidation;
using FluentValidation.Validators;

namespace Kurrent.Utils.ConfigValidation;

public class RootConfigValidator : AbstractValidator<AppConfig>
{
    public RootConfigValidator()
    {
        // Rule: There must be at least one poller or one webhook
        RuleFor(config => config)
            .Must(config => (config.Pollers != null && config.Pollers.Any()) ||
                            (config.Webhooks != null && config.Webhooks.Any()))
            .WithMessage("Configuration must have at least one poller or one webhook.");

        // Rule: Each event name across pollers and webhooks must be unique
        RuleFor(config => config)
            .Must(HaveUniqueEventNames)
            .WithMessage("Event names across pollers and webhooks must be unique.");
        
        RuleFor(config => config)
            .Custom((config, context) =>
            {
                var validEventNames = GetAllEventNames(config);
                context.RootContextData["ValidEventNames"] = validEventNames;
            });

        // Validate child collections
        RuleForEach(config => config.Pollers).SetValidator(new PollerConfigValidator());
        RuleForEach(config => config.Webhooks).SetValidator(new WebhookConfigValidator());
        RuleForEach(config => config.Repositories).SetValidator(new RepositoryConfigValidator());
        RuleForEach(config => config.Notifiers).SetValidator(new NotifierConfigValidator());
    }

    private bool HaveUniqueEventNames(AppConfig config)
    {
        var eventNames = new List<string>();

        if (config.Pollers != null)
            eventNames.AddRange(config.Pollers.Select(p => p.EventName));

        if (config.Webhooks != null)
            eventNames.AddRange(config.Webhooks.Select(w => w.EventName));

        return eventNames.Distinct().Count() == eventNames.Count;
    }
    
    private HashSet<string> GetAllEventNames(AppConfig config)
    {
        var eventNames = new HashSet<string>();

        if (config.Pollers != null)
            eventNames.UnionWith(config.Pollers.Select(p => p.EventName));

        if (config.Webhooks != null)
            eventNames.UnionWith(config.Webhooks.Select(w => w.EventName));

        return eventNames;
    }
}
