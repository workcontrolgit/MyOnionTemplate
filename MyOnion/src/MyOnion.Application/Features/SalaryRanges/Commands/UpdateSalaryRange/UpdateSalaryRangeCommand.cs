namespace MyOnion.Application.Features.SalaryRanges.Commands.UpdateSalaryRange
{
    /// <summary>
    /// Command to update a salary range.
    /// </summary>
    public class UpdateSalaryRangeCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }

        public class UpdateSalaryRangeCommandHandler : IRequestHandler<UpdateSalaryRangeCommand, Result<Guid>>
        {
            private readonly ISalaryRangeRepositoryAsync _repository;

            public UpdateSalaryRangeCommandHandler(ISalaryRangeRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Guid>> Handle(UpdateSalaryRangeCommand command, CancellationToken cancellationToken)
            {
                var salaryRange = await _repository.GetByIdAsync(command.Id);
                if (salaryRange == null)
                {
                    throw new ApiException("SalaryRange Not Found.");
                }

                salaryRange.Name = command.Name;
                salaryRange.MinSalary = command.MinSalary;
                salaryRange.MaxSalary = command.MaxSalary;

                await _repository.UpdateAsync(salaryRange);
                return Result<Guid>.Success(salaryRange.Id);
            }
        }
    }
}
