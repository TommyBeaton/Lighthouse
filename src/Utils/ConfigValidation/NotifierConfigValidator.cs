using FluentValidation;

namespace Kurrent.Utils.ConfigValidation;

public class NotifierConfigValidator : AbstractValidator<NotifierConfig>
{
    public NotifierConfigValidator()
    {
        RuleFor(n => n.Name)
            .NotEmpty()
            .WithMessage("Notifier Name is required.");

        RuleFor(n => n.Type)
            .NotEmpty()
            .WithMessage("Notifier Type is required.")
            .Must(type => type == "slack")
            .WithMessage("Notifier Type must be 'slack'.");

        RuleFor(n => n.Token)
            .NotEmpty()
            .WithMessage("Notifier Token is required.");

        RuleFor(n => n.Channel)
            .NotEmpty()
            .WithMessage("Notifier Channel is required.");

        RuleFor(n => n.EventSubscriptions)
            .NotNull()
            .WithMessage("Notifier EventSubscriptions must not be null.")
            .Must(list => list.Any())
            .WithMessage("Notifier must have at least one event subscription.")
            .ForEach(subscription =>
            {
                subscription.NotEmpty().WithMessage("Event subscription cannot be empty.");
            });
    }
}
