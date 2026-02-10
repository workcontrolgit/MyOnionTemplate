# Email Templates with Variable Substitution

## Overview

This feature provides a flexible, secure email templating system with variable substitution using the Scriban templating engine. Templates are stored as JSON files and support dynamic content rendering with type-safe variable validation.

## Features

- ✅ **File-Based Templates** - JSON configuration files stored in `email-templates/`
- ✅ **Scriban Templating** - Powerful, secure template engine with Liquid-like syntax
- ✅ **Variable Substitution** - Dynamic content replacement with validation
- ✅ **HTML Sanitization** - XSS prevention built-in
- ✅ **Template Management API** - List and retrieve templates via REST endpoints
- ✅ **Required Variable Validation** - Ensures all required variables are provided
- ✅ **Example Variables** - Self-documenting templates with sample data

## Package Dependencies

**Scriban 6.5.2**
- NuGet: https://www.nuget.org/packages/Scriban
- GitHub: https://github.com/scriban/scriban
- Fast, secure, lightweight templating engine
- Thread-safe for concurrent requests
- Liquid-compatible syntax

## Architecture

### Components

1. **EmailTemplate** - DTO representing a template definition
2. **IEmailTemplateService** - Interface for template operations
3. **EmailTemplateService** - Implementation with file-based storage and caching
4. **SendTemplatedEmailCommand** - Command for sending templated emails
5. **EmailsController** - REST API endpoints

### File Structure

```
MyOnion.WebApi/
└── email-templates/
    ├── welcome-user.json
    ├── password-reset.json
    ├── employee-invitation.json
    └── monthly-report.json
```

### Template JSON Schema

```json
{
  "id": "unique-template-id",
  "name": "Human-Readable Name",
  "description": "When to use this template",
  "category": "Transactional|Marketing|HR|Reports",
  "subjectTemplate": "Subject with {{ variables }}",
  "bodyTemplate": "<html>Body with {{ variables }}</html>",
  "from": "optional@sender.com",
  "requiredVariables": ["var1", "var2"],
  "exampleVariables": {
    "var1": "example value",
    "var2": "example value"
  }
}
```

## API Endpoints

### 1. Send Templated Email

**Endpoint:** `POST /api/v1/emails/templated`
**Authorization:** Required (JWT)

**Request:**
```json
{
  "templateId": "welcome-user",
  "variables": {
    "company_name": "MyOnion Corp",
    "user": {
      "first_name": "John",
      "email": "john.doe@example.com"
    },
    "activation_url": "https://example.com/activate/abc123",
    "current_year": "2026",
    "company_address": "123 Main St, San Francisco, CA"
  },
  "to": ["john.doe@example.com"],
  "cc": [],
  "bcc": [],
  "from": null
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "message": "Email sent using template 'Welcome New User'",
  "errors": [],
  "value": {
    "sent": true,
    "messageId": "a1b2c3d4-e5f6-4a5b-8c7d-9e8f7a6b5c4d",
    "sentAtUtc": "2026-02-07T22:00:00Z",
    "toCount": 1,
    "ccCount": 0,
    "bccCount": 0,
    "totalRecipients": 1
  },
  "executionTimeMs": 1450.3
}
```

**Error - Template Not Found (200 OK with isSuccess=false):**
```json
{
  "isSuccess": false,
  "message": "Email template 'invalid-id' not found.",
  "errors": [],
  "value": null
}
```

**Error - Missing Required Variables (200 OK with isSuccess=false):**
```json
{
  "isSuccess": false,
  "message": "Missing required variables: user.first_name, activation_url",
  "errors": [],
  "value": null
}
```

### 2. Get All Templates

**Endpoint:** `GET /api/v1/emails/templates`
**Authorization:** Required (JWT)

**Response:**
```json
[
  {
    "id": "welcome-user",
    "name": "Welcome New User",
    "description": "Welcome email sent to newly registered users",
    "category": "Transactional",
    "subjectTemplate": "Welcome to {{ company_name }}, {{ user.first_name }}!",
    "bodyTemplate": "<!DOCTYPE html>...",
    "from": null,
    "requiredVariables": ["company_name", "user.first_name", "user.email", "activation_url", "current_year"],
    "exampleVariables": {
      "company_name": "MyOnion Corp",
      "user.first_name": "John",
      "user.email": "john.doe@example.com",
      "activation_url": "https://example.com/activate/abc123",
      "current_year": "2026"
    }
  },
  {
    "id": "password-reset",
    "name": "Password Reset Request",
    ...
  }
]
```

### 3. Get Template by ID

**Endpoint:** `GET /api/v1/emails/templates/{templateId}`
**Authorization:** Required (JWT)

