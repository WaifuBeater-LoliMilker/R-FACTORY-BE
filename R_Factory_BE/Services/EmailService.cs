using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace R_Factory_BE.Services;

/// <summary>
/// SMTP settings bound from the "SmtpSettings" appsettings section.
/// When <see cref="Enabled"/> is false (default), emails are silently skipped.
/// </summary>
public class SmtpSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public string FromDisplayName { get; set; } = "R_Factory Monitor";
    public bool EnableSsl { get; set; } = true;
    /// <summary>Comma- or semicolon-separated list of recipient addresses.</summary>
    public string Recipients { get; set; } = "";
}

public interface IEmailService
{
    /// <summary>
    /// Sends a plain-text email to the configured recipients.
    /// No-op when SMTP is disabled or there are no recipients.
    /// </summary>
    Task SendWarningAsync(string subject, string body, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendWarningAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("SmtpSettings.Enabled is false; skipping email send.");
            return;
        }

        var recipients = (_settings.Recipients ?? string.Empty)
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (recipients.Length == 0)
        {
            _logger.LogWarning("SMTP enabled but no recipients configured; skipping email send.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.From))
        {
            _logger.LogWarning("SMTP host or From address not configured; skipping email send.");
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.From, _settings.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        foreach (var recipient in recipients)
        {
            message.To.Add(recipient);
        }

#pragma warning disable CS0618 // System.Net.Mail.SmtpClient is obsolete but still supported in .NET 8.
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await client.SendMailAsync(message, cancellationToken);
#pragma warning restore CS0618

        _logger.LogInformation("Warning email sent to {Recipients}.", string.Join(", ", recipients));
    }
}
