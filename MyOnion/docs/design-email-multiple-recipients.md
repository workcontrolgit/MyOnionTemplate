# Email Multiple Recipients Feature

## Overview

This document describes the implementation of multiple recipients support for the email API, including To, CC (Carbon Copy), and BCC (Blind Carbon Copy) functionality with HTML sanitization for security.

## Features Implemented

### 1. Multiple Recipients Support
- **To** - Primary recipients (required, 1-50 addresses)
- **CC** - Carbon copy recipients (optional, 0-50 addresses)
- **BCC** - Blind carbon copy recipients (optional, 0-50 addresses)
- **Total Limit** - Maximum 100 total recipients across all fields

### 2. HTML Sanitization
- **XSS Prevention** - Strips dangerous tags (`<script>`, `<iframe>`, `<object>`)
- **Safe Formatting** - Preserves legitimate HTML (paragraphs, links, lists, tables)
- **Attribute Filtering** - Only allows safe attributes (href, title, class, style)
- **URL Scheme Validation** - Restricts to http, https, and mailto schemes

### 3. Enhanced Validation
- Email format validation for all recipients
- Per-field recipient count limits (max 50 each)
- Total recipient count validation (max 100 combined)
- Empty email address detection
- RFC 5321 compliance (254 character email limit)

## Files Modified

### Application Layer

**MyOnion.Application/DTOs/Email/EmailRequest.cs**
- Changed `To` from `string` to `List<string>`
- Added `Cc` and `Bcc` as `List<string>` properties
- Initialized all lists with `new()` to prevent null reference issues

**MyOnion.Application/Features/Emails/Commands/SendEmail/SendEmailCommand.cs**
- Updated command properties to support lists
- Integrated HtmlSanitizer for XSS prevention
- Added recipient count calculation
- Removed logger dependency (follows Application layer pattern)
- Changed method name from `HandleAsync` to `Handle` (matches interface)

**MyOnion.Application/Features/Emails/Commands/SendEmail/SendEmailCommandValidator.cs**
- Added `RuleForEach` validation for each email in To, CC, BCC lists
- Added per-field recipient count limits (max 50)
- Added total recipient count validation (max 100)
- Conditional validation for optional CC and BCC fields

**MyOnion.Application/Features/Emails/Commands/SendEmail/SendEmailResponse.cs**
- Added `ToCount`, `CcCount`, `BccCount` properties
- Added `TotalRecipients` property for total count

### Infrastructure Layer

**MyOnion.Infrastructure.Shared/Services/EmailService.cs**
- Updated to iterate through To, CC, and BCC lists
- Uses MailKit's `email.To.Add()`, `email.Cc.Add()`, `email.Bcc.Add()` methods
- Maintains backward compatibility with existing MailSettings

### Presentation Layer

**MyOnion.WebApi/Controllers/v1/EmailsController.cs**
- Updated XML documentation to reflect multiple recipients capability

## Package Dependencies

**HtmlSanitizer 9.0.892** (added to MyOnion.Application)
- NuGet: https://www.nuget.org/packages/HtmlSanitizer/
- GitHub: https://github.com/mganss/HtmlSanitizer
- Uses AngleSharp for HTML parsing
- Thread-safe for concurrent requests

## API Request/Response Examples

### Example 1: Single Recipient

**Request:**
```json
POST /api/v1/emails
Authorization: Bearer {token}

{
  "to": ["john.doe@example.com"],
  "subject": "Welcome Email",
  "body": "<p>Welcome to our platform, <strong>John</strong>!</p>",
  "cc": [],
  "bcc": [],
  "from": null
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Email sent successfully",
  "errors": [],
  "value": {
    "sent": true,
    "messageId": "a1b2c3d4-e5f6-4a5b-8c7d-9e8f7a6b5c4d",
    "sentAtUtc": "2026-02-07T20:30:00Z",
    "toCount": 1,
    "ccCount": 0,
    "bccCount": 0,
    "totalRecipients": 1
  },
  "executionTimeMs": 1250.5
}
```

### Example 2: Multiple Recipients with CC and BCC

