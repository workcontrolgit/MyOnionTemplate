namespace $safeprojectname$.Features.Employees.Commands.DeleteEmployeeById
{
    /// <summary>
    /// Command to delete an employee by id.
    /// </summary>
    public class DeleteEmployeeByIdCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteEmployeeByIdCommandHandler : IRequestHandler<DeleteEmployeeByIdCommand, Result<Guid>>
        {
            private readonly IEmployeeRepositoryAsync _repository;

            public DeleteEmployeeByIdCommandHandler(IEmployeeRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Guid>> Handle(DeleteEmployeeByIdCommand command, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(command.Id);
                if (entity == null)
                {
                    throw new ApiException("Employee Not Found.");
                }

                await _repository.DeleteAsync(entity);
                return Result<Guid>.Success(entity.Id);
            }
        }
    }
}

