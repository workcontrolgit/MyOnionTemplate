using FluentValidation.TestHelper;
using MyOnion.Application.Features.Emails.Commands.SendTemplatedEmail;

namespace MyOnion.Application.Tests.Emails;

public class SendTemplatedEmailCommandValidatorTests
{
    private readonly SendTemplatedEmailCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>
            {
                { "name", "John" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTemplateId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage("Template Id is required.");
    }

    [Fact]
    public void Validate_WithTemplateIdTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = new string('a', 101),
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage("Template Id must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_WithEmptyToList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string>(),
            Variables = new Dictionary<string, object>()
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
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "invalid-email" },
            Variables = new Dictionary<string, object>()
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

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "newsletter",
            To = toList,
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.To)
            .WithErrorMessage("Cannot send to more than 50 recipients.");
    }

    [Fact]
    public void Validate_WithMoreThan100TotalRecipients_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "newsletter",
            To = Enumerable.Range(1, 40).Select(i => $"to{i}@example.com").ToList(),
            Cc = Enumerable.Range(1, 40).Select(i => $"cc{i}@example.com").ToList(),
            Bcc = Enumerable.Range(1, 21).Select(i => $"bcc{i}@example.com").ToList(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Total number of recipients (To + CC + BCC) cannot exceed 100.");
    }

    [Fact]
    public void Validate_WithInvalidFromAddress_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "test@example.com" },
            From = "invalid-email",
            Variables = new Dictionary<string, object>()
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
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "test@example.com" },
            From = "custom@example.com",
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullVariables_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "test@example.com" },
            Variables = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Variables)
            .WithErrorMessage("Variables cannot be null.");
    }

    [Fact]
    public void Validate_WithEmptyVariablesDictionary_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidCcEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "valid@example.com" },
            Cc = new List<string> { "invalid-cc" },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Cc[0]")
            .WithErrorMessage("'invalid-cc' is not a valid email address.");
    }

    [Fact]
    public void Validate_WithInvalidBccEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "valid@example.com" },
            Bcc = new List<string> { "invalid-bcc" },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Bcc[0]")
            .WithErrorMessage("'invalid-bcc' is not a valid email address.");
    }

    [Fact]
    public void Validate_WithMultipleValidRecipients_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "monthly-report",
            To = new List<string> { "alice@example.com", "bob@example.com" },
            Cc = new List<string> { "manager@example.com" },
            Bcc = new List<string> { "audit@example.com" },
            Variables = new Dictionary<string, object>
            {
                { "month", "January" },
                { "year", 2026 }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
