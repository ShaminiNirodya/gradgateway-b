using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using GradGateway.Business.Interfaces;
using GradGateway.Business.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GradGateway.Business.Services;

public class FirebaseAdminAuthService : IFirebaseAdminService
{
    private readonly FirebaseAdminOptions _options;
    private readonly ILogger<FirebaseAdminAuthService> _logger;
    private static readonly object InitLock = new();
    private static bool _initialized;

    public FirebaseAdminAuthService(IOptions<FirebaseAdminOptions> options, ILogger<FirebaseAdminAuthService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var uid = await GetFirebaseUidByEmailAsync(email, cancellationToken);
        return uid != null;
    }

    public async Task<string?> GetFirebaseUidByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (!TryEnsureInitialized())
        {
            return null;
        }

        try
        {
            var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, cancellationToken);
            return user.Uid;
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            return null;
        }
    }

    public async Task UpdatePasswordByEmailAsync(string email, string newPassword, CancellationToken cancellationToken = default)
    {
        if (!TryEnsureInitialized())
        {
            throw new InvalidOperationException(
                "Firebase Admin is not configured. Set Firebase:ServiceAccountPath in appsettings.");
        }
        var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, cancellationToken);
        await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
        {
            Uid = user.Uid,
            Password = newPassword,
        }, cancellationToken);

        _logger.LogInformation("Firebase password updated for {Email}", email);
    }

    private bool TryEnsureInitialized()
    {
        if (_initialized)
        {
            return true;
        }

        lock (InitLock)
        {
            if (_initialized)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(_options.ProjectId))
            {
                _logger.LogWarning("Firebase:ProjectId is not configured.");
                return false;
            }

            if (FirebaseApp.DefaultInstance == null)
            {
                if (string.IsNullOrWhiteSpace(_options.ServiceAccountPath) || !File.Exists(_options.ServiceAccountPath))
                {
                    _logger.LogWarning(
                        "Firebase Admin service account file not found at {Path}",
                        _options.ServiceAccountPath ?? "(not set)");
                    return false;
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(_options.ServiceAccountPath),
                    ProjectId = _options.ProjectId,
                });
            }

            _initialized = true;
            return true;
        }
    }
}
