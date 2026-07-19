using System.Text.Json.Serialization;

namespace GradGateway.Business.DTOs;

public record ApplyRequestDto(
    Guid OpportunityId,
    string? CoverLetter
);

public sealed class UpdateApplicationStatusRequestDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class RespondToJobOfferRequestDto
{
    [JsonPropertyName("accepted")]
    public bool Accepted { get; set; }

    [JsonPropertyName("applicationId")]
    public Guid? ApplicationId { get; set; }
}

public record CreateJobOfferRequestDto(
    Guid StudentProfileId,
    string JobTitle,
    string JobType,
    string? Compensation,
    string ProposalMessage,
    Guid? OpportunityId = null
);

public record ApplicationResponseDto(
    Guid Id,
    Guid? OpportunityId,
    Guid StudentProfileId,
    string JobTitle,
    string CompanyName,
    string StudentName,
    string StudentEmail,
    string? CoverLetter,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAt,
    Guid? CompanyProfileId = null
);
