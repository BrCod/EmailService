using EmailService.Application.Commands.SendEmail;
using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Domain.Services;
using Moq;
using Xunit;

namespace EmailService.Tests.Application
{
    public class SendEmailCommandHandlerTests
    {
        [Fact]
        public async Task Successful_Send_Persists_Result()
        {
            var sender = new Mock<IEmailSender>();
            var repo = new Mock<IEmailMessageRepository>();
            var validator = new EmailValidationService();
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<SendEmailCommandHandler>();
            sender.Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), default))
                  .ReturnsAsync(new SendEmailResult { Status = "Sent", ProviderMessageId = "abc" });

            var handler = new SendEmailCommandHandler(sender.Object, repo.Object, validator, logger);

            var msg = new EmailMessage(new EmailAddress("from@example.com"),
                new[] { new EmailAddress("to@example.com") }, "Subject", "Body", null, new Dictionary<string,string>(), new List<string>(), "tenant", Guid.NewGuid().ToString());

            var result = await handler.HandleAsync(new SendEmailCommand(msg));

            Assert.True(result.Success);
            repo.Verify(r => r.SaveAsync(msg, result, default), Times.Once);
        }
    }
}
