using EmailService.Application.Commands.SendEmail;
using EmailService.Application.Interfaces;
using EmailService.Infrastructure.Config;
using EmailService.Infrastructure.Email;
using EmailService.Infrastructure.Persistence;
using EmailService.Infrastructure.RabbitMq;
using EmailService.Shared.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Options
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

// Health checks
builder.Services.AddHealthChecks();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("EmailService.Api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("EmailService")
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter());

// Metrics (Prometheus)
builder.Services.AddSingleton<JsonSerializerOptionsFactory>();

// DI registrations
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IEmailMessageRepository, EmailMessageRepository>();
builder.Services.AddSingleton<EmailService.Domain.Services.EmailValidationService>();
builder.Services.AddSingleton<SendEmailCommandHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// Health endpoints
app.MapHealthChecks("/health", new HealthCheckOptions());

// Metrics endpoint
app.UseRouting();
app.UseHttpMetrics();
app.MapMetrics();

// Minimal root
app.MapGet("/", () => Results.Ok(new { service = "EmailService.Api", status = "ok" }));

app.Run();
