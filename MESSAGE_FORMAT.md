# Email Service Message Format

This document describes the message format used to send emails through the Email Service via RabbitMQ.

## Overview

The Email Service uses **CloudEvents** format (v1.0) to wrap email payloads. Messages are published to the `email.send` RabbitMQ queue and processed asynchronously by the email service.

## Message Structure

All messages follow the CloudEvents specification with the following format:

```json
{
  "specversion": "1.0",
  "type": "email.send.request",
  "source": "your-application-name",
  "id": "unique-message-id-uuid",
  "time": "2026-01-12T15:27:03.408Z",
  "datacontenttype": "application/json",
  "data": {
    // Email payload (see below)
  }
}
```

### CloudEvents Attributes

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `specversion` | string | ✅ | CloudEvents specification version. Must be `"1.0"` |
| `type` | string | ✅ | Event type. Must be `"email.send.request"` |
| `source` | string | ✅ | Source of the event (e.g., `"api.example.com"`, `"user-service"`) |
| `id` | string | ✅ | Unique message identifier (typically a UUID) |
| `time` | string | ✅ | ISO 8601 timestamp of message creation |
| `datacontenttype` | string | ✅ | Content type of the data payload. Must be `"application/json"` |
| `data` | object | ✅ | Email payload (see Email Payload section) |

## Email Payload

The `data` field contains the email message:

```json
{
  "From": {
    "Value": "sender@example.com"
  },
  "To": [
    {
      "Value": "recipient@example.com"
    },
    {
      "Value": "another@example.com"
    }
  ],
  "Subject": "Email Subject",
  "Body": "Email body content",
  "TemplateId": null,
  "TemplateData": {},
  "Attachments": [],
  "TenantId": "tenant-id",
  "CorrelationId": "correlation-id-uuid"
}
```

### Email Payload Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `From` | EmailAddress | ✅ | Sender email address. Structure: `{"Value": "email@example.com"}` |
| `To` | EmailAddress[] | ✅ | Recipient email address(es). Array of `{"Value": "email@example.com"}` |
| `Subject` | string | ✅ | Email subject line |
| `Body` | string | ✅ | Email body content (plain text or HTML) |
| `TemplateId` | string | ❌ | Template ID for templated emails (currently not implemented) |
| `TemplateData` | object | ❌ | Template variables (currently not implemented) |
| `Attachments` | string[] | ❌ | File paths to attachments (empty array if none) |
| `TenantId` | string | ✅ | Tenant identifier for multi-tenant support |
| `CorrelationId` | string | ✅ | Correlation ID for tracing (UUID format) |

### EmailAddress Format

Email addresses are represented as objects with a single `Value` property:

```json
{
  "Value": "email@example.com"
}
```

**Validation Rules:**
- Email must match pattern: `^[^\s@]+@[^\s@]+\.[^\s@]+$`
- Must be a valid email address format

## Complete Example

```json
{
  "specversion": "1.0",
  "type": "email.send.request",
  "source": "order-service",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "time": "2026-01-12T15:27:03.408Z",
  "datacontenttype": "application/json",
  "data": {
    "From": {
      "Value": "noreply@example.com"
    },
    "To": [
      {
        "Value": "customer@example.com"
      }
    ],
    "Subject": "Order Confirmation #12345",
    "Body": "Thank you for your order! Your order number is #12345.\n\nOrder Details:\n- Item: Widget\n- Quantity: 2\n- Total: $50.00\n\nExpected delivery: 2-3 business days.",
    "TemplateId": null,
    "TemplateData": {},
    "Attachments": [],
    "TenantId": "tenant-001",
    "CorrelationId": "order-12345-correlate"
  }
}
```

## Sending Messages

### Via PowerShell Script

Use the provided test script (`test-email-sender.ps1`):

```powershell
.\test-email-sender.ps1
```

### Via RabbitMQ Management UI

1. Open http://localhost:15672
2. Login with credentials: `guest` / `guest`
3. Navigate to **Exchanges** → **amq.default**
4. Click **Publish message**
5. Set **Routing key**: `email.send`
6. Paste the complete message JSON into the payload field
7. Click **Publish message**

### Via Code (C#)

Using the EmailPublisher client library:

