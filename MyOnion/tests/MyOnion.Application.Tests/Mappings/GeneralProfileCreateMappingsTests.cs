using Mapster;
using MyOnion.Application.Mappings;

namespace MyOnion.Application.Tests.Mappings
{
    public class GeneralProfileCreateMappingsTests
    {
        private static IMapper CreateMapper()
        {
            var config = new TypeAdapterConfig();
            new GeneralProfile().Register(config);
            return new Mapper(config);
        }

        [Fact]
        public void CreatePositionCommand_ShouldMapToPosition()
        {
            var mapper = CreateMapper();
            var command = new CreatePositionCommand
            {
                PositionTitle = "Developer",
                PositionNumber = "POS-100",
                PositionDescription = "Builds features",
                DepartmentId = Guid.NewGuid(),
                SalaryRangeId = Guid.NewGuid()
            };

            var position = mapper.Map<Position>(command);

            position.PositionTitle.Value.Should().Be("Developer");
            position.PositionNumber.Should().Be("POS-100");
            position.PositionDescription.Should().Be("Builds features");
            position.DepartmentId.Should().Be(command.DepartmentId);
            position.SalaryRangeId.Should().Be(command.SalaryRangeId);
        }

        [Fact]
        public void CreateDepartmentCommand_ShouldMapToDepartment()
        {
            var mapper = CreateMapper();
            var command = new CreateDepartmentCommand { Name = "Engineering" };

            var department = mapper.Map<Department>(command);

            department.Name.Value.Should().Be("Engineering");
        }

        [Fact]
        public void CreateSalaryRangeCommand_ShouldMapToSalaryRange()
        {
            var mapper = CreateMapper();
            var command = new CreateSalaryRangeCommand
            {
                Name = "Senior",
                MinSalary = 5000,
                MaxSalary = 9000
            };

            var salaryRange = mapper.Map<SalaryRange>(command);

            salaryRange.Name.Should().Be("Senior");
            salaryRange.MinSalary.Should().Be(5000);
            salaryRange.MaxSalary.Should().Be(9000);
        }
    }
}
