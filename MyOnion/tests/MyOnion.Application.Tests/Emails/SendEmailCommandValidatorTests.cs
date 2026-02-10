using FluentValidation.TestHelper;
using MyOnion.Application.Features.Emails.Commands.SendEmail;

namespace MyOnion.Application.Tests.Emails;

public class SendEmailCommandValidatorTests
{
    private readonly SendEmailCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidSingleRecipient_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "valid@example.com" },
            Subject = "Test Subject",
            Body = "<p>Test body</p>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyToList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string>(),
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.To)
            .WithErrorMessage("At least one recipient (To) is required.");
    }

    [Fact]
    public void Validate_WithInvalidEmailInTo_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "invalid-email" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("To[0]")
            .WithErrorMessage("'invalid-email' is not a valid email address.");
    }

    [Fact]
    public void Validate_WithMoreThan50ToRecipients_ShouldHaveValidationError()
    {
        // Arrange
        var toList = Enumerable.Range(1, 51)
            .Select(i => $"user{i}@example.com")
            .ToList();

        var command = new SendEmailCommand
        {
            To = toList,
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.To)
            .WithErrorMessage("Cannot send to more than 50 recipients.");
    }

    [Fact]
    public void Validate_WithMoreThan50CcRecipients_ShouldHaveValidationError()
    {
        // Arrange
        var ccList = Enumerable.Range(1, 51)
            .Select(i => $"cc{i}@example.com")
            .ToList();

        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Cc = ccList,
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cc)
            .WithErrorMessage("Cannot send to more than 50 CC recipients.");
    }

    [Fact]
    public void Validate_WithMoreThan50BccRecipients_ShouldHaveValidationError()
    {
        // Arrange
        var bccList = Enumerable.Range(1, 51)
            .Select(i => $"bcc{i}@example.com")
            .ToList();

        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Bcc = bccList,
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Bcc)
            .WithErrorMessage("Cannot send to more than 50 BCC recipients.");
    }

    [Fact]
    public void Validate_WithMoreThan100TotalRecipients_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = Enumerable.Range(1, 40).Select(i => $"to{i}@example.com").ToList(),
            Cc = Enumerable.Range(1, 40).Select(i => $"cc{i}@example.com").ToList(),
            Bcc = Enumerable.Range(1, 21).Select(i => $"bcc{i}@example.com").ToList(),
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Total number of recipients (To + CC + BCC) cannot exceed 100.");
    }

    [Fact]
    public void Validate_WithExactly100TotalRecipients_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = Enumerable.Range(1, 50).Select(i => $"to{i}@example.com").ToList(),
            Cc = Enumerable.Range(1, 30).Select(i => $"cc{i}@example.com").ToList(),
            Bcc = Enumerable.Range(1, 20).Select(i => $"bcc{i}@example.com").ToList(),
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidEmailInCc_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "valid@example.com" },
            Cc = new List<string> { "invalid-cc-email" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Cc[0]")
            .WithErrorMessage("'invalid-cc-email' is not a valid email address.");
    }

    [Fact]
    public void Validate_WithInvalidEmailInBcc_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "valid@example.com" },
            Bcc = new List<string> { "invalid-bcc-email" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Bcc[0]")
            .WithErrorMessage("'invalid-bcc-email' is not a valid email address.");
    }

    [Fact]
    public void Validate_WithEmptySubject_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject is required.");
    }

    [Fact]
    public void Validate_WithSubjectTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = new string('a', 201),
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_WithEmptyBody_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorMessage("Body is required.");
    }

    [Fact]
    public void Validate_WithBodyTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = new string('a', 50001)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorMessage("Body must not exceed 50,000 characters.");
    }

    [Fact]
    public void Validate_WithInvalidFromAddress_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>",
            From = "invalid-email"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.From)
            .WithErrorMessage("From must be a valid email address.");
    }

    [Fact]
    public void Validate_WithValidFromAddress_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>",
            From = "custom@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // 265 characters
        var command = new SendEmailCommand
        {
            To = new List<string> { longEmail },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("To[0]")
            .WithErrorMessage("Email address must not exceed 254 characters.");
    }

    [Fact]
    public void Validate_WithEmptyStringInToList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("To[0]")
            .WithErrorMessage("Recipient email address cannot be empty.");
    }

    [Fact]
    public void Validate_WithEmptyStringInCcList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "valid@example.com" },
            Cc = new List<string> { "" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Cc[0]")
            .WithErrorMessage("CC email address cannot be empty.");
    }

    [Fact]
    public void Validate_WithEmptyStringInBccList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "valid@example.com" },
            Bcc = new List<string> { "" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Bcc[0]")
            .WithErrorMessage("BCC email address cannot be empty.");
    }

    [Fact]
    public void Validate_WithMultipleValidRecipients_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "alice@example.com", "bob@example.com" },
            Cc = new List<string> { "manager@example.com" },
            Bcc = new List<string> { "audit@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
