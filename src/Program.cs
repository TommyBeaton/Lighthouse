using Kurrent.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient();

builder.Configuration.AddEnvironmentVariables();

builder.RegisterServices(builder.Configuration);

var app = builder.Build();

// Validate app configuration before start up.
app.ValidateConfiguration();

app.MapHealthChecks("/health");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.AddWebhooks();

app.MapGet("/status", () => StatusCodes.Status200OK);

app.Run();

namespace Kurrent
{
    public partial class Program { }
}