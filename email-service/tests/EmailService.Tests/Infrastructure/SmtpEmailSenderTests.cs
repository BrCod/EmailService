using EmailService.Application.Commands.SendEmail;
using EmailService.Domain.Entities;
using EmailService.Infrastructure.Config;
using EmailService.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EmailService.Tests.Infrastructure
{
    public class SmtpEmailSenderTests
    {
        [Fact]
        public async Task SendAsync_Returns_Failed_When_Server_Unreachable()
        {
            var opts = Options.Create(new SmtpOptions { Host = "localhost", Port = 1 });
            var sender = new SmtpEmailSender(opts, NullLogger<SmtpEmailSender>.Instance);

            var msg = new EmailMessage(new EmailAddress("from@example.com"),
                new[] { new EmailAddress("to@example.com") }, "Subject", "Body", null, new Dictionary<string,string>(), new List<string>(), "tenant", Guid.NewGuid().ToString());

            var result = await sender.SendAsync(msg);
            Assert.False(result.Success);
        }
    }
}
