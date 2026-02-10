# Implementation Plan: Email Controller for Angular Integration

## Context

The MyOnion API currently has email infrastructure in place (`IEmailService` using MailKit) but lacks an HTTP endpoint for clients to send emails. This implementation will add a REST API endpoint that Angular frontend applications can call to send emails through the backend's SMTP configuration.

**Problem Solved:**
- Frontend applications need to send emails (contact forms, notifications, etc.) without exposing SMTP credentials
- Email sending should be authenticated and validated server-side
- Need to provide structured feedback to frontend about email send success/failure

**Architecture Alignment:**
This implementation follows MyOnion's established CQRS pattern with Command + Handler + Validator, consistent with existing features like Employees, Departments, and Positions.

## SMTP Configuration Review

**Current Infrastructure:**
- **MailKit 4.14.1** and **MimeKit 4.14.0** for SMTP operations
- **Configuration:** `appsettings.json` → `MailSettings` section
- **SMTP Settings:** SmtpHost (smtp.ethereal.email), SmtpPort (587), SmtpUser, SmtpPass, EmailFrom, DisplayName
- **Connection:** Uses `SecureSocketOptions.StartTls` encryption
- **Service Registration:** Transient lifetime in `Infrastructure.Shared/ServiceRegistration.cs`
- **Implementation:** `Infrastructure.Shared/Services/EmailService.cs` with `IEmailService` interface

**Current Limitations:**
- `IEmailService.SendAsync()` returns void Task (no success/failure indication)
- Throws `ApiException` on SMTP errors
- No validation on `EmailRequest` DTO
- Single recipient only (no CC/BCC)
- HTML body only

## Implementation Design

### Files to Create

```
MyOnion.Application/Features/Emails/Commands/SendEmail/
├── SendEmailCommand.cs         (Command + nested Handler)
├── SendEmailCommandValidator.cs (FluentValidation rules)
└── SendEmailResponse.cs        (Response DTO)

MyOnion.WebApi/Controllers/v1/
└── EmailsController.cs         (REST endpoint)
```

### 1. SendEmailResponse DTO

**Purpose:** Structured response for email send operation

**Properties:**
```csharp
public class SendEmailResponse
{
    public bool Sent { get; set; }
    public string MessageId { get; set; }  // Guid for tracking
    public DateTime SentAtUtc { get; set; }
}
```

### 2. SendEmailCommand + Handler

**Command Properties:**
```csharp
public class SendEmailCommand : IRequest<Result<SendEmailResponse>>
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }      // HTML content
    public string From { get; set; }      // Optional, falls back to config
}
```

**Handler Dependencies:**
- `IEmailService` - Existing email service from Infrastructure.Shared
- `ILogger<SendEmailCommandHandler>` - For error logging

**Handler Logic:**
1. Create `EmailRequest` from command properties
2. Generate `MessageId` (Guid) and `SentAtUtc` timestamp
3. Call `await _emailService.SendAsync(emailRequest)`
4. Wrap in try-catch:
   - Success: Return `Result<SendEmailResponse>.Success(response, "Email sent successfully")`
   - `ApiException`: Log details, return `Result.Failure("Failed to send email. Please try again later.")`
   - Generic `Exception`: Log error, return `Result.Failure("Email service is currently unavailable.")`

**Error Handling Strategy:**
- Catch `ApiException` (thrown by EmailService) and convert to Result.Failure with user-friendly message
- Log detailed exceptions server-side but return sanitized messages to prevent exposing SMTP credentials
- Return 200 OK with `Result.IsSuccess = false` for SMTP failures (not throwing exceptions)

### 3. SendEmailCommandValidator

**Validation Rules:**
```csharp
RuleFor(x => x.To)
    .NotEmpty().WithMessage("{PropertyName} is required.")
    .EmailAddress().WithMessage("{PropertyName} must be a valid email address.")
    .MaximumLength(254).WithMessage("{PropertyName} must not exceed 254 characters.");

RuleFor(x => x.Subject)
    .NotEmpty().WithMessage("{PropertyName} is required.")
    .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

RuleFor(x => x.Body)
    .NotEmpty().WithMessage("{PropertyName} is required.")
    .MaximumLength(50000).WithMessage("{PropertyName} must not exceed 50,000 characters.");

// Conditional validation for optional From field
When(x => !string.IsNullOrWhiteSpace(x.From), () =>
{
    RuleFor(x => x.From)
        .EmailAddress()
        .MaximumLength(254);
});
```

