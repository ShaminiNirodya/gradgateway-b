namespace GradGateway.Business.DTOs;

public record UserResponseDto(string Email, string Role, string FirebaseUid, bool IsActive = true);
public record UserRegistrationDto(string Email, string FirebaseUid, string Role);
