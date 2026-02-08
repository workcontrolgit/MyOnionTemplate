using MyOnion.Application.DTOs.Email;
using MyOnion.Application.Features.Emails.Commands.SendTemplatedEmail;

namespace MyOnion.Application.Tests.Emails;

public class SendTemplatedEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IEmailTemplateService> _templateServiceMock = new();

    [Fact]
    public async Task Handle_WithValidTemplate_ShouldSendEmailSuccessfully()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "welcome-user",
            Name = "Welcome User",
            SubjectTemplate = "Welcome, {{ first_name }}!",
            BodyTemplate = "<p>Hello {{ first_name }} {{ last_name }}, welcome to our platform!</p>",
            RequiredVariables = new List<string> { "first_name", "last_name" }
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "john.doe@example.com" },
            Variables = new Dictionary<string, object>
            {
                { "first_name", "John" },
                { "last_name", "Doe" }
            }
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("welcome-user"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Welcome, John!", "<p>Hello John Doe, welcome to our platform!</p>"));

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Welcome User");
        result.Value.Should().NotBeNull();
        result.Value.Sent.Should().BeTrue();
        result.Value.ToCount.Should().Be(1);
        result.Value.MessageId.Should().NotBeEmpty();

        _templateServiceMock.Verify(s => s.GetTemplateAsync("welcome-user"), Times.Once);
        _templateServiceMock.Verify(s => s.ValidateRequiredVariables(template, command.Variables), Times.Once);
        _templateServiceMock.Verify(s => s.RenderTemplateAsync(template, command.Variables), Times.Once);
        _emailServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTemplateNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "non-existent",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("non-existent"))
            .ReturnsAsync((EmailTemplate)null);

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Email template 'non-existent' not found");
        result.Value.Should().BeNull();

        _emailServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMissingRequiredVariables_ShouldReturnFailure()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "welcome-user",
            Name = "Welcome User",
            SubjectTemplate = "Welcome, {{ first_name }}!",
            BodyTemplate = "<p>Hello {{ first_name }} {{ last_name }}</p>",
            RequiredVariables = new List<string> { "first_name", "last_name" }
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "welcome-user",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>
            {
                { "first_name", "John" }
                // Missing last_name
            }
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("welcome-user"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string> { "last_name" });

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Missing required variables: last_name");
        result.Value.Should().BeNull();

        _emailServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleRecipients_ShouldCountCorrectly()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "monthly-report",
            Name = "Monthly Report",
            SubjectTemplate = "Monthly Report - {{ month }}",
            BodyTemplate = "<p>Report for {{ month }}</p>",
            RequiredVariables = new List<string> { "month" }
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "monthly-report",
            To = new List<string> { "alice@example.com", "bob@example.com" },
            Cc = new List<string> { "manager@example.com" },
            Bcc = new List<string> { "audit@example.com" },
            Variables = new Dictionary<string, object> { { "month", "January" } }
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("monthly-report"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Monthly Report - January", "<p>Report for January</p>"));

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ToCount.Should().Be(2);
        result.Value.CcCount.Should().Be(1);
        result.Value.BccCount.Should().Be(1);
        result.Value.TotalRecipients.Should().Be(4);
    }

    [Fact]
    public async Task Handle_ShouldSanitizeRenderedHtml()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "test-template",
            Name = "Test Template",
            SubjectTemplate = "Test",
            BodyTemplate = "{{ content }}",
            RequiredVariables = new List<string> { "content" }
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "test-template",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>
            {
                { "content", "<p>Safe</p><script>alert('XSS')</script>" }
            }
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("test-template"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Test", "<p>Safe</p><script>alert('XSS')</script>"));

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Body.Should().NotContain("<script>");
        capturedRequest.Body.Should().Contain("<p>Safe</p>");
    }

    [Fact]
    public async Task Handle_WithCustomFromAddress_ShouldUseIt()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "test-template",
            Name = "Test Template",
            SubjectTemplate = "Test",
            BodyTemplate = "<p>Test</p>",
            From = "template-default@example.com",
            RequiredVariables = new List<string>()
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "test-template",
            To = new List<string> { "test@example.com" },
            From = "custom@example.com",
            Variables = new Dictionary<string, object>()
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("test-template"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Test", "<p>Test</p>"));

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.From.Should().Be("custom@example.com");
    }

    [Fact]
    public async Task Handle_WithNoCustomFromAddress_ShouldUseTemplateDefault()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "test-template",
            Name = "Test Template",
            SubjectTemplate = "Test",
            BodyTemplate = "<p>Test</p>",
            From = "template-default@example.com",
            RequiredVariables = new List<string>()
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "test-template",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("test-template"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Test", "<p>Test</p>"));

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.From.Should().Be("template-default@example.com");
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsApiException_ShouldReturnFailure()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "test-template",
            Name = "Test Template",
            SubjectTemplate = "Test",
            BodyTemplate = "<p>Test</p>",
            RequiredVariables = new List<string>()
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "test-template",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("test-template"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Test", "<p>Test</p>"));

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .ThrowsAsync(new ApiException("SMTP error"));

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to send email. Please try again later.");
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsException_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = "test-template",
            Name = "Test Template",
            SubjectTemplate = "Test",
            BodyTemplate = "<p>Test</p>",
            RequiredVariables = new List<string>()
        };

        var command = new SendTemplatedEmailCommand
        {
            TemplateId = "test-template",
            To = new List<string> { "test@example.com" },
            Variables = new Dictionary<string, object>()
        };

        _templateServiceMock
            .Setup(s => s.GetTemplateAsync("test-template"))
            .ReturnsAsync(template);

        _templateServiceMock
            .Setup(s => s.ValidateRequiredVariables(template, command.Variables))
            .Returns(new List<string>());

        _templateServiceMock
            .Setup(s => s.RenderTemplateAsync(template, command.Variables))
            .ReturnsAsync(("Test", "<p>Test</p>"));

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .ThrowsAsync(new Exception("Network error"));

        var handler = new SendTemplatedEmailCommand.Handler(_emailServiceMock.Object, _templateServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Email service error");
        result.Message.Should().Contain("Network error");
        result.Value.Should().BeNull();
    }
}
