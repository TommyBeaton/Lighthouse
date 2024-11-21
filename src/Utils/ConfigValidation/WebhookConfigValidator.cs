using FluentValidation;

namespace Kurrent.Utils.ConfigValidation;
public class WebhookConfigValidator : AbstractValidator<WebhookConfig>
{
    public WebhookConfigValidator()
    {
        RuleFor(w => w.EventName)
            .NotEmpty()
            .WithMessage("Webhook EventName is required.");

        RuleFor(w => w.Path)
            .NotEmpty()
            .WithMessage("Webhook Path is required.");

        RuleFor(w => w.Type)
            .NotEmpty()
            .WithMessage("Webhook Type is required.")
            .Must(type => type == "acr" || type == "docker")
            .WithMessage("Webhook Type must be 'acr' or 'docker'.");
    }
}
