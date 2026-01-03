namespace $safeprojectname$.Features.Departments.Commands.UpdateDepartment
{
    /// <summary>
    /// Validates update requests for departments.
    /// </summary>
    public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator()
        {
            RuleFor(d => d.Id)
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleFor(d => d.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");
        }
    }
}

