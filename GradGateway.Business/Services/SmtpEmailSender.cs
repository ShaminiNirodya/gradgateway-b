using GradGateway.Business.Interfaces;
using GradGateway.Business.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GradGateway.Business.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetCodeAsync(string toEmail, string code, CancellationToken cancellationToken = default)
    {
        var subject = "Your GradGateway password reset code";
        var body = $"""
            Hello,

            Your password reset code is: {code}

            This code expires in 1 hour. If you did not request a password reset, you can ignore this email.

            — GradGateway
            """;

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning(
                "SMTP is not configured. Password reset code for {Email}: {Code}",
                toEmail,
                code);
            return;
        }

        var smtpUser = _options.SmtpUser?.Trim();
        var smtpPassword = _options.SmtpPassword?.Trim().Replace(" ", string.Empty);

        if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
        {
            throw new InvalidOperationException("Email:SmtpUser and Email:SmtpPassword must be set when Email:Enabled is true.");
        }

        if (smtpPassword.Length != 16)
        {
            _logger.LogWarning(
                "Gmail app passwords are usually 16 characters; SmtpPassword length is {Length}. Check appsettings.Development.json.",
                smtpPassword.Length);
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress.Trim()));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        var socketOption = _options.SmtpPort switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, socketOption, cancellationToken);
        await client.AuthenticateAsync(smtpUser, smtpPassword, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Password reset code email sent to {Email}", toEmail);
    }
}