**Example:** `GET /api/v1/emails/templates/welcome-user`

**Response (200 OK):**
```json
{
  "id": "welcome-user",
  "name": "Welcome New User",
  "description": "Welcome email sent to newly registered users",
  "category": "Transactional",
  "subjectTemplate": "Welcome to {{ company_name }}, {{ user.first_name }}!",
  "bodyTemplate": "<!DOCTYPE html>...",
  "from": null,
  "requiredVariables": ["company_name", "user.first_name", "user.email", "activation_url", "current_year"],
  "exampleVariables": {
    "company_name": "MyOnion Corp",
    "user.first_name": "John"
  }
}
```

**Response (404 Not Found):**
```json
{
  "message": "Template 'invalid-id' not found"
}
```

## Scriban Template Syntax

### Variable Substitution

```scriban
{{ variable_name }}
{{ user.first_name }}
{{ company.address.city }}
```

### Conditionals

```scriban
{{ if user.is_premium }}
  <p>Welcome, premium member!</p>
{{ else }}
  <p>Upgrade to premium!</p>
{{ end }}
```

### Loops

```scriban
<ul>
{{ for item in items }}
  <li>{{ item.name }}</li>
{{ end }}
</ul>
```

### Filters

```scriban
{{ user.name | upcase }}
{{ price | format "C2" }}
{{ date | date.to_string "%Y-%m-%d" }}
```

### Safe Navigation

```scriban
{{ user?.profile?.avatar ?? "default.png" }}
```

## Built-In Templates

### 1. welcome-user

**Purpose:** Welcome email for new user registrations
**Category:** Transactional
**Required Variables:**
- `company_name` - Company name
- `user.first_name` - User's first name
- `user.email` - User's email address
- `activation_url` - Account activation link
- `current_year` - Current year for footer

**Use Case:**
```csharp
var command = new SendTemplatedEmailCommand
{
    TemplateId = "welcome-user",
    Variables = new Dictionary<string, object>
    {
        ["company_name"] = "MyOnion Corp",
        ["user"] = new { first_name = "John", email = "john@example.com" },
        ["activation_url"] = "https://app.com/activate/token123",
        ["current_year"] = DateTime.Now.Year.ToString()
    },
    To = new List<string> { "john@example.com" }
};
```

### 2. password-reset

**Purpose:** Password reset request email
**Category:** Transactional
**Required Variables:**
- `user.first_name` - User's first name
- `user.email` - User's email
- `reset_url` - Password reset link
- `expiration_hours` - Link expiration time
- `company_name` - Company name
- `current_year` - Current year

**Security Features:**
- Warns user if they didn't request reset
- Shows expiration time
- Includes security tips

### 3. employee-invitation

**Purpose:** New employee onboarding invitation
**Category:** HR
**Required Variables:**
- `employee.first_name`, `employee.email`, `employee.employee_number`, `employee.start_date`
- `position.title`
- `department.name`
- `company_name`
- `setup_url` - Account setup link
- `hr_email`, `hr_phone`, `hr_contact_name`
- `current_year`

**Features:**
- Professional HR branding
- Detailed employee information table
- Next steps checklist
- HR contact information

### 4. monthly-report

**Purpose:** Department monthly summary reports
**Category:** Reports
**Required Variables:**
- `manager.first_name`
- `department.name`
- `report.month`, `report.year`
- `metrics.total_employees`, `metrics.new_hires`, `metrics.avg_salary`
- `dashboard_url`
- `company_name`, `current_year`

**Optional Variables:**
- `top_performers` (array) - Top performing employees
- `action_items` (array) - Action items list

**Features:**
- Visual stat cards
- Dynamic tables with loops
- Gradient styling

## Creating Custom Templates

### Step 1: Create Template JSON

Create a new file in `MyOnion.WebApi/email-templates/my-template.json`:

