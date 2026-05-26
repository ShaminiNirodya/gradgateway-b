namespace GradGateway.Business.DTOs;

public record InviteTeamMemberRequestDto(
    string Name,
    string Email,
    string Role
);

public record TeamMemberResponseDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string Status,
    DateTime InvitedAt,
    DateTime? AcceptedAt
);

public record AcceptTeamInviteRequestDto(
    string Token
);
