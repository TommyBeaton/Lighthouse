using FluentValidation;

namespace Kurrent.Utils.ConfigValidation;

public class RepositoryConfigValidator : AbstractValidator<RepositoryConfig>
{
    public RepositoryConfigValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .WithMessage("Repository Name is required.");

        RuleFor(r => r.Url)
            .NotEmpty()
            .WithMessage("Repository Url is required.")
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Repository Url must be a valid URL.");

        RuleFor(r => r.FileExtensions)
            .NotNull()
            .WithMessage("Repository FileExtensions must not be null.")
            .Must(list => list.Any())
            .WithMessage("Repository must have at least one file extension.")
            .ForEach(ext =>
            {
                ext.NotEmpty().WithMessage("File extension cannot be empty.");
                ext.Must(e => e.StartsWith("."))
                   .WithMessage("File extension '{PropertyValue}' must start with a '.'.");
            });

        RuleFor(r => r.EventSubscriptions)
            .NotNull()
            .WithMessage("Repository EventSubscriptions must not be null.")
            .Must(list => list.Any())
            .WithMessage("Repository must have at least one event subscription.")
            .ForEach(subscription =>
            {
                subscription.NotEmpty().WithMessage("Event subscription cannot be empty.");
            });

        RuleFor(r => r.Branch)
            .NotEmpty()
            .WithMessage("Repository Branch is required.");
    }
}
