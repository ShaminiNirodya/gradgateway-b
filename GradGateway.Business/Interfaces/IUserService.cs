using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetOrCreateUserAsync(UserRegistrationDto dto);
    Task<UserResponseDto?> GetUserByFirebaseUidAsync(string firebaseUid);
    Task<int> GetUserCountAsync();
}
