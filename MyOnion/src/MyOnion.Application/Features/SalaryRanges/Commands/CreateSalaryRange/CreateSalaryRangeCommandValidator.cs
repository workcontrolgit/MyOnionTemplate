namespace MyOnion.Application.Features.SalaryRanges.Commands.CreateSalaryRange
{
    /// <summary>
    /// Validates create salary range commands.
    /// </summary>
    public class CreateSalaryRangeCommandValidator : AbstractValidator<CreateSalaryRangeCommand>
    {
        public CreateSalaryRangeCommandValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(s => s.MinSalary)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be positive.");

            RuleFor(s => s.MaxSalary)
                .GreaterThanOrEqualTo(s => s.MinSalary).WithMessage("{PropertyName} must be greater than MinSalary.");
        }
    }
}
