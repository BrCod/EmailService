using EmailPublisher;
using EmailPublisher.Models;
using System.Text;
using Xunit;

namespace EmailPublisher.Tests
{
    public class CloudEventFactoryTests
    {
        [Fact]
        public void CreateEmailSendRequest_Produces_ValidJson()
        {
            var msg = new EmailMessage
            {
                From = new EmailAddress("from@example.com"),
                To = new() { new EmailAddress("to@example.com") },
                Subject = "Hello",
                Body = "World",
                TenantId = "tenant",
                CorrelationId = "corr"
            };
            var payload = CloudEventFactory.CreateEmailSendRequest(msg, "source");
            var json = Encoding.UTF8.GetString(payload.ToArray());
            Assert.Contains("\"type\":\"email.send.request\"", json);
            Assert.Contains("\"data\"", json);
        }
    }
}
