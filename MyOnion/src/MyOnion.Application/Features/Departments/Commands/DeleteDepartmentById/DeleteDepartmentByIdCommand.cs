namespace MyOnion.Application.Features.Departments.Commands.DeleteDepartmentById
{
    /// <summary>
    /// Command to delete a department by identifier.
    /// </summary>
    public class DeleteDepartmentByIdCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteDepartmentByIdCommandHandler : IRequestHandler<DeleteDepartmentByIdCommand, Result<Guid>>
        {
            private readonly IDepartmentRepositoryAsync _repository;

            public DeleteDepartmentByIdCommandHandler(IDepartmentRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Guid>> Handle(DeleteDepartmentByIdCommand command, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(command.Id);
                if (entity == null)
                {
                    throw new ApiException("Department Not Found.");
                }

                await _repository.DeleteAsync(entity);
                return Result<Guid>.Success(entity.Id);
            }
        }
    }
}
