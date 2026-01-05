namespace MyOnion.Application.Features.SalaryRanges.Queries.GetSalaryRanges
{
    /// <summary>
    /// GetAllSalaryRangesQuery - handles media IRequest
    /// BaseRequestParameter - contains paging parameters
    /// To add filter/search parameters, add search properties to the body of this class
    /// </summary>
    public class GetSalaryRangesQuery : QueryParameter, IRequest<PagedResult<IEnumerable<Entity>>>
    {
        public string Name { get; set; }
    }

    public class GetAllSalaryRangesQueryHandler : IRequestHandler<GetSalaryRangesQuery, PagedResult<IEnumerable<Entity>>>
    {
        private readonly ISalaryRangeRepositoryAsync _repository;
        private readonly IModelHelper _modelHelper;

        /// <summary>
        /// Constructor for GetAllSalaryRangesQueryHandler class.
        /// </summary>
        /// <param name="repository">ISalaryRangeRepositoryAsync object.</param>
        /// <param name="modelHelper">IModelHelper object.</param>
        /// <returns>
        /// GetAllSalaryRangesQueryHandler object.
        /// </returns>
        public GetAllSalaryRangesQueryHandler(ISalaryRangeRepositoryAsync repository, IModelHelper modelHelper)
        {
            _repository = repository;
            _modelHelper = modelHelper;
        }

        /// <summary>
        /// Handles the GetSalaryRangesQuery request and returns a PagedResponse containing the requested data.
        /// </summary>
        /// <param name="request">The GetSalaryRangesQuery request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A PagedResponse containing the requested data.</returns>
        public async Task<PagedResult<IEnumerable<Entity>>> Handle(GetSalaryRangesQuery request, CancellationToken cancellationToken)
        {
            var objRequest = request;
            if (!string.IsNullOrEmpty(objRequest.Fields))
            {
                objRequest.Fields = _modelHelper.ValidateModelFields<GetSalaryRangesViewModel>(objRequest.Fields);
            }
            else
            {
                objRequest.Fields = _modelHelper.GetModelFields<GetSalaryRangesViewModel>();
            }

            if (!string.IsNullOrEmpty(objRequest.OrderBy))
            {
                objRequest.OrderBy = _modelHelper.ValidateModelFields<GetSalaryRangesViewModel>(objRequest.OrderBy);
            }

            var result = await _repository.GetSalaryRangeResponseAsync(objRequest);

            return PagedResult<IEnumerable<Entity>>.Success(result.data, objRequest.PageNumber, objRequest.PageSize, result.recordsCount);
        }
    }
}
