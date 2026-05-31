namespace GradGateway.Business.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetCodeAsync(string toEmail, string code, CancellationToken cancellationToken = default);
}
