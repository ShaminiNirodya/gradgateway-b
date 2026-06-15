namespace GradGateway.Business.DTOs;

public record PublicTestimonialDto(
    Guid Id,
    string Quote,
    string AuthorName,
    string AuthorRole);

public record TestimonialListItemDto(
    Guid Id,
    string Quote,
    string AuthorName,
    string AuthorRole,
    string Status,
    int SortOrder,
    string? SubmitterEmail,
    string? SubmitterRole,
    Guid? SubmittedByUserId,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    DateTime? ReviewedAt);

public record SubmitTestimonialDto(
    string Quote,
    string AuthorName,
    string AuthorRole,
    string? Email,
    string? SubmitterRole);

public record AdminCreateTestimonialDto(
    string Quote,
    string AuthorName,
    string AuthorRole,
    string? Status,
    int? SortOrder);

public record AdminUpdateTestimonialDto(
    string Quote,
    string AuthorName,
    string AuthorRole,
    int SortOrder);

public record AdminSetTestimonialStatusDto(string Status);
