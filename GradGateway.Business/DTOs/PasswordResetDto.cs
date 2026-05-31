namespace GradGateway.Business.DTOs;

public record ForgotPasswordRequestDto(string Email);

public record VerifyResetCodeRequestDto(string Email, string Code);

public record ResetPasswordRequestDto(string Email, string Code, string NewPassword);

public record ForgotPasswordResponseDto(string Message, bool Success);

public record VerifyCodeResponseDto(string Message, bool Valid);
