namespace MyOnion.Application.Features.Departments.Commands.CreateDepartment
{
    /// <summary>
    /// Validates department creation requests.
    /// </summary>
    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(d => d.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(250).WithMessage("{PropertyName} must not exceed 250 characters.");
        }
    }
}
