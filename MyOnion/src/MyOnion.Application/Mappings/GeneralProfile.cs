using MyOnion.Application.Features.Departments.Commands.CreateDepartment;
using MyOnion.Application.Features.Employees.Commands.CreateEmployee;
using MyOnion.Application.Features.Positions.DTOs;
using MyOnion.Application.Features.SalaryRanges.Commands.CreateSalaryRange;

namespace MyOnion.Application.Mappings
{
    // Defines a mapping profile for general mappings between entities and view models.
    public class GeneralProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Maps an Employee entity to a GetEmployeesViewModel, and vice versa.
            config.NewConfig<Employee, GetEmployeesViewModel>().TwoWays();

            // Maps a Position entity to a PositionSummaryDto, and vice versa.
            config.NewConfig<Position, PositionSummaryDto>().TwoWays();
            // Maps a Department entity to a GetDepartmentsViewModel, and vice versa.
            config.NewConfig<Department, GetDepartmentsViewModel>().TwoWays();

            // Maps a SalaryRange entity to a GetSalaryRangesViewModel, and vice versa.
            config.NewConfig<SalaryRange, GetSalaryRangesViewModel>().TwoWays();
            // Maps a CreatePositionCommand to a Position entity.
            config.NewConfig<CreatePositionCommand, Position>();
            // Maps a CreateDepartmentCommand to a Department entity.
            config.NewConfig<CreateDepartmentCommand, Department>();
            // Maps a CreateEmployeeCommand to an Employee entity.
            config.NewConfig<CreateEmployeeCommand, Employee>();
            // Maps a CreateSalaryRangeCommand to a SalaryRange entity.
            config.NewConfig<CreateSalaryRangeCommand, SalaryRange>();
        }
    }
}
