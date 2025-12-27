using Microsoft.EntityFrameworkCore;
using MyOnion.Application.Features.Employees.Queries.GetEmployees;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Interfaces.Repositories;
using MyOnion.Application.Parameters;
using MyOnion.Application.Specifications.Employees;
using MyOnion.Domain.Common;
using MyOnion.Domain.Entities;
using MyOnion.Infrastructure.Persistence.Contexts;

namespace MyOnion.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepositoryAsync : GenericRepositoryAsync<Employee>, IEmployeeRepositoryAsync
    {
        private readonly DbSet<Employee> _repository;
        private readonly IDataShapeHelper<Employee> _dataShaper;

        public EmployeeRepositoryAsync(
            ApplicationDbContext dbContext,
            IDataShapeHelper<Employee> dataShaper) : base(dbContext)
        {
            _repository = dbContext.Set<Employee>();
            _dataShaper = dataShaper;
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetEmployeeResponseAsync(GetEmployeesQuery requestParameters)
        {
            var recordsTotal = await _repository.CountAsync();

            var filteredSpecification = new EmployeesByFiltersSpecification(requestParameters, applyPaging: false);
            var pagedSpecification = new EmployeesByFiltersSpecification(requestParameters);

            var recordsFiltered = await CountAsync(filteredSpecification);
            var resultData = await ListAsync(pagedSpecification);

            var shapedData = _dataShaper.ShapeData(resultData, requestParameters.Fields);
            var recordsCount = BuildRecordsCount(recordsTotal, recordsFiltered);

            return (shapedData, recordsCount);
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedEmployeeResponseAsync(PagedEmployeesQuery requestParameters)
        {
            var recordsTotal = await _repository.CountAsync();

            var filteredSpecification = new EmployeesKeywordSpecification(requestParameters, applyPaging: false);
            var pagedSpecification = new EmployeesKeywordSpecification(requestParameters);

            var recordsFiltered = await CountAsync(filteredSpecification);
            var resultData = await ListAsync(pagedSpecification);

            var shapedData = _dataShaper.ShapeData(resultData, requestParameters.Fields);
            var recordsCount = BuildRecordsCount(recordsTotal, recordsFiltered);

            return (shapedData, recordsCount);
        }

        private static RecordsCount BuildRecordsCount(int total, int filtered)
        {
            return new RecordsCount
            {
                RecordsFiltered = filtered,
                RecordsTotal = total
            };
        }
    }
}
