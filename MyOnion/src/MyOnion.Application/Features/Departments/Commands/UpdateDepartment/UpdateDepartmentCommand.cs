using MyOnion.Application.Events;
using MyOnion.Domain.ValueObjects;

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
            private readonly IEventDispatcher _eventDispatcher;

            public UpdateDepartmentCommandHandler(
                IDepartmentRepositoryAsync repository,
                IEventDispatcher eventDispatcher)
            {
                _repository = repository;
                _eventDispatcher = eventDispatcher;
            }

            public async Task<Result<Guid>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
            {
                var department = await _repository.GetByIdAsync(command.Id);
                if (department == null)
                {
                    throw new ApiException("Department Not Found.");
                }

                department.Name = new DepartmentName(command.Name);
                await _repository.UpdateAsync(department);
                await _eventDispatcher.PublishAsync(new DepartmentChangedEvent(department.Id), cancellationToken);

                return Result<Guid>.Success(department.Id);
            }
        }
    }
}
