using MyOnion.Application.Features.Departments.Commands.CreateDepartment;
using MyOnion.Application.Features.Employees.Commands.CreateEmployee;
using MyOnion.Application.Features.Positions.DTOs;
using MyOnion.Application.Features.SalaryRanges.Commands.CreateSalaryRange;
using MyOnion.Domain.ValueObjects;

namespace MyOnion.Application.Mappings
{
    // Defines a mapping profile for general mappings between entities and view models.
    public class GeneralProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Maps an Employee entity to a GetEmployeesViewModel, and vice versa.
            config.NewConfig<Employee, GetEmployeesViewModel>()
                .Map(dest => dest.FirstName, src => src.Name.FirstName)
                .Map(dest => dest.MiddleName, src => src.Name.MiddleName)
                .Map(dest => dest.LastName, src => src.Name.LastName);
            config.NewConfig<GetEmployeesViewModel, Employee>()
                .Map(dest => dest.Name, src => new PersonName(src.FirstName, src.MiddleName, src.LastName));

            // Maps a Position entity to a PositionSummaryDto, and vice versa.
            config.NewConfig<Position, PositionSummaryDto>()
                .Map(dest => dest.PositionTitle, src => src.PositionTitle.Value);
            config.NewConfig<PositionSummaryDto, Position>()
                .Map(dest => dest.PositionTitle, src => new PositionTitle(src.PositionTitle));
            // Maps a Department entity to a GetDepartmentsViewModel, and vice versa.
            config.NewConfig<Department, GetDepartmentsViewModel>()
                .Map(dest => dest.Name, src => src.Name.Value);
            config.NewConfig<GetDepartmentsViewModel, Department>()
                .Map(dest => dest.Name, src => new DepartmentName(src.Name));

            // Maps a SalaryRange entity to a GetSalaryRangesViewModel, and vice versa.
            config.NewConfig<SalaryRange, GetSalaryRangesViewModel>().TwoWays();
            // Maps a CreatePositionCommand to a Position entity.
            config.NewConfig<CreatePositionCommand, Position>()
                .MapWith(src => new Position
                {
                    PositionTitle = new PositionTitle(src.PositionTitle),
                    PositionNumber = src.PositionNumber,
                    PositionDescription = src.PositionDescription,
                    DepartmentId = src.DepartmentId,
                    SalaryRangeId = src.SalaryRangeId
                });
            // Maps a CreateDepartmentCommand to a Department entity.
            config.NewConfig<CreateDepartmentCommand, Department>()
                .MapWith(src => new Department
                {
                    Name = new DepartmentName(src.Name)
                });
            // Maps a CreateEmployeeCommand to an Employee entity.
            config.NewConfig<CreateEmployeeCommand, Employee>()
                .MapWith(src => new Employee
                {
                    Name = new PersonName(src.FirstName, src.MiddleName, src.LastName),
                    PositionId = src.PositionId,
                    DepartmentId = src.DepartmentId,
                    Salary = src.Salary,
                    Birthday = src.Birthday,
                    Email = src.Email,
                    Gender = src.Gender,
                    EmployeeNumber = src.EmployeeNumber,
                    Prefix = src.Prefix,
                    Phone = src.Phone
                });
            // Maps a CreateSalaryRangeCommand to a SalaryRange entity.
            config.NewConfig<CreateSalaryRangeCommand, SalaryRange>()
                .MapWith(src => new SalaryRange
                {
                    Name = src.Name,
                    MinSalary = src.MinSalary,
                    MaxSalary = src.MaxSalary
                });
        }
    }
}
