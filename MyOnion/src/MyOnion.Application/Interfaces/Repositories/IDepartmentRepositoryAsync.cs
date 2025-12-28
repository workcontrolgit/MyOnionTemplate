// Defines an asynchronous repository interface for the Department entity
namespace MyOnion.Application.Interfaces.Repositories
{
    public interface IDepartmentRepositoryAsync : IGenericRepositoryAsync<Department>
    {
        // Methods inherited from IGenericRepositoryAsync<Department> will be available here, such as AddAsync, UpdateAsync, and DeleteAsync.
    }
}