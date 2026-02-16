using Mapster;
using MyOnion.Application.Mappings;

namespace MyOnion.Application.Tests.Mappings
{
    public class GeneralProfileCreateEmployeeMappingTests
    {
        [Fact]
        public void CreateEmployeeCommand_ShouldMapToEmployee_WithPersonNameAndCoreFields()
        {
            var config = new TypeAdapterConfig();
            new GeneralProfile().Register(config);
            var mapper = new Mapper(config);

            var command = new CreateEmployeeCommand
            {
                FirstName = "Jane",
                MiddleName = "Q",
                LastName = "Doe",
                PositionId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Salary = 1000,
                Birthday = DateTime.UtcNow.AddYears(-30),
                Email = "jane@example.com",
                Gender = MyOnion.Domain.Enums.Gender.Female,
                EmployeeNumber = "EMP-1",
                Prefix = "Ms.",
                Phone = "123"
            };

            var employee = mapper.Map<Employee>(command);

            employee.Name.FirstName.Should().Be("Jane");
            employee.Name.MiddleName.Should().Be("Q");
            employee.Name.LastName.Should().Be("Doe");
            employee.PositionId.Should().Be(command.PositionId);
            employee.DepartmentId.Should().Be(command.DepartmentId);
            employee.EmployeeNumber.Should().Be("EMP-1");
        }
    }
}