**Request:**
```json
{
  "to": [
    "alice@example.com",
    "bob@example.com"
  ],
  "cc": [
    "manager@example.com"
  ],
  "bcc": [
    "audit@example.com",
    "compliance@example.com"
  ],
  "subject": "Team Meeting Notes",
  "body": "<h2>Meeting Summary</h2><ul><li>Item 1</li><li>Item 2</li></ul>",
  "from": null
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Email sent successfully",
  "errors": [],
  "value": {
    "sent": true,
    "messageId": "f7e8d9c0-b1a2-3c4d-5e6f-7a8b9c0d1e2f",
    "sentAtUtc": "2026-02-07T20:35:00Z",
    "toCount": 2,
    "ccCount": 1,
    "bccCount": 2,
    "totalRecipients": 5
  },
  "executionTimeMs": 1450.3
}
```

### Example 3: HTML Sanitization

**Request with Malicious Content:**
```json
{
  "to": ["test@example.com"],
  "subject": "Test",
  "body": "<p>Safe content</p><script>alert('XSS')</script><a href='javascript:void(0)'>Bad link</a>",
  "cc": [],
  "bcc": [],
  "from": null
}
```

**Actual Email Body Sent (sanitized):**
```html
<p>Safe content</p>


<a>Bad link</a>
```

The `<script>` tag and `javascript:` URL are completely removed.

### Example 4: Validation Error - Invalid Email

**Request:**
```json
{
  "to": ["invalid-email", "valid@example.com"],
  "subject": "Test",
  "body": "<p>Test</p>",
  "cc": [],
  "bcc": []
}
```

**Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "'invalid-email' is not a valid email address."
  ],
  "value": null
}
```

### Example 5: Validation Error - Too Many Recipients

**Request:**
```json
{
  "to": ["user1@example.com", "user2@example.com", ... 51 emails total],
  "subject": "Bulk Email",
  "body": "<p>Message</p>",
  "cc": [],
  "bcc": []
}
```

**Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "Cannot send to more than 50 recipients."
  ],
  "value": null
}
```

## Angular/TypeScript Integration

### TypeScript Interfaces

```typescript
export interface SendEmailRequest {
  to: string[];
  cc?: string[];
  bcc?: string[];
  subject: string;
  body: string;
  from?: string;
}

export interface SendEmailResponse {
  sent: boolean;
  messageId: string;
  sentAtUtc: Date;
  toCount: number;
  ccCount: number;
  bccCount: number;
  totalRecipients: number;
}
```

### Angular Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EmailService {
  private apiUrl = 'https://localhost:5001';

  constructor(private http: HttpClient) {}

  sendEmail(request: SendEmailRequest): Observable<Result<SendEmailResponse>> {
    return this.http.post<Result<SendEmailResponse>>(
      `${this.apiUrl}/api/v1/emails`,
      request
    );
  }

  // Convenience method for single recipient
  sendSimpleEmail(to: string, subject: string, body: string): Observable<Result<SendEmailResponse>> {
    return this.sendEmail({
      to: [to],
      subject,
      body
    });
  }

  // Convenience method for multiple recipients
  sendBulkEmail(recipients: string[], subject: string, body: string): Observable<Result<SendEmailResponse>> {
    return this.sendEmail({
      to: recipients,
      subject,
      body
    });
  }
}
```

### Component Usage Examples

**Simple Email:**
```typescript
this.emailService.sendSimpleEmail(
  'user@example.com',
  'Welcome!',
  '<p>Welcome to our platform</p>'
).subscribe({
  next: (result) => {
    if (result.isSuccess) {
      this.showSuccess(`Email sent to ${result.value.toCount} recipient(s)`);
    } else {
      this.showError(result.message);
    }
  },
  error: (err) => this.handleHttpError(err)
});
```

**Email with CC and BCC:**
```typescript
this.emailService.sendEmail({
  to: ['alice@example.com', 'bob@example.com'],
  cc: ['manager@example.com'],
  bcc: ['audit@example.com'],
  subject: 'Monthly Report',
  body: this.generateReportHtml()
}).subscribe({
  next: (result) => {
    if (result.isSuccess) {
      console.log(`Email sent to ${result.value.totalRecipients} total recipients`);
      console.log(`- To: ${result.value.toCount}`);
      console.log(`- CC: ${result.value.ccCount}`);
      console.log(`- BCC: ${result.value.bccCount}`);
    }
  }
});
```

## Security Considerations

### XSS Prevention
- All HTML content is sanitized before sending
- Dangerous tags (`<script>`, `<iframe>`, `<embed>`, `<object>`) are removed
- JavaScript event handlers (`onclick`, `onerror`, etc.) are stripped
- Only http/https/mailto URL schemes are allowed

### Allowed HTML Elements
```
p, br, strong, b, em, i, u, a, ul, ol, li,
h1, h2, h3, h4, h5, h6, blockquote, code, pre,
span, div, table, thead, tbody, tr, th, td
```

### Allowed HTML Attributes
```
href, title, alt, class, style
```

### Allowed CSS Properties
```
color, background-color, font-size, font-weight, text-align,
padding, margin, border, width, height
```

### Rate Limiting Recommendations
- **Per-user limit:** 10 emails per minute
- **Per-IP limit:** 20 emails per minute
- **Daily limit:** 200 emails per user
- Implementation: Use AspNetCoreRateLimit NuGet package

### Recipient Limits
- **Per-field:** 50 recipients (To, CC, or BCC)
- **Total:** 100 recipients combined
- **Purpose:** Prevent abuse and spam

## Testing

### Manual Testing via Swagger

1. Start the API:
   ```powershell
   dotnet watch run --project MyOnion/src/MyOnion.WebApi/MyOnion.WebApi.csproj
   ```

2. Navigate to: `https://localhost:5001/swagger`

