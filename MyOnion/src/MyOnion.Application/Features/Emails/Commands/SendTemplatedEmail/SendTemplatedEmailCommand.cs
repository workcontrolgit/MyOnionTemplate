using Ganss.Xss;
using MyOnion.Application.Common.Results;
using MyOnion.Application.Exceptions;
using MyOnion.Application.Features.Emails.Commands.SendEmail;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Messaging;

namespace MyOnion.Application.Features.Emails.Commands.SendTemplatedEmail;

public class SendTemplatedEmailCommand : IRequest<Result<SendEmailResponse>>
{
    public string TemplateId { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public string From { get; set; }

    public class Handler : IRequestHandler<SendTemplatedEmailCommand, Result<SendEmailResponse>>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;
        private static readonly HtmlSanitizer _htmlSanitizer = CreateHtmlSanitizer();

        public Handler(IEmailService emailService, IEmailTemplateService templateService)
        {
            _emailService = emailService;
            _templateService = templateService;
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

        public async Task<Result<SendEmailResponse>> Handle(SendTemplatedEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Load template
                var template = await _templateService.GetTemplateAsync(request.TemplateId);
                if (template == null)
                {
                    return Result<SendEmailResponse>.Failure($"Email template '{request.TemplateId}' not found.");
                }

                // Validate required variables
                var missingVariables = _templateService.ValidateRequiredVariables(template, request.Variables);
                if (missingVariables.Any())
                {
                    return Result<SendEmailResponse>.Failure(
                        $"Missing required variables: {string.Join(", ", missingVariables)}");
                }

                // Render template with variables
                var (subject, body) = await _templateService.RenderTemplateAsync(template, request.Variables);

                // Sanitize HTML to prevent XSS attacks
                var sanitizedBody = _htmlSanitizer.Sanitize(body);

                var messageId = Guid.NewGuid().ToString();
                var sentAtUtc = DateTime.UtcNow;

                var emailRequest = new EmailRequest
                {
                    To = request.To,
                    Cc = request.Cc,
                    Bcc = request.Bcc,
                    Subject = subject,
                    Body = sanitizedBody,
                    From = request.From ?? template.From
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

                return Result<SendEmailResponse>.Success(response, $"Email sent using template '{template.Name}'");
            }
            catch (ApiException)
            {
                return Result<SendEmailResponse>.Failure("Failed to send email. Please try again later.");
            }
            catch (Exception ex)
            {
                return Result<SendEmailResponse>.Failure($"Email service error: {ex.Message}");
            }
        }
    }
}
