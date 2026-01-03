namespace $safeprojectname$.Features.SalaryRanges.Commands.DeleteSalaryRangeById
{
    /// <summary>
    /// Command to delete a salary range.
    /// </summary>
    public class DeleteSalaryRangeByIdCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteSalaryRangeByIdCommandHandler : IRequestHandler<DeleteSalaryRangeByIdCommand, Result<Guid>>
        {
            private readonly ISalaryRangeRepositoryAsync _repository;

            public DeleteSalaryRangeByIdCommandHandler(ISalaryRangeRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Guid>> Handle(DeleteSalaryRangeByIdCommand command, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(command.Id);
                if (entity == null)
                {
                    throw new ApiException("SalaryRange Not Found.");
                }

                await _repository.DeleteAsync(entity);
                return Result<Guid>.Success(entity.Id);
            }
        }
    }
}

