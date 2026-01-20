using MyOnion.Application.Events;

namespace MyOnion.Application.Features.Positions.Commands.CreatePosition
{
    // This class represents a command to create a new position.
    public partial class CreatePositionCommand : IRequest<Result<Guid>>
    {
        // The title of the position being created.
        public string PositionTitle { get; set; }

        // A unique number assigned to the position being created.
        public string PositionNumber { get; set; }

        // A description of the position being created.
        public string PositionDescription { get; set; }

        // The ID of the department that the position belongs to.
        public Guid DepartmentId { get; set; }

        // The ID of the salary range associated with the position being created.
        public Guid SalaryRangeId { get; set; }

    }

    // This class handles the logic for creating a new position.
    public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Result<Guid>>
    {
        // A repository to interact with the database for positions.
        private readonly IPositionRepositoryAsync _repository;

        // An object mapper to convert between different data types.
        private readonly IMapper _mapper;
        private readonly IEventDispatcher _eventDispatcher;

        // Constructor that injects the position repository and mapper into the handler.
        public CreatePositionCommandHandler(
            IPositionRepositoryAsync repository,
            IMapper mapper,
            IEventDispatcher eventDispatcher)
        {
            _repository = repository;
            _mapper = mapper;
            _eventDispatcher = eventDispatcher;
        }

        // This method is called when a new position creation command is issued.
        public async Task<Result<Guid>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
        {
            // Maps the incoming command to a Position object using the mapper.
            var position = _mapper.Map<Position>(request);

            // Adds the new position to the database asynchronously.
            await _repository.AddAsync(position);
            await _eventDispatcher.PublishAsync(new PositionChangedEvent(position.Id), cancellationToken);

            // Returns a response containing the ID of the newly created position.
            return Result<Guid>.Success(position.Id);
        }
    }
}