```json
{
  "id": "order-confirmation",
  "name": "Order Confirmation",
  "description": "Email sent after successful order placement",
  "category": "Transactional",
  "subjectTemplate": "Order #{{ order.number }} Confirmed - Thank You!",
  "bodyTemplate": "<!DOCTYPE html>\n<html>\n<head>\n  <style>\n    body { font-family: Arial, sans-serif; }\n    .order-summary { border: 1px solid #ddd; padding: 20px; }\n  </style>\n</head>\n<body>\n  <h1>Thank You for Your Order!</h1>\n  <p>Hi {{ customer.name }},</p>\n  <p>Your order #{{ order.number }} has been confirmed.</p>\n  \n  <div class=\"order-summary\">\n    <h2>Order Details</h2>\n    <ul>\n    {{ for item in order.items }}\n      <li>{{ item.name }} - ${{ item.price }} x {{ item.quantity }}</li>\n    {{ end }}\n    </ul>\n    <p><strong>Total: ${{ order.total }}</strong></p>\n  </div>\n  \n  <p>Estimated delivery: {{ order.estimated_delivery }}</p>\n  <p>Track your order: <a href=\"{{ tracking_url }}\">{{ tracking_url }}</a></p>\n</body>\n</html>",
  "from": "orders@myonion.com",
  "requiredVariables": [
    "customer.name",
    "order.number",
    "order.total",
    "order.estimated_delivery",
    "tracking_url"
  ],
  "exampleVariables": {
    "customer.name": "Alice Johnson",
    "order.number": "12345",
    "order.total": "99.99",
    "order.estimated_delivery": "February 15, 2026",
    "tracking_url": "https://track.example.com/12345"
  }
}
```

### Step 2: Use the Template

```csharp
var command = new SendTemplatedEmailCommand
{
    TemplateId = "order-confirmation",
    Variables = new Dictionary<string, object>
    {
        ["customer"] = new { name = "Alice Johnson" },
        ["order"] = new
        {
            number = "12345",
            total = "99.99",
            estimated_delivery = "February 15, 2026",
            items = new[]
            {
                new { name = "Product A", price = "49.99", quantity = 1 },
                new { name = "Product B", price = "25.00", quantity = 2 }
            }
        },
        ["tracking_url"] = "https://track.example.com/12345"
    },
    To = new List<string> { "alice@example.com" }
};

var result = await mediator.Send(command);
```

### Step 3: Test in Swagger

1. Start API: `dotnet watch run --project MyOnion/src/MyOnion.WebApi/MyOnion.WebApi.csproj`
2. Navigate to: `https://localhost:5001/swagger`
3. Authorize with JWT
4. Test `POST /api/v1/emails/templated`

## Angular/TypeScript Integration

### TypeScript Interfaces

```typescript
export interface EmailTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  subjectTemplate: string;
  bodyTemplate: string;
  from?: string;
  requiredVariables: string[];
  exampleVariables: { [key: string]: string };
}

export interface SendTemplatedEmailRequest {
  templateId: string;
  variables: { [key: string]: any };
  to: string[];
  cc?: string[];
  bcc?: string[];
  from?: string;
}
```

### Angular Service

```typescript
@Injectable({
  providedIn: 'root'
})
export class EmailTemplateService {
  private apiUrl = 'https://localhost:5001/api/v1/emails';

  constructor(private http: HttpClient) {}

  getAllTemplates(): Observable<EmailTemplate[]> {
    return this.http.get<EmailTemplate[]>(`${this.apiUrl}/templates`);
  }

  getTemplate(templateId: string): Observable<EmailTemplate> {
    return this.http.get<EmailTemplate>(`${this.apiUrl}/templates/${templateId}`);
  }

  sendTemplatedEmail(request: SendTemplatedEmailRequest): Observable<Result<SendEmailResponse>> {
    return this.http.post<Result<SendEmailResponse>>(
      `${this.apiUrl}/templated`,
      request
    );
  }

  // Convenience method
  sendWelcomeEmail(user: User, activationUrl: string): Observable<Result<SendEmailResponse>> {
    return this.sendTemplatedEmail({
      templateId: 'welcome-user',
      variables: {
        company_name: 'MyOnion Corp',
        user: {
          first_name: user.firstName,
          email: user.email
        },
        activation_url: activationUrl,
        current_year: new Date().getFullYear().toString(),
        company_address: '123 Main St, San Francisco, CA'
      },
      to: [user.email]
    });
  }
}
```

### Component Usage

```typescript
export class UserRegistrationComponent {
  constructor(private emailService: EmailTemplateService) {}

  onUserRegistered(user: User) {
    const activationToken = this.generateActivationToken();
    const activationUrl = `${window.location.origin}/activate/${activationToken}`;

    this.emailService.sendWelcomeEmail(user, activationUrl)
      .subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.showSuccess('Welcome email sent!');
          } else {
            this.showError(result.message);
          }
        },
        error: (err) => this.handleError(err)
      });
  }
}
```

## Security Considerations

### XSS Prevention
- All template output is HTML sanitized before sending
- Scriban templates are parsed server-side only
- No client-side template evaluation

### Template Security
- Templates are read-only JSON files
- No code execution in templates (safe expressions only)
- Scriban's safe mode prevents dangerous operations

### Variable Validation
- Required variables are validated before rendering
- Missing variables cause explicit error messages
- Type safety through C# dictionaries

