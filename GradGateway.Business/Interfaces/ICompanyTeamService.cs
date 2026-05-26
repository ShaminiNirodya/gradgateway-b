using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface ICompanyTeamService
{
    Task<TeamMemberResponseDto> InviteMemberAsync(string firebaseUid, InviteTeamMemberRequestDto dto);
    Task<List<TeamMemberResponseDto>> GetMembersAsync(string firebaseUid);
    Task RemoveMemberAsync(string firebaseUid, Guid memberId);
    Task<TeamMemberResponseDto> AcceptInviteAsync(AcceptTeamInviteRequestDto dto);
}
