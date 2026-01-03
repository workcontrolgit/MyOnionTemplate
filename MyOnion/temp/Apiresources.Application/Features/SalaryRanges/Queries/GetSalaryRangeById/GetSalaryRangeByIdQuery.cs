namespace $safeprojectname$.Features.SalaryRanges.Queries.GetSalaryRangeById
{
    /// <summary>
    /// Query to fetch a salary range by identifier.
    /// </summary>
    public class GetSalaryRangeByIdQuery : IRequest<Result<SalaryRange>>
    {
        public Guid Id { get; set; }

        public class GetSalaryRangeByIdQueryHandler : IRequestHandler<GetSalaryRangeByIdQuery, Result<SalaryRange>>
        {
            private readonly ISalaryRangeRepositoryAsync _repository;

            public GetSalaryRangeByIdQueryHandler(ISalaryRangeRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<SalaryRange>> Handle(GetSalaryRangeByIdQuery query, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(query.Id);
                if (entity == null)
                {
                    throw new ApiException("SalaryRange Not Found.");
                }

                return Result<SalaryRange>.Success(entity);
            }
        }
    }
}

