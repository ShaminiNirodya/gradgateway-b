using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetOrCreateUserAsync(UserRegistrationDto dto);
    Task<UserResponseDto?> GetUserByFirebaseUidAsync(string firebaseUid);
    Task<int> GetUserCountAsync();
    Task<ForgotPasswordResponseDto> RequestPasswordResetAsync(string email);
    Task<VerifyCodeResponseDto> VerifyResetCodeAsync(string email, string code);
    Task<ForgotPasswordResponseDto> ResetPasswordAsync(string email, string code, string newPassword);
}
