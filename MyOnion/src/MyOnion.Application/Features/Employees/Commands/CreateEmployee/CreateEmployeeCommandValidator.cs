namespace MyOnion.Application.Features.Employees.Commands.CreateEmployee
{
    /// <summary>
    /// Validates employee creation requests.
    /// </summary>
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(e => e.FirstName)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(e => e.LastName)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(e => e.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .EmailAddress().WithMessage("{PropertyName} must be a valid email.");

            RuleFor(e => e.EmployeeNumber)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(20).WithMessage("{PropertyName} must not exceed 20 characters.");

            RuleFor(e => e.PositionId)
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleFor(e => e.Salary)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be positive.");

            RuleFor(e => e.Birthday)
                .LessThan(DateTime.UtcNow).WithMessage("{PropertyName} must be in the past.");
        }
    }
}
