using MyOnion.Application.Events;

namespace MyOnion.Application.Features.Employees.Commands.DeleteEmployeeById
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
            private readonly IEventDispatcher _eventDispatcher;

            public DeleteEmployeeByIdCommandHandler(
                IEmployeeRepositoryAsync repository,
                IEventDispatcher eventDispatcher)
            {
                _repository = repository;
                _eventDispatcher = eventDispatcher;
            }

            public async Task<Result<Guid>> Handle(DeleteEmployeeByIdCommand command, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(command.Id);
                if (entity == null)
                {
                    throw new ApiException("Employee Not Found.");
                }

                await _repository.DeleteAsync(entity);
                await _eventDispatcher.PublishAsync(new EmployeeChangedEvent(entity.Id), cancellationToken);
                return Result<Guid>.Success(entity.Id);
            }
        }
    }
}
