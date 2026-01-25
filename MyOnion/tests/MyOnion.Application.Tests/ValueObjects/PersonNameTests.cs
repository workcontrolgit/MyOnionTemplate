namespace MyOnion.Application.Tests.ValueObjects;

public class PersonNameTests
{
    [Fact]
    public void Constructor_ShouldNormalizeWhitespace()
    {
        var name = new PersonName("  Ada  ", "  Lovelace  ", "  Byron  ");

        name.FirstName.Should().Be("Ada");
        name.MiddleName.Should().Be("Lovelace");
        name.LastName.Should().Be("Byron");
    }

    [Fact]
    public void Constructor_ShouldAllowNullOrWhitespaceMiddleName()
    {
        var name = new PersonName("Ada", "   ", "Byron");

        name.MiddleName.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowWhenFirstOrLastMissing()
    {
        var missingFirst = () => new PersonName("  ", null, "Byron");
        var missingLast = () => new PersonName("Ada", null, "  ");

        missingFirst.Should().Throw<ArgumentException>();
        missingLast.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowWhenAnyPartTooLong()
    {
        var longValue = new string('a', 101);

        var act = () => new PersonName(longValue, null, "Byron");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FullName_ShouldIncludeMiddleNameWhenPresent()
    {
        var name = new PersonName("Ada", "Lovelace", "Byron");

        name.FullName.Should().Be("Ada Lovelace Byron");
    }

    [Fact]
    public void FullName_ShouldExcludeMiddleNameWhenMissing()
    {
        var name = new PersonName("Ada", null, "Byron");

        name.FullName.Should().Be("Ada Byron");
    }
}
