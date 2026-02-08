using FluentValidation;

namespace MyOnion.Application.Features.Emails.Commands.SendTemplatedEmail;

public class SendTemplatedEmailCommandValidator : AbstractValidator<SendTemplatedEmailCommand>
{
    public SendTemplatedEmailCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        // At least one To recipient is required
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("At least one recipient (To) is required.")
            .Must(recipients => recipients.Count <= 50).WithMessage("Cannot send to more than 50 recipients.");

        // Validate each To email address
        RuleForEach(x => x.To)
            .NotEmpty().WithMessage("Recipient email address cannot be empty.")
            .EmailAddress().WithMessage("'{PropertyValue}' is not a valid email address.")
            .MaximumLength(254).WithMessage("Email address must not exceed 254 characters.");

        // Validate CC recipients (optional)
        When(x => x.Cc != null && x.Cc.Any(), () =>
        {
            RuleFor(x => x.Cc)
                .Must(recipients => recipients.Count <= 50).WithMessage("Cannot send to more than 50 CC recipients.");

            RuleForEach(x => x.Cc)
                .NotEmpty().WithMessage("CC email address cannot be empty.")
                .EmailAddress().WithMessage("'{PropertyValue}' is not a valid email address.")
                .MaximumLength(254).WithMessage("Email address must not exceed 254 characters.");
        });

        // Validate BCC recipients (optional)
        When(x => x.Bcc != null && x.Bcc.Any(), () =>
        {
            RuleFor(x => x.Bcc)
                .Must(recipients => recipients.Count <= 50).WithMessage("Cannot send to more than 50 BCC recipients.");

            RuleForEach(x => x.Bcc)
                .NotEmpty().WithMessage("BCC email address cannot be empty.")
                .EmailAddress().WithMessage("'{PropertyValue}' is not a valid email address.")
                .MaximumLength(254).WithMessage("Email address must not exceed 254 characters.");
        });

        // Validate total recipient count
        RuleFor(x => x)
            .Must(cmd => (cmd.To?.Count ?? 0) + (cmd.Cc?.Count ?? 0) + (cmd.Bcc?.Count ?? 0) <= 100)
            .WithMessage("Total number of recipients (To + CC + BCC) cannot exceed 100.");

        // Validate From field (optional)
        When(x => !string.IsNullOrWhiteSpace(x.From), () =>
        {
            RuleFor(x => x.From)
                .EmailAddress().WithMessage("{PropertyName} must be a valid email address.")
                .MaximumLength(254).WithMessage("{PropertyName} must not exceed 254 characters.");
        });

        // Validate variables dictionary
        RuleFor(x => x.Variables)
            .NotNull().WithMessage("{PropertyName} cannot be null.");
    }
}
