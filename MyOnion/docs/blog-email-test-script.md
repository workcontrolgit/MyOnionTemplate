# SMTP Test Script

## SMTP Configuration

- Host: `smtp.ethereal.email`
- Port: `587`
- Security: `STARTTLS`
- Username: `lonie.davis67@ethereal.email`
- Password: `MAPpZnCe9MEYuKdN8B`

## Appsettings Example

Use this block in `MyOnion/src/MyOnion.WebApi/appsettings.Development.json`:

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

## Ethereal Setup and Message Verification

1. Open `https://ethereal.email` and sign in with your Ethereal mailbox credentials.
2. To check received emails, open `https://ethereal.email/messages`.
3. To view SMTP credentials/settings, open mailbox details from the Ethereal dashboard (`https://ethereal.email`) and review the SMTP section.
4. Keep the inbox page open while testing.
5. After calling the API, refresh the inbox and open the newest message.
6. Confirm:
   - Subject is rendered from template.
   - Recipient list is correct.
   - HTML body is rendered and sanitized.

## Prerequisites

1. API is running locally.
2. You have a valid JWT access token.
3. Template `welcome-user` exists.

## Example Templated Email Test (PowerShell)

```powershell
$token = "<JWT_TOKEN>"
$apiBase = "https://localhost:5001"

$payload = @{
  templateId = "welcome-user"
  to         = @("your-email@example.com")
  cc         = @()
  bcc        = @()
  from       = $null
  variables  = @{
    company_name    = "MyOnion Corp"
    activation_url  = "https://example.com/activate/abc123"
    current_year    = "2026"
    company_address = "123 Main St, San Francisco, CA 94105"

    # Current validation requires these flat keys
    "user.first_name" = "Jane"
    "user.email"      = "your-email@example.com"

    # Template rendering uses nested object
    user = @{
      first_name = "Jane"
      email      = "your-email@example.com"
    }
  }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod \
  -Method Post \
  -Uri "$apiBase/api/v1/Emails/templated" \
  -Headers @{ Authorization = "Bearer $token" } \
  -ContentType "application/json" \
  -Body $payload
```

## Expected Response

Success returns JSON with:
- `isSuccess: true`
- `value.sent: true`
- `value.messageId`
- `value.sentAtUtc`
- recipient counts (`toCount`, `ccCount`, `bccCount`, `totalRecipients`)

## If You Still Get Missing Variables

If response says `Missing required variables: user.first_name, user.email`, check your JSON contains both:
- `"user.first_name": "..."`
- `"user.email": "..."`
- and nested `"user": { "first_name": "...", "email": "..." }`
