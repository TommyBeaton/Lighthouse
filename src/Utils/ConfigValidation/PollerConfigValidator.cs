using FluentValidation;

namespace Kurrent.Utils.ConfigValidation;

public class PollerConfigValidator : AbstractValidator<PollerConfig>
{
    public PollerConfigValidator()
    {
        RuleFor(p => p.EventName)
            .NotEmpty()
            .WithMessage("Poller EventName is required.");

        RuleFor(p => p.Type)
            .NotEmpty()
            .WithMessage("Poller Type is required.")
            .Must(type => type == "acr" || type == "docker")
            .WithMessage("Poller Type must be 'acr' or 'docker'.");

        RuleFor(p => p.IntervalInSeconds)
            .GreaterThanOrEqualTo(15)
            .WithMessage("Poller IntervalInSeconds must be greater than or equal to 15.");

        RuleFor(p => p.Url)
            .NotEmpty()
            .WithMessage("Poller Url is required.")
            .When(p => p.Type == "acr")
            .Must(url => Uri.IsWellFormedUriString($"https://{url}", UriKind.Absolute))
            .WithMessage("Poller Url must be a valid URL.")
            .When(p => !string.IsNullOrEmpty(p.Url));

        RuleFor(p => p.Images)
            .NotNull()
            .WithMessage("Poller Images must not be null.")
            .Must(images => images.Any())
            .WithMessage("Poller must have at least one image.");
    }
}
