// Defines an asynchronous repository interface for the Department entity
namespace MyOnion.Application.Interfaces.Repositories
{
    public interface IDepartmentRepositoryAsync : IGenericRepositoryAsync<Department>
    {
        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetDepartmentResponseAsync(GetDepartmentsQuery requestParameters);
    }
}
