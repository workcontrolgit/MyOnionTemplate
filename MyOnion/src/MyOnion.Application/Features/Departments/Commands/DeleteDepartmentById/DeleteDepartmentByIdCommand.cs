using MyOnion.Application.Events;

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
            private readonly IEventDispatcher _eventDispatcher;

            public DeleteDepartmentByIdCommandHandler(
                IDepartmentRepositoryAsync repository,
                IEventDispatcher eventDispatcher)
            {
                _repository = repository;
                _eventDispatcher = eventDispatcher;
            }

            public async Task<Result<Guid>> Handle(DeleteDepartmentByIdCommand command, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(command.Id);
                if (entity == null)
                {
                    throw new ApiException("Department Not Found.");
                }

                await _repository.DeleteAsync(entity);
                await _eventDispatcher.PublishAsync(new DepartmentChangedEvent(entity.Id), cancellationToken);
                return Result<Guid>.Success(entity.Id);
            }
        }
    }
}