**Rationale:**
- Email format validation prevents SMTP errors
- Length limits prevent abuse (RFC 5321 specifies 254 char max for emails)
- Body limit (50K chars) prevents oversized HTML emails
- From is optional; if not provided, EmailService uses `MailSettings.EmailFrom` as default

### 4. EmailsController

**Endpoint Design:**
```csharp
[ApiVersion("1.0")]
public class EmailsController : BaseApiController
{
    [HttpPost]
    [Authorize]  // Requires authenticated user
    [ProducesResponseType(typeof(Result<SendEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Send(SendEmailCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
```

**Route:** `POST /api/v1/emails`

**Authorization:** `[Authorize]` - requires authenticated user (no specific role requirement)

**Response Behavior:**
- Always returns 200 OK when operation completes without throwing exception
- `Result.IsSuccess` property indicates actual email send outcome
- Validation errors return 400 BadRequest (handled by ValidationBehavior + ErrorHandlerMiddleware)

## Critical Files to Reference

**Pattern Templates:**
1. `MyOnion.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeCommand.cs`
   - Follow this pattern for Command structure with nested Handler class
   - Shows dependency injection pattern (repository, mapper, services)

2. `MyOnion.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeCommandValidator.cs`
   - Follow this pattern for FluentValidation rules
   - Shows email validation, required fields, max length validation

3. `MyOnion.WebApi/Controllers/v1/EmployeesController.cs`
   - Follow this pattern for controller structure
   - Shows proper use of `[Authorize]`, `[ProducesResponseType]`, and result handling

**Integration Points:**
4. `MyOnion.Infrastructure.Shared/Services/EmailService.cs`
   - Existing email service implementation to integrate with
   - Important: Throws `ApiException` on SMTP failures (line 59)
   - Uses `EmailRequest` DTO with To, Subject, Body, From properties

5. `MyOnion.Application/Interfaces/IEmailService.cs`
   - Interface to inject into handler: `Task SendAsync(EmailRequest request)`

6. `MyOnion.Domain/Settings/MailSettings.cs`
   - Configuration class for SMTP settings
   - Loaded from appsettings.json "MailSettings" section

## Security Considerations

**Authentication Required:**
- `[Authorize]` attribute prevents anonymous email sending
- Requires valid JWT token from frontend

**Error Message Sanitization:**
- SMTP errors logged with full details server-side
- Generic messages returned to client to prevent credential exposure
- Configuration errors don't expose MailSettings values

**Input Validation:**
- Email format validation prevents header injection
- Length limits prevent abuse/DoS
- MailKit library sanitizes headers automatically

**Rate Limiting (Future Enhancement):**
- Consider adding rate limiter: 5-10 emails per minute per authenticated user
- Prevents abuse by compromised accounts

**From Field Security:**
- Optional From field allows custom sender
- **Recommendation:** Consider locking down From to only use `MailSettings.EmailFrom` or whitelist approved senders to prevent spoofing

## Angular Integration Example

**TypeScript Service:**
```typescript
export interface SendEmailRequest {
  to: string;
  subject: string;
  body: string;
  from?: string;
}

export interface SendEmailResponse {
  sent: boolean;
  messageId: string;
  sentAtUtc: Date;
}

sendEmail(request: SendEmailRequest): Observable<Result<SendEmailResponse>> {
  return this.http.post<Result<SendEmailResponse>>(
    `${this.apiUrl}/api/v1/emails`,
    request
  );
}
```

**Component Usage:**
```typescript
this.emailService.sendEmail({
  to: 'support@example.com',
  subject: 'Contact Form Submission',
  body: '<p>User message here</p>'
}).subscribe({
  next: (result) => {
    if (result.isSuccess) {
      this.showSuccess('Email sent!');
    } else {
      this.showError(result.message);
    }
  },
  error: (err) => {
    if (err.status === 401) {
      this.redirectToLogin();
    } else if (err.status === 400) {
      this.showValidationErrors(err.error.errors);
    }
  }
});
```

## Response Examples

**Success (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Email sent successfully",
  "errors": [],
  "value": {
    "sent": true,
    "messageId": "a1b2c3d4-e5f6-4a5b-8c7d-9e8f7a6b5c4d",
    "sentAtUtc": "2026-02-07T15:30:00Z"
  },
  "executionTimeMs": 1250.5
}
```

**Validation Error (400 Bad Request):**
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "To must be a valid email address.",
    "Subject is required."
  ],
  "value": null
}
```

