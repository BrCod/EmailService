# Test Email Sender Script
# This script publishes test email messages to RabbitMQ

Write-Host "Testing Email Service via RabbitMQ..." -ForegroundColor Green

# You can use the EmailPublisher library from your code, or use curl/Invoke-RestMethod
# For quick testing, you can manually publish to RabbitMQ or use a C# console app

# Option 1: Create a simple JSON message and publish via HTTP API (if you add an endpoint)
# Option 2: Use RabbitMQ management API to publish directly
# Option 3: Create a quick C# console app using your EmailPublisher library

$rabbitMqHost = "localhost"
$rabbitMqPort = "15672"
$username = "guest"
$password = "guest"
$vhost = "/"
$exchange = ""
$routingKey = "email.send"

# Create base64 credentials
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))

# Create a test email message in CloudEvents format
$message = @{
    specversion = "1.0"
    type = "email.send.request"
    source = "test-script"
    id = [Guid]::NewGuid().ToString()
    time = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    datacontenttype = "application/json"
    data = @{
        From = @{
            Value = "sender@example.com"
        }
        To = @(
            @{
                Value = "test@example.com"
            }
        )
        Subject = "Test Email from Docker"
        Body = "This is a test email sent through the email service running on Docker!"
        TemplateId = $null
        TemplateData = @{}
        Attachments = @()
        TenantId = "default"
        CorrelationId = [Guid]::NewGuid().ToString()
    }
} | ConvertTo-Json -Depth 10

Write-Host "Email Message:" -ForegroundColor Cyan
Write-Host $message

# Publish to RabbitMQ using Management API
$publishUrl = "http://${rabbitMqHost}:${rabbitMqPort}/api/exchanges/%2F/amq.default/publish"

$publishBody = @{
    properties = @{
        delivery_mode = 2
        content_type = "application/json"
    }
    routing_key = $routingKey
    payload = $message
    payload_encoding = "string"
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri $publishUrl -Method Post -Headers @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json"
    } -Body $publishBody

    Write-Host "`nMessage published successfully!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Yellow
    Write-Host "`nCheck the following:" -ForegroundColor Cyan
    Write-Host "1. RabbitMQ Management: http://localhost:15672" -ForegroundColor White
    Write-Host "2. MailHog UI: http://localhost:8025" -ForegroundColor White
    Write-Host "3. Email Service Logs: docker-compose logs -f email-service" -ForegroundColor White
}
catch {
    Write-Host "`nError publishing message:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "`nMake sure Docker services are running: docker-compose ps" -ForegroundColor Yellow
}