### From Address
- Can be locked down per template
- Falls back to MailSettings configuration
- Optional override with validation

## Performance

### Template Caching
- Templates are cached in memory after first load
- Thread-safe dictionary with lock for updates
- Cache invalidation: restart application

### Rendering Performance
- Scriban is highly optimized (~millions of renders/sec)
- Template parsing is cached automatically
- Minimal overhead over plain HTML

### Recommendations
- **Low Volume (<100/day):** Current implementation sufficient
- **Medium Volume (<10K/day):** Add Redis cache for templates
- **High Volume (>10K/day):** Consider message queue + worker pool

## Testing

### Manual Testing

1. **List all templates:**
   ```bash
   curl -H "Authorization: Bearer {token}" \
     https://localhost:5001/api/v1/emails/templates
   ```

2. **Get specific template:**
   ```bash
   curl -H "Authorization: Bearer {token}" \
     https://localhost:5001/api/v1/emails/templates/welcome-user
   ```

3. **Send templated email:**
   ```bash
   curl -X POST -H "Authorization: Bearer {token}" \
     -H "Content-Type: application/json" \
     -d '{
       "templateId": "welcome-user",
       "variables": {
         "company_name": "Test Corp",
         "user": { "first_name": "John", "email": "john@test.com" },
         "activation_url": "https://test.com/activate/123",
         "current_year": "2026"
       },
       "to": ["john@ethereal.email"]
     }' \
     https://localhost:5001/api/v1/emails/templated
   ```

### Automated Testing

```csharp
public class EmailTemplateServiceTests
{
    [Fact]
    public async Task RenderTemplate_WithValidVariables_ReturnsRenderedContent()
    {
        // Arrange
        var service = new EmailTemplateService();
        var template = new EmailTemplate
        {
            SubjectTemplate = "Hello {{ name }}",
            BodyTemplate = "<p>Welcome, {{ name }}!</p>"
        };
        var variables = new Dictionary<string, object>
        {
            ["name"] = "John"
        };

        // Act
        var (subject, body) = await service.RenderTemplateAsync(template, variables);

        // Assert
        Assert.Equal("Hello John", subject);
        Assert.Equal("<p>Welcome, John!</p>", body);
    }

    [Fact]
    public void ValidateRequiredVariables_WithMissingVariables_ReturnsMissingList()
    {
        // Arrange
        var service = new EmailTemplateService();
        var template = new EmailTemplate
        {
            RequiredVariables = new List<string> { "name", "email" }
        };
        var variables = new Dictionary<string, object>
        {
            ["name"] = "John"
        };

        // Act
        var missing = service.ValidateRequiredVariables(template, variables);

        // Assert
        Assert.Single(missing);
        Assert.Contains("email", missing);
    }
}
```

## Troubleshooting

### Template Not Found
- **Error:** `Email template 'xxx' not found`
- **Fix:** Ensure template JSON file exists in `email-templates/` folder
- **Check:** Template ID matches filename without .json extension

### Missing Variables
- **Error:** `Missing required variables: xxx, yyy`
- **Fix:** Provide all required variables in the request
- **Tip:** Use GET /api/v1/emails/templates/{id} to see required variables

### Rendering Errors
- **Error:** Template rendering fails
- **Fix:** Validate Scriban syntax in bodyTemplate
- **Tip:** Test templates at https://scribantest.net/

### Templates Not Loading
- **Issue:** GetAllTemplates returns empty array
- **Fix:** Check email-templates directory exists in application base directory
- **Development:** Templates should be in `MyOnion.WebApi/email-templates/`
- **Production:** Ensure templates are copied to output directory

## Future Enhancements

1. **Database Storage** - Store templates in database for runtime editing
2. **Template Versioning** - Track template changes over time
3. **A/B Testing** - Send different template versions to measure effectiveness
4. **Preview API** - Render template without sending for preview
5. **Multilingual Support** - Template translations by locale
6. **Rich Editor Integration** - WYSIWYG template editor in admin panel
7. **Template Inheritance** - Base templates with child overrides
8. **Inline CSS Inliner** - Automatic CSS inlining for email clients
9. **Attachment Support** - Include attachments with templates
10. **Analytics Tracking** - Open and click tracking

## References

- **Scriban Documentation:** https://github.com/scriban/scriban/blob/master/doc/language.md
- **Scriban NuGet:** https://www.nuget.org/packages/Scriban
- **Email Template Best Practices:** https://www.campaignmonitor.com/dev-resources/guides/coding/
- **Liquid Syntax:** https://shopify.github.io/liquid/ (Scriban is compatible)
- **Transactional Email Guide:** https://sendgrid.com/resource/transactional-email-best-practices/
