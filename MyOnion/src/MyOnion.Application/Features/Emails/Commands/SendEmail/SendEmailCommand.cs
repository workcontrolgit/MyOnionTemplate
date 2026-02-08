using Ganss.Xss;
using MyOnion.Application.Common.Results;
using MyOnion.Application.Exceptions;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Messaging;

namespace MyOnion.Application.Features.Emails.Commands.SendEmail;

public class SendEmailCommand : IRequest<Result<SendEmailResponse>>
{
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public string Subject { get; set; }
    public string Body { get; set; }
    public string From { get; set; }

    public class Handler : IRequestHandler<SendEmailCommand, Result<SendEmailResponse>>
    {
        private readonly IEmailService _emailService;
        private static readonly HtmlSanitizer _htmlSanitizer = CreateHtmlSanitizer();

        public Handler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        private static HtmlSanitizer CreateHtmlSanitizer()
        {
            var sanitizer = new HtmlSanitizer();

            // Allow common safe HTML tags for email formatting
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.UnionWith(new[]
            {
                "p", "br", "strong", "b", "em", "i", "u", "a", "ul", "ol", "li",
                "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "code", "pre",
                "span", "div", "table", "thead", "tbody", "tr", "th", "td"
            });

            // Allow safe attributes
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.UnionWith(new[]
            {
                "href", "title", "alt", "class", "style"
            });

            // Allow safe CSS properties for styling
            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedCssProperties.UnionWith(new[]
            {
                "color", "background-color", "font-size", "font-weight", "text-align",
                "padding", "margin", "border", "width", "height"
            });

            // Allow only http/https URLs
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.UnionWith(new[] { "http", "https", "mailto" });

            return sanitizer;
        }

        public async Task<Result<SendEmailResponse>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var messageId = Guid.NewGuid().ToString();
                var sentAtUtc = DateTime.UtcNow;

                // Sanitize HTML to prevent XSS attacks
                var sanitizedBody = _htmlSanitizer.Sanitize(request.Body);

                var emailRequest = new EmailRequest
                {
                    To = request.To,
                    Cc = request.Cc,
                    Bcc = request.Bcc,
                    Subject = request.Subject,
                    Body = sanitizedBody,
                    From = request.From
                };

                await _emailService.SendAsync(emailRequest);

                var toCount = request.To?.Count ?? 0;
                var ccCount = request.Cc?.Count ?? 0;
                var bccCount = request.Bcc?.Count ?? 0;

                var response = new SendEmailResponse
                {
                    Sent = true,
                    MessageId = messageId,
                    SentAtUtc = sentAtUtc,
                    ToCount = toCount,
                    CcCount = ccCount,
                    BccCount = bccCount,
                    TotalRecipients = toCount + ccCount + bccCount
                };

                return Result<SendEmailResponse>.Success(response, "Email sent successfully");
            }
            catch (ApiException)
            {
                return Result<SendEmailResponse>.Failure("Failed to send email. Please try again later.");
            }
            catch (Exception)
            {
                return Result<SendEmailResponse>.Failure("Email service is currently unavailable.");
            }
        }
    }
}
