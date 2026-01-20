using MyOnion.Application.Events;

namespace MyOnion.Application.Features.Positions.Commands.UpdatePosition
{
    // Define a command class for updating a position
    public class UpdatePositionCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; } // Unique identifier of the position to be updated
        public string PositionTitle { get; set; } // New title for the position
        public string PositionNumber { get; set; } // New number for the position (optional)
        public string PositionDescription { get; set; } // New description for the position
        public Guid DepartmentId { get; set; } // ID of the department to which the position belongs
        public Guid SalaryRangeId { get; set; } // ID of the salary range for the position

        // Define a handler class for processing the update command
        public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, Result<Guid>>
        {
            private readonly IPositionRepositoryAsync _repository; // Repository for accessing positions
            private readonly IEventDispatcher _eventDispatcher;

            // Constructor to inject the repository
            public UpdatePositionCommandHandler(
                IPositionRepositoryAsync positionRepository,
                IEventDispatcher eventDispatcher)
            {
                _repository = positionRepository;
                _eventDispatcher = eventDispatcher;
            }

            // Handle method to process the update command
            public async Task<Result<Guid>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
            {
                var position = await _repository.GetByIdAsync(command.Id); // Get the position by ID

                if (position == null)
                {
                    throw new ApiException($"Position Not Found."); // Throw an exception if the position is not found
                }
                else
                {
                    // Update the position with the new values from the command
                    position.PositionTitle = command.PositionTitle;
                    position.PositionDescription = command.PositionDescription;

                    await _repository.UpdateAsync(position); // Save the updated position to the repository
                    await _eventDispatcher.PublishAsync(new PositionChangedEvent(position.Id), cancellationToken);

                    return Result<Guid>.Success(position.Id); // Return a response containing the ID of the updated position
                }
            }
        }
    }
}


