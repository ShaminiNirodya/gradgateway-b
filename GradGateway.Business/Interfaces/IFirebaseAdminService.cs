namespace GradGateway.Business.Interfaces;

public interface IFirebaseAdminService
{
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<string?> GetFirebaseUidByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdatePasswordByEmailAsync(string email, string newPassword, CancellationToken cancellationToken = default);
}
