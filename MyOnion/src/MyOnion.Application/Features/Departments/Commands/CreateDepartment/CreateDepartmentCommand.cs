using MyOnion.Application.Events;

namespace MyOnion.Application.Features.Departments.Commands.CreateDepartment
{
    /// <summary>
    /// Command to create a new department.
    /// </summary>
    public class CreateDepartmentCommand : IRequest<Result<Guid>>
    {
        /// <summary>
        /// Department name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Handles department creation and returns the identifier.
        /// </summary>
        public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
        {
            private readonly IDepartmentRepositoryAsync _repository;
            private readonly IMapper _mapper;
            private readonly IEventDispatcher _eventDispatcher;

            public CreateDepartmentCommandHandler(
                IDepartmentRepositoryAsync repository,
                IMapper mapper,
                IEventDispatcher eventDispatcher)
            {
                _repository = repository;
                _mapper = mapper;
                _eventDispatcher = eventDispatcher;
            }

            public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
            {
                var department = _mapper.Map<Department>(request);
                await _repository.AddAsync(department);
                await _eventDispatcher.PublishAsync(new DepartmentChangedEvent(department.Id), cancellationToken);
                return Result<Guid>.Success(department.Id);
            }
        }
    }
}
