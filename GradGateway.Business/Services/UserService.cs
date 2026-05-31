using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;

namespace GradGateway.Business.Services;

public class UserService : IUserService
{
    private readonly GradGatewayDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IFirebaseAdminService _firebaseAdmin;
    private readonly ILogger<UserService> _logger;
    private readonly IHostEnvironment _environment;

    public UserService(
        GradGatewayDbContext context,
        IEmailSender emailSender,
        IFirebaseAdminService firebaseAdmin,
        ILogger<UserService> logger,
        IHostEnvironment environment)
    {
        _context = context;
        _emailSender = emailSender;
        _firebaseAdmin = firebaseAdmin;
        _logger = logger;
        _environment = environment;
    }

    public async Task<UserResponseDto?> GetOrCreateUserAsync(UserRegistrationDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == dto.FirebaseUid);
        
        if (user == null)
        {
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var userRole))
            {
                throw new ArgumentException($"Invalid role: {dto.Role}");
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = dto.FirebaseUid,
                Email = dto.Email,
                Role = userRole,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return new UserResponseDto(user.Email, user.Role.ToString(), user.FirebaseUid);
    }

    public async Task<UserResponseDto?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        
        if (user == null)
        {
            return null;
        }

        return new UserResponseDto(user.Email, user.Role.ToString(), user.FirebaseUid);
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<ForgotPasswordResponseDto> RequestPasswordResetAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            return new ForgotPasswordResponseDto("If an account exists with this email, a reset code will be sent.", true);
        }

        var resetCode = GenerateResetCode();

        var existingTokens = await _context.PasswordResetTokens
            .Where(t => !t.IsUsed && t.Email.ToLower() == normalizedEmail)
            .ToListAsync();
        foreach (var token in existingTokens)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;
        }

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = user.Email,
            Token = resetCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        try
        {
            await _emailSender.SendPasswordResetCodeAsync(normalizedEmail, resetCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", normalizedEmail);

            if (_environment.IsDevelopment())
            {
                _logger.LogWarning(
                    "Development fallback: password reset code for {Email} is {Code}",
                    normalizedEmail,
                    resetCode);
                return new ForgotPasswordResponseDto(
                    "Email could not be sent (check Gmail app password). For development, see the API console for your reset code.",
                    true);
            }

            return new ForgotPasswordResponseDto("Unable to send reset code right now. Please try again later.", false);
        }

        return new ForgotPasswordResponseDto("If an account exists with this email, a reset code will be sent.", true);
    }

    public async Task<VerifyCodeResponseDto> VerifyResetCodeAsync(string email, string code)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = code.Trim();

        var resetToken = await FindValidResetTokenAsync(normalizedEmail, normalizedCode);
        if (resetToken == null)
        {
            return new VerifyCodeResponseDto("Invalid or expired reset code.", false);
        }

        return new VerifyCodeResponseDto("Reset code verified successfully.", true);
    }

    public async Task<ForgotPasswordResponseDto> ResetPasswordAsync(string email, string code, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return new ForgotPasswordResponseDto("Password must be at least 6 characters.", false);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = code.Trim();

        var resetToken = await FindValidResetTokenAsync(normalizedEmail, normalizedCode);
        if (resetToken == null)
        {
            return new ForgotPasswordResponseDto("Invalid or expired reset code.", false);
        }

        try
        {
            await _firebaseAdmin.UpdatePasswordByEmailAsync(normalizedEmail, newPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Firebase password for {Email}", normalizedEmail);
            return new ForgotPasswordResponseDto("Unable to reset password. Please contact support.", false);
        }

        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;
        _context.PasswordResetTokens.Update(resetToken);
        await _context.SaveChangesAsync();

        return new ForgotPasswordResponseDto("Password reset successful.", true);
    }

    private async Task<PasswordResetToken?> FindValidResetTokenAsync(string normalizedEmail, string code)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.Email.ToLower() == normalizedEmail &&
                t.Token == code &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow);
    }

    private static string GenerateResetCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}
