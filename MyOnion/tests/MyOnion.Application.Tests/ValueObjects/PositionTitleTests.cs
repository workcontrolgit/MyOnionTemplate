namespace MyOnion.Application.Tests.ValueObjects;

public class PositionTitleTests
{
    [Fact]
    public void Constructor_ShouldNormalizeWhitespace()
    {
        var title = new PositionTitle("  Senior   Engineer  ");

        title.Value.Should().Be("Senior Engineer");
    }

    [Fact]
    public void Constructor_ShouldThrowOnEmpty()
    {
        var act = () => new PositionTitle("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowOnTooLong()
    {
        var act = () => new PositionTitle(new string('a', 251));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_ShouldBeCaseInsensitive()
    {
        var left = new PositionTitle("Developer");
        var right = new PositionTitle("developer");

        left.Equals(right).Should().BeTrue();
    }
}
