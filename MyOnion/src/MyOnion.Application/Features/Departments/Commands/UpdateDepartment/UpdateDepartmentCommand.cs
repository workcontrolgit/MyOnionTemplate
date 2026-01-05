namespace MyOnion.Application.Features.Departments.Commands.UpdateDepartment
{
    /// <summary>
    /// Command to update a department.
    /// </summary>
    public class UpdateDepartmentCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Handles department updates.
        /// </summary>
        public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result<Guid>>
        {
            private readonly IDepartmentRepositoryAsync _repository;

            public UpdateDepartmentCommandHandler(IDepartmentRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Guid>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
            {
                var department = await _repository.GetByIdAsync(command.Id);
                if (department == null)
                {
                    throw new ApiException("Department Not Found.");
                }

                department.Name = command.Name;
                await _repository.UpdateAsync(department);

                return Result<Guid>.Success(department.Id);
            }
        }
    }
}
