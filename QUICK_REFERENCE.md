# Email Service Quick Reference

## Start Services

```powershell
docker-compose up -d
```

## Send Test Email

```powershell
.\test-email-sender.ps1
```

## View Emails

- **MailHog UI**: http://localhost:8025
- **RabbitMQ UI**: http://localhost:15672 (guest/guest)

## Check Status

```powershell
# View logs
docker logs email-service-api -f

# Queue status
docker exec email-service-rabbitmq rabbitmqctl list_queues

# All containers
docker-compose ps
```

## Minimal Message Example

```json
{
  "specversion": "1.0",
  "type": "email.send.request",
  "source": "my-app",
  "id": "12345678-1234-1234-1234-123456789012",
  "time": "2026-01-12T15:27:03Z",
  "datacontenttype": "application/json",
  "data": {
    "From": {"Value": "sender@example.com"},
    "To": [{"Value": "recipient@example.com"}],
    "Subject": "Hello",
    "Body": "Hello World",
    "TemplateId": null,
    "TemplateData": {},
    "Attachments": [],
    "TenantId": "default",
    "CorrelationId": "msg-001"
  }
}
```

## Key Points

- ✅ Message type MUST be: `"email.send.request"`
- ✅ Queue name: `email.send`
- ✅ Exchange: `amq.default`
- ✅ All timestamps in ISO 8601 format
- ✅ All IDs should be UUIDs
- ✅ Email addresses wrapped in `{"Value": "email@example.com"}`

## Troubleshooting

| Issue | Check |
|-------|-------|
| Messages not processing | `docker logs email-service-api \| Select-String ERROR` |
| Emails not in MailHog | Verify "email.send.request" type |
| Queue keeps growing | Check service logs for processing errors |
| Connection refused | Run `docker-compose ps` to verify services |

## See Also

- [MESSAGE_FORMAT.md](MESSAGE_FORMAT.md) - Detailed format specification
