using EmailPublisher;
using EmailPublisher.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddEmailPublisher(options =>
        {
            options.HostName = "localhost";
            options.Port = 5672;
            options.UserName = "guest";
            options.Password = "guest";
            options.VirtualHost = "/";
            options.Queue = "email.send";
        });
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var emailPublisher = host.Services.GetRequiredService<IEmailPublisher>();

try
{
    Console.WriteLine("Email Service Test Client");
    Console.WriteLine("==========================\n");

    var emailMessage = new EmailMessage
    {
        To = new List<EmailAddress>
        {
            new EmailAddress { Email = "test@example.com", Name = "Test User" }
        },
        Subject = "Test Email from Docker",
        Body = "Hello! This is a test email sent through the Docker-based email service.",
        IsHtml = false
    };

    Console.WriteLine($"Sending email to: {emailMessage.To[0].Email}");
    Console.WriteLine($"Subject: {emailMessage.Subject}\n");

    await emailPublisher.PublishEmailAsync(emailMessage);

    Console.WriteLine("✓ Email message published to RabbitMQ successfully!");
    Console.WriteLine("\nNext steps:");
    Console.WriteLine("1. Check RabbitMQ Management UI: http://localhost:15672");
    Console.WriteLine("2. View the email in MailHog: http://localhost:8025");
    Console.WriteLine("3. Check service logs: docker-compose logs -f email-service");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error publishing email");
    Console.WriteLine($"\n✗ Error: {ex.Message}");
}
finally
{
    await host.StopAsync();
}
