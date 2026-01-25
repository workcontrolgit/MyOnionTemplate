namespace MyOnion.Application.Tests.ValueObjects;

public class DepartmentNameTests
{
    [Fact]
    public void Constructor_ShouldNormalizeWhitespace()
    {
        var name = new DepartmentName("  Human   Resources  ");

        name.Value.Should().Be("Human Resources");
    }

    [Fact]
    public void Constructor_ShouldThrowOnEmpty()
    {
        var act = () => new DepartmentName("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowOnTooLong()
    {
        var act = () => new DepartmentName(new string('a', 251));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_ShouldBeCaseInsensitive()
    {
        var left = new DepartmentName("Finance");
        var right = new DepartmentName("finance");

        left.Equals(right).Should().BeTrue();
    }
}