3. Authenticate with JWT token

4. Test POST /api/v1/emails with various scenarios:
   - Single recipient
   - Multiple recipients
   - CC and BCC
   - Invalid emails (should return 400)
   - Too many recipients (should return 400)
   - HTML with dangerous content (verify sanitization)

### Verify Ethereal Email

1. Check SMTP configuration in `appsettings.json`:
   ```json
   "MailSettings": {
     "SmtpHost": "smtp.ethereal.email",
     "SmtpPort": 587,
     "SmtpUser": "your-user",
     "SmtpPass": "your-pass",
     "EmailFrom": "noreply@test.com"
   }
   ```

2. View sent emails at: https://ethereal.email
3. Verify recipients appear correctly in To, CC, BCC fields
4. Confirm HTML is rendered safely without scripts

## Future Enhancements

### 1. Email Templates
```csharp
public class SendTemplatedEmailCommand
{
    public string TemplateId { get; set; }
    public Dictionary<string, string> Variables { get; set; }
    public List<string> To { get; set; }
}
```

### 2. Attachments Support
```csharp
public class EmailAttachment
{
    public string FileName { get; set; }
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
}
```

### 3. Email Queue (Background Processing)
- Use Hangfire or MassTransit
- Avoid blocking HTTP requests
- Retry failed sends
- Track delivery status

### 4. Audit Trail
```csharp
public class EmailAudit
{
    public Guid Id { get; set; }
    public string MessageId { get; set; }
    public string SentBy { get; set; }
    public int RecipientCount { get; set; }
    public DateTime SentAtUtc { get; set; }
    public bool Success { get; set; }
}
```

### 5. Delivery Tracking
- Webhook integration for delivery notifications
- Track opens and clicks (requires pixel tracking)
- Bounce handling

## Performance Considerations

### Current Implementation
- **Synchronous:** Blocks HTTP request until SMTP completes
- **Connection:** New SMTP connection per request
- **Acceptable for:** <100 emails/day

### For High Volume (>1000 emails/day)
1. **Connection Pooling:** Reuse SMTP connections
2. **Background Queue:** Process emails asynchronously
3. **Bulk Send API:** Batch multiple emails
4. **External Service:** Consider SendGrid, AWS SES, or Mailgun

## Troubleshooting

### Email Not Sending
1. Check SMTP credentials in appsettings.json
2. Verify firewall allows outbound port 587
3. Check application logs for SMTP errors
4. Test SMTP credentials with a mail client

### Validation Errors
1. Ensure all To emails are valid format
2. Check total recipient count doesn't exceed 100
3. Verify at least one To recipient is provided

### HTML Not Rendering
1. Ensure HTML is valid and well-formed
2. Check that email client supports HTML
3. Verify sanitizer isn't removing required tags
4. Test with simple HTML first (`<p>Test</p>`)

## References

- **HtmlSanitizer Library:** https://github.com/mganss/HtmlSanitizer
- **MailKit Documentation:** https://github.com/jstedfast/MailKit
- **OWASP XSS Prevention:** https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html
- **RFC 5321 (SMTP):** https://tools.ietf.org/html/rfc5321
- **Ethereal Email (Testing):** https://ethereal.email
