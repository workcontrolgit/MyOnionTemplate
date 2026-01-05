namespace MyOnion.Application.Features.SalaryRanges.Commands.UpdateSalaryRange
{
    /// <summary>
    /// Validates salary range update commands.
    /// </summary>
    public class UpdateSalaryRangeCommandValidator : AbstractValidator<UpdateSalaryRangeCommand>
    {
        public UpdateSalaryRangeCommandValidator()
        {
            RuleFor(s => s.Id)
                .NotEmpty().WithMessage("{PropertyName} is required.");

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