**SMTP Error (200 OK with IsSuccess = false):**
```json
{
  "isSuccess": false,
  "message": "Failed to send email. Please try again later.",
  "errors": ["SMTP connection timeout"],
  "value": null
}
```

## Testing and Verification

### Manual Testing via Swagger

1. **Start API:** `dotnet watch run --project MyOnion/src/MyOnion.WebApi/MyOnion.WebApi.csproj`
2. **Navigate to Swagger:** `https://localhost:5001/swagger`
3. **Authenticate:** Click "Authorize" button, enter JWT token
4. **Test POST /api/v1/emails:**
   ```json
   {
     "to": "recipient@ethereal.email",
     "subject": "Test Email",
     "body": "<p>This is a test</p>",
     "from": null
   }
   ```
5. **Verify Response:** Check for `isSuccess: true` and valid messageId
6. **Check Ethereal Inbox:** Verify email delivery at https://ethereal.email

### Validation Testing

- Submit with invalid email format → Expect 400 with validation errors
- Submit without subject → Expect 400 "Subject is required"
- Submit without authentication → Expect 401 Unauthorized
- Submit with oversized body (>50K chars) → Expect 400 validation error

### Error Testing

- Temporarily misconfigure SMTP in appsettings.json → Expect 200 with `isSuccess: false` and user-friendly error
- Check application logs for detailed exception information

### Unit Tests (Future)

Create tests in `MyOnion.Application.UnitTests/Features/Emails/Commands/SendEmail/`:
- `SendEmailCommandValidatorTests.cs` - Validate all validation rules
- `SendEmailCommandHandlerTests.cs` - Mock IEmailService, test success/failure paths

## Known Limitations and Future Enhancements

**Current Limitations:**
- Single recipient only (no CC/BCC)
- No attachment support
- No email templates
- Synchronous operation (blocks HTTP request until SMTP completes)
- No rate limiting

**Future Enhancements:**
1. **Rate limiting** - Prevent abuse (5-10 emails/minute per user)
2. **HTML sanitization** - Strip dangerous tags/scripts from body
3. **Email queue** - Background processing to avoid blocking HTTP requests
4. **Multiple recipients** - Support List<string> for To, CC, BCC
5. **Email templates** - Standardized emails with variable substitution
6. **Attachments** - File upload with virus scanning
7. **Audit trail** - Store sent emails in database for compliance

## Implementation Steps

1. ✅ **Create SendEmailResponse.cs** - DTO with Sent, MessageId, SentAtUtc properties
2. ✅ **Create SendEmailCommand.cs** - Command with nested Handler, inject IEmailService and ILogger
3. ✅ **Create SendEmailCommandValidator.cs** - FluentValidation rules for all fields
4. ✅ **Create EmailsController.cs** - POST endpoint with [Authorize] attribute
5. **Test with Swagger** - Verify authentication, validation, successful send, error handling
6. **Document for frontend** - Provide Angular team with request/response examples
7. **Add unit tests** (optional) - Validator tests and handler tests with mocked IEmailService

## Rollout Checklist

- [ ] Verify MailSettings configured in appsettings.Development.json (use Ethereal Email for testing)
- [x] Implement all four files (Response, Command, Validator, Controller)
- [ ] Test authentication requirement (401 without token)
- [ ] Test validation errors (invalid email, missing fields)
- [ ] Test successful email send (verify in Ethereal inbox)
- [ ] Test SMTP error handling (misconfigure settings temporarily)
- [ ] Document endpoint for Angular team with request/response examples
- [ ] Consider adding rate limiting for production deployment
- [ ] Review From field security - consider locking down to prevent spoofing

## Notes

- **Validation happens automatically** via ValidationBehavior pipeline behavior - no need to manually invoke validator in handler
- **EmailService is Transient** - new instance per request, creates new SMTP connection each time (acceptable for low volume; consider connection pooling for high volume)
- **MessageId generated optimistically** - created before send, so ID exists even if send fails (consider generating after successful send if this is problematic)
- **Authorization policy flexible** - currently allows any authenticated user; tighten to Admin role if needed with `[Authorize(Policy = AuthorizationConsts.AdminPolicy)]`
