# SMTP Email Features in Template OnionAPI v10.3.0

Template OnionAPI is a Visual Studio template that bundles a prewired .NET 10 Clean Architecture Web API solution: `Domain`, `Application`, `Infrastructure.Persistence`, `Infrastructure.Shared`, and `WebApi` hosts with matching test projects. It is already configured with a lightweight mediator, FluentValidation, EF Core, Swagger, Serilog, sample CQRS features, database seeding, and DI extensions so teams can scaffold a production-ready API stack in Visual Studio with one click.

Download the template for free from Visual Studio Marketplace: https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI. For Template OnionAPI source code, visit https://github.com/workcontrolgit/MyOnionTemplate.

SMTP is a new feature added in Template OnionAPI v10.3.0. The release adds direct and template-based email flows with recipient validation, HTML sanitization, template rendering, and secured API endpoints for transactional messaging.

## What SMTP Email Features Do

1. **Direct Email Sending** - Send HTML emails with `To`, `Cc`, and `Bcc` recipients through SMTP.
2. **Template-Based Email Sending** - Use predefined JSON templates and runtime variables for reusable messaging.
3. **Input and Recipient Validation** - Enforce recipient format and limits before sending.
4. **HTML Sanitization** - Sanitize email bodies to reduce XSS risk from dynamic input.
5. **Operational Response Metadata** - Return message ID, timestamp, and recipient counts for diagnostics.

## Implemented Email Features

Template OnionAPI v10.3.0 includes:

- **SMTP transport with STARTTLS** via MailKit (`SecureSocketOptions.StartTls`).
- **Recipient handling** for `To`, `Cc`, and `Bcc`.
- **Validation rules** (max recipients and email format validation).
- **Template rendering** using Scriban from files in `email-templates`.
- **Template variable checks** using required variable definitions.
- **Sanitization policy** using `Ganss.Xss` for safe HTML output.
- **Authorized API endpoints** for send and template discovery.

## SMTP Configuration Example

Configure `MailSettings` in `appsettings.Development.json`:

```json
"MailSettings": {
  "EmailFrom": "lonie.davis67@ethereal.email",
  "SmtpHost": "smtp.ethereal.email",
  "SmtpPort": 587,
  "SmtpUser": "lonie.davis67@ethereal.email",
  "SmtpPass": "MAPpZnCe9MEYuKdN8B",
  "DisplayName": "Lonie Davis"
}
```

## Why These Features Matter

- **Faster implementation** - New email workflows can be created from templates without changing SMTP transport logic.
- **Better reliability** - Validation catches bad input before SMTP calls.
- **Improved security** - Sanitization reduces harmful HTML/script risks.
- **Cleaner operations** - Response metadata helps troubleshooting and monitoring.
- **Scalable design** - Clear layering keeps Web API, application logic, and infrastructure decoupled.

## Example Code

### SMTP Send with STARTTLS

```csharp
using var smtp = new SmtpClient();
smtp.Connect(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
smtp.Authenticate(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
await smtp.SendAsync(email);
smtp.Disconnect(true);
```
Source: `MyOnion/src/MyOnion.Infrastructure.Shared/Services/EmailService.cs`

### Templated Email Rendering and Send

```csharp
var template = await _templateService.GetTemplateAsync(request.TemplateId);
var missingVariables = _templateService.ValidateRequiredVariables(template, request.Variables);
var (subject, body) = await _templateService.RenderTemplateAsync(template, request.Variables);
var sanitizedBody = _htmlSanitizer.Sanitize(body);
await _emailService.SendAsync(emailRequest);
```
Source: `MyOnion/src/MyOnion.Application/Features/Emails/Commands/SendTemplatedEmail/SendTemplatedEmailCommand.cs`

### API Endpoints

```csharp
[HttpPost]
public async Task<IActionResult> Send(SendEmailCommand command)

[HttpPost("templated")]
public async Task<IActionResult> SendTemplated(SendTemplatedEmailCommand command)

[HttpGet("templates")]
public async Task<IActionResult> GetTemplates()
```
Source: `MyOnion/src/MyOnion.WebApi/Controllers/v1/EmailsController.cs`

## How to Test SMTP End-to-End

### 1. Configure SMTP

Use the `MailSettings` block shown above.

### 2. Run the API

```powershell
dotnet run --project .\MyOnion\src\MyOnion.WebApi\MyOnion.WebApi.csproj
```

### 3. Disable Auth for Test SMTP in Development

Set the feature flag in `MyOnion/src/MyOnion.WebApi/appsettings.Development.json`:

```json
"FeatureManagement": {
  "AuthEnabled": false
}
```

This allows Swagger test calls to bypass the Access Token requirement in development for SMTP testing.

### 4. Send a Templated Email

Call `POST /api/v1/Emails/templated` with:

```json
{
  "templateId": "welcome-user",
  "to": ["your-email@example.com"],
  "variables": {
    "company_name": "Template OnionAPI Corp",
    "activation_url": "https://example.com/activate/abc123",
    "current_year": "2026",
    "company_address": "123 Main St",
    "user.first_name": "Jane",
    "user.email": "your-email@example.com",
    "user": {
      "first_name": "Jane",
      "email": "your-email@example.com"
    }
  }
}
```

### 5. Verify in Ethereal

- Inbox/messages: https://ethereal.email/messages
- Account dashboard and SMTP settings: https://ethereal.email

Sign in with your Ethereal account and verify subject, recipients, and rendered body.

## Production Best Practices

1. **Store SMTP secrets outside source control** - Use user-secrets or environment variables.
2. **Keep HTML sanitization enabled** - Do not bypass sanitization for dynamic content.
3. **Use template IDs for consistent messaging** - Prefer template-based emails for repeated workflows.
4. **Validate recipient counts in clients too** - Prevent avoidable API validation failures.
5. **Add SMTP integration checks in CI/CD** - Use test SMTP infrastructure for delivery validation.

## Blog Summary

- Template OnionAPI v10.3.0 includes SMTP email features in the VS template.
- Features include direct send, template send, validation, sanitization, and authorized APIs.
- MailKit with STARTTLS secures SMTP transport.
- Ethereal provides a quick way to verify email delivery during development.
- The implementation is layered, testable, and ready for transactional workflows.


