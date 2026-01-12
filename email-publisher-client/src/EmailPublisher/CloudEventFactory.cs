using System.Text.Json;
using EmailPublisher.Models;

namespace EmailPublisher
{
    public static class CloudEventFactory
    {
        public static ReadOnlyMemory<byte> CreateEmailSendRequest(EmailMessage message, string source)
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
            var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            return new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(json));
        }
    }
}
