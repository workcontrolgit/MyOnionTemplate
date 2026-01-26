namespace MyOnion.Application.Tests.Specifications;

public class SpecificationOrderByTests
{
    [Fact]
    public void DepartmentsByFiltersSpecification_DefaultsToNameValueOrder()
    {
        var spec = new DepartmentsByFiltersSpecification(new GetDepartmentsQuery());

        spec.OrderBy.Should().Be("Name.Value");
    }

    [Fact]
    public void DepartmentsByFiltersSpecification_MapsNameOrder()
    {
        var spec = new DepartmentsByFiltersSpecification(new GetDepartmentsQuery { OrderBy = "Name" });

        spec.OrderBy.Should().Be("Name.Value");
    }

    [Fact]
    public void PositionsByFiltersSpecification_DefaultsToPositionNumber()
    {
        var spec = new PositionsByFiltersSpecification(new GetPositionsQuery());

        spec.OrderBy.Should().Be("PositionNumber");
    }

    [Fact]
    public void PositionsByFiltersSpecification_MapsPositionTitleOrder()
    {
        var spec = new PositionsByFiltersSpecification(new GetPositionsQuery { OrderBy = "PositionTitle" });

        spec.OrderBy.Should().Be("PositionTitle.Value");
    }

    [Fact]
    public void PositionsByFiltersSpecification_MapsDepartmentOrder()
    {
        var spec = new PositionsByFiltersSpecification(new GetPositionsQuery { OrderBy = "Department" });

        spec.OrderBy.Should().Be("Department.Name.Value");
    }
}
