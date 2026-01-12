using EmailService.Application.Commands.SendEmail;
using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Infrastructure.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService.Infrastructure.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opts;
        private readonly ILogger<SmtpEmailSender> _logger;
        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _opts = options.Value;
            _logger = logger;
        }

        public async Task<SendEmailResult> SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(message.From.Value, message.From.Value));
            foreach (var to in message.To)
                mimeMessage.To.Add(new MailboxAddress(to.Value, to.Value));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = message.Body ?? string.Empty;
            if (message.Attachments?.Count > 0)
            {
                foreach (var att in message.Attachments)
                {
                    // Attachment representation is opaque; treat as file paths if exist
                    if (System.IO.File.Exists(att))
                    {
                        bodyBuilder.Attachments.Add(att);
                    }
                }
            }
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var secure = _opts.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
                await client.ConnectAsync(_opts.Host, _opts.Port, secure, ct);
                if (!string.IsNullOrWhiteSpace(_opts.UserName))
                {
                    await client.AuthenticateAsync(_opts.UserName, _opts.Password, ct);
                }

                await client.SendAsync(mimeMessage, ct);
                await client.DisconnectAsync(true, ct);

                return new SendEmailResult { Status = "Sent", ProviderMessageId = mimeMessage.MessageId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP send failed");
                return new SendEmailResult { Status = "Failed", Error = ex.Message };
            }
        }
    }
}
