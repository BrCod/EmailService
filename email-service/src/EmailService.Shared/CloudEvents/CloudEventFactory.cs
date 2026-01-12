using System.Text.Json;
using EmailService.Domain.Entities;

namespace EmailService.Shared.CloudEvents
{
    public static class CloudEventFactory
    {
        public static string CreateEmailSendRequestJson(EmailMessage message, string source)
        {
            var envelope = new
            {
                specversion = "1.0",
                id = Guid.NewGuid().ToString(),
                source = source,
                type = "email.send.request",
                time = DateTimeOffset.UtcNow,
                datacontenttype = "application/json",
                data = message
            };
            return JsonSerializer.Serialize(envelope, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
