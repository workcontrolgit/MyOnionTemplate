namespace $safeprojectname$.Features.Departments.Queries.GetDepartments
{
    /// <summary>
    /// GetAllDepartmentsQuery - handles media IRequest
    /// BaseRequestParameter - contains paging parameters
    /// To add filter/search parameters, add search properties to the body of this class
    /// </summary>
    public class GetDepartmentsQuery : QueryParameter, IRequest<PagedResult<IEnumerable<Entity>>>
    {
        public string Name { get; set; }
    }

    public class GetAllDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, PagedResult<IEnumerable<Entity>>>
    {
        private readonly IDepartmentRepositoryAsync _repository;
        private readonly IModelHelper _modelHelper;

        /// <summary>
        /// Constructor for GetAllDepartmentsQueryHandler class.
        /// </summary>
        /// <param name="repository">IDepartmentRepositoryAsync object.</param>
        /// <param name="modelHelper">IModelHelper object.</param>
        /// <returns>
        /// GetAllDepartmentsQueryHandler object.
        /// </returns>
        public GetAllDepartmentsQueryHandler(IDepartmentRepositoryAsync repository, IModelHelper modelHelper)
        {
            _repository = repository;
            _modelHelper = modelHelper;
        }

        /// <summary>
        /// Handles the GetDepartmentsQuery request and returns a PagedResponse containing the requested data.
        /// </summary>
        /// <param name="request">The GetDepartmentsQuery request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A PagedResponse containing the requested data.</returns>
        public async Task<PagedResult<IEnumerable<Entity>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var objRequest = request;
            if (!string.IsNullOrEmpty(objRequest.Fields))
            {
                objRequest.Fields = _modelHelper.ValidateModelFields<GetDepartmentsViewModel>(objRequest.Fields);
            }
            else
            {
                objRequest.Fields = _modelHelper.GetModelFields<GetDepartmentsViewModel>();
            }

            if (!string.IsNullOrEmpty(objRequest.OrderBy))
            {
                objRequest.OrderBy = _modelHelper.ValidateModelFields<GetDepartmentsViewModel>(objRequest.OrderBy);
            }

            var result = await _repository.GetDepartmentResponseAsync(objRequest);
            return PagedResult<IEnumerable<Entity>>.Success(result.data, objRequest.PageNumber, objRequest.PageSize, result.recordsCount);
        }
    }
}

