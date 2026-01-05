using MyOnion.Application.Features.Departments.Commands.CreateDepartment;
using MyOnion.Application.Features.Employees.Commands.CreateEmployee;
using MyOnion.Application.Features.Positions.DTOs;
using MyOnion.Application.Features.SalaryRanges.Commands.CreateSalaryRange;

namespace MyOnion.Application.Mappings
{
    // Defines a mapping profile for general mappings between entities and view models.
    public class GeneralProfile : Profile
    {
        // Initializes a new instance of the GeneralProfile class.
        public GeneralProfile()
        {
            // Maps an Employee entity to a GetEmployeesViewModel, and vice versa.
            CreateMap<Employee, GetEmployeesViewModel>().ReverseMap();

            // Maps a Position entity to a PositionSummaryDto, and vice versa.
            CreateMap<Position, PositionSummaryDto>().ReverseMap();
            // Maps a Department entity to a GetDepartmentsViewModel, and vice versa.
            CreateMap<Department, GetDepartmentsViewModel>().ReverseMap();

            // Maps a SalaryRange entity to a GetSalaryRangesViewModel, and vice versa.
            CreateMap<SalaryRange, GetSalaryRangesViewModel>().ReverseMap();
            // Maps a CreatePositionCommand to a Position entity.
            CreateMap<CreatePositionCommand, Position>();
            // Maps a CreateDepartmentCommand to a Department entity.
            CreateMap<CreateDepartmentCommand, Department>();
            // Maps a CreateEmployeeCommand to an Employee entity.
            CreateMap<CreateEmployeeCommand, Employee>();
            // Maps a CreateSalaryRangeCommand to a SalaryRange entity.
            CreateMap<CreateSalaryRangeCommand, SalaryRange>();
        }
    }
}
