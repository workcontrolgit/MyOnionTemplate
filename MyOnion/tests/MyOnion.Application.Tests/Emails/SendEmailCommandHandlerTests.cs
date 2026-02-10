using Ganss.Xss;
using MyOnion.Application.Features.Emails.Commands.SendEmail;

namespace MyOnion.Application.Tests.Emails;

public class SendEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock = new();

    [Fact]
    public async Task Handle_WithValidSingleRecipient_ShouldSendEmailSuccessfully()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "john.doe@example.com" },
            Subject = "Test Subject",
            Body = "<p>Test body</p>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Email sent successfully");
        result.Value.Should().NotBeNull();
        result.Value.Sent.Should().BeTrue();
        result.Value.ToCount.Should().Be(1);
        result.Value.CcCount.Should().Be(0);
        result.Value.BccCount.Should().Be(0);
        result.Value.TotalRecipients.Should().Be(1);
        result.Value.MessageId.Should().NotBeEmpty();
        result.Value.SentAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        _emailServiceMock.Verify(s => s.SendAsync(It.Is<EmailRequest>(r =>
            r.To.Count == 1 &&
            r.To[0] == "john.doe@example.com" &&
            r.Subject == "Test Subject"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleRecipients_ShouldCountCorrectly()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "alice@example.com", "bob@example.com" },
            Cc = new List<string> { "manager@example.com" },
            Bcc = new List<string> { "audit@example.com", "compliance@example.com" },
            Subject = "Team Meeting",
            Body = "<p>Meeting notes</p>"
        };

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ToCount.Should().Be(2);
        result.Value.CcCount.Should().Be(1);
        result.Value.BccCount.Should().Be(2);
        result.Value.TotalRecipients.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldSanitizeHtml_RemovingScriptTags()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Safe content</p><script>alert('XSS')</script><p>More safe content</p>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Body.Should().NotContain("<script>");
        capturedRequest.Body.Should().NotContain("alert");
        capturedRequest.Body.Should().Contain("<p>Safe content</p>");
        capturedRequest.Body.Should().Contain("<p>More safe content</p>");
    }

    [Fact]
    public async Task Handle_ShouldSanitizeHtml_RemovingJavascriptUrls()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<a href='javascript:void(0)'>Bad link</a><a href='https://safe.com'>Good link</a>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Body.Should().NotContain("javascript:");
        capturedRequest.Body.Should().Contain("https://safe.com");
    }

    [Fact]
    public async Task Handle_ShouldSanitizeHtml_RemovingDangerousTags()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Safe</p><iframe src='bad.com'></iframe><object data='bad.com'></object><embed src='bad.com'>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Body.Should().NotContain("<iframe");
        capturedRequest.Body.Should().NotContain("<object");
        capturedRequest.Body.Should().NotContain("<embed");
        capturedRequest.Body.Should().Contain("<p>Safe</p>");
    }

    [Fact]
    public async Task Handle_ShouldPreserveAllowedHtmlTags()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<h1>Title</h1><p>Paragraph with <strong>bold</strong> and <em>italic</em></p><ul><li>Item 1</li></ul>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Body.Should().Contain("<h1>Title</h1>");
        capturedRequest.Body.Should().Contain("<strong>bold</strong>");
        capturedRequest.Body.Should().Contain("<em>italic</em>");
        capturedRequest.Body.Should().Contain("<ul>");
        capturedRequest.Body.Should().Contain("<li>Item 1</li>");
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsApiException_ShouldReturnFailure()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .ThrowsAsync(new ApiException("SMTP connection failed"));

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to send email. Please try again later.");
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsException_ShouldReturnGenericFailure()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email service is currently unavailable.");
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCustomFromAddress_ShouldPassThroughToService()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>",
            From = "custom@example.com",
            Cc = new List<string>(),
            Bcc = new List<string>()
        };

        EmailRequest? capturedRequest = null;
        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Callback<EmailRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.From.Should().Be("custom@example.com");
    }

    [Fact]
    public async Task Handle_WithNullCcAndBcc_ShouldHandleGracefully()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            To = new List<string> { "test@example.com" },
            Subject = "Test",
            Body = "<p>Test</p>",
            Cc = null,
            Bcc = null
        };

        _emailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        var handler = new SendEmailCommand.Handler(_emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CcCount.Should().Be(0);
        result.Value.BccCount.Should().Be(0);
        result.Value.TotalRecipients.Should().Be(1);
    }
}
