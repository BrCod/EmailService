namespace EmailPublisher
{
    public sealed class EmailPublisherOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string Queue { get; set; } = "email.send";
        public bool UsePublisherConfirms { get; set; } = true;
        public string Source { get; set; } = "email-publisher-client";
    }
}