```csharp
var emailPublisher = serviceProvider.GetRequiredService<IEmailPublisher>();

var message = new EmailMessage
{
    From = new EmailAddress("sender@example.com"),
    To = new List<EmailAddress> 
    { 
        new EmailAddress("recipient@example.com") 
    },
    Subject = "Test Email",
    Body = "This is a test email",
    TenantId = "default",
    CorrelationId = Guid.NewGuid().ToString()
};

await emailPublisher.PublishEmailAsync(message);
```

### Via RabbitMQ.Client (Direct)

```csharp
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

var message = new
{
    specversion = "1.0",
    type = "email.send.request",
    source = "my-app",
    id = Guid.NewGuid().ToString(),
    time = DateTime.UtcNow.ToString("O"),
    datacontenttype = "application/json",
    data = new
    {
        From = new { Value = "sender@example.com" },
        To = new[] { new { Value = "recipient@example.com" } },
        Subject = "Test",
        Body = "Test email",
        TemplateId = (string?)null,
        TemplateData = new { },
        Attachments = new string[] { },
        TenantId = "default",
        CorrelationId = Guid.NewGuid().ToString()
    }
};

var json = JsonSerializer.Serialize(message);
var body = Encoding.UTF8.GetBytes(json);
channel.BasicPublish("amq.default", "email.send", null, body);
```

## Response

The email service processes messages asynchronously. There is no direct response. Success or failure is logged.

**Success Indicator:**
- Message is removed from queue (acknowledged)
- Log entry: `[INF] Email sent successfully`

**Failure Indicator:**
- Message is negatively acknowledged (nacked)
- Log entry: `[ERR] Message processing failed`
- Check service logs for details

## Viewing Results

### MailHog Web UI

Access the MailHog web interface at **http://localhost:8025** to view all captured emails:

- View inbox with all sent emails
- Click on any email to see details, HTML rendering, and headers
- No emails are actually sent; they're captured for testing

### Service Logs

Check service logs for processing details:

```powershell
docker logs email-service-api -f
```

Look for entries like:
```
[15:27:03 INF] Message received from queue
[15:27:03 INF] CloudEvent type: email.send.request
[15:27:03 INF] Sending email via handler
[15:27:03 INF] Email sent successfully
```

## Error Handling

Common errors and solutions:

| Error | Cause | Solution |
|-------|-------|----------|
| `Unexpected CloudEvent type` | Wrong `type` field | Use `"email.send.request"` |
| `Invalid email payload` | Missing required fields | Ensure all required fields are present |
| `Message processing failed` | Deserialization error | Validate JSON structure matches format |
| `SMTP connection error` | MailHog not running | Ensure MailHog container is running: `docker-compose ps` |

## Queue Management

### Check Queue Status

```powershell
docker exec email-service-rabbitmq rabbitmqctl list_queues name messages consumers
```

### Purge Queue

```powershell
docker exec email-service-rabbitmq rabbitmqctl purge_queue email.send
```

### Monitor Queue

```powershell
docker logs email-service-rabbitmq -f
```

## Best Practices

1. **Always include a CorrelationId** for tracing and debugging
2. **Use meaningful TenantId** values for multi-tenant environments
3. **Set appropriate Source** values to identify message origin
4. **Include timestamps** in ISO 8601 format
5. **Validate email addresses** before publishing (follows RFC 5322 simplified pattern)
6. **Monitor logs** during testing to catch issues early
7. **Use UUIDs** for message IDs and correlation IDs

## Testing

### Quick Test

```powershell
.\test-email-sender.ps1
```

Then check **http://localhost:8025** for the received email.

### Multiple Recipients Example

```json
{
  "specversion": "1.0",
  "type": "email.send.request",
  "source": "bulk-service",
  "id": "bulk-001",
  "time": "2026-01-12T15:30:00.000Z",
  "datacontenttype": "application/json",
  "data": {
    "From": {"Value": "noreply@company.com"},
    "To": [
      {"Value": "user1@example.com"},
      {"Value": "user2@example.com"},
      {"Value": "user3@example.com"}
    ],
    "Subject": "Announcement",
    "Body": "Important announcement for all users.",
    "TemplateId": null,
    "TemplateData": {},
    "Attachments": [],
    "TenantId": "default",
    "CorrelationId": "announce-2026-01-12"
  }
}
```

## References

- [CloudEvents Specification v1.0](https://cloudevents.io/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [MailHog Documentation](https://github.com/mailhog/MailHog)
