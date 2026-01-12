namespace EmailService.Infrastructure.Config
{
    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public bool UseSsl { get; set; } = false;
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string FromAddress { get; set; } = "no-reply@example.com";
    }
}
