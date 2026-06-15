namespace GradGateway.Business.DTOs;

public record PublicPlatformContentDto(
    Guid Id,
    string ContentType,
    string Section,
    string Title,
    string Body,
    string? Summary,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Audiences,
    string? Category,
    string? Slug,
    string? RelatedLinkHref,
    string? RelatedLinkLabel,
    int SortOrder);

public record AdminPlatformContentListItemDto(
    Guid Id,
    string ContentType,
    string Section,
    string Title,
    string Body,
    string? Summary,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Audiences,
    string? Category,
    string? Slug,
    string? RelatedLinkHref,
    string? RelatedLinkLabel,
    string Status,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record AdminCreatePlatformContentDto(
    string ContentType,
    string Section,
    string Title,
    string Body,
    string? Summary,
    IReadOnlyList<string>? Steps,
    IReadOnlyList<string>? Audiences,
    string? Category,
    string? Slug,
    string? RelatedLinkHref,
    string? RelatedLinkLabel,
    string? Status,
    int SortOrder);

public record AdminUpdatePlatformContentDto(
    string ContentType,
    string Section,
    string Title,
    string Body,
    string? Summary,
    IReadOnlyList<string>? Steps,
    IReadOnlyList<string>? Audiences,
    string? Category,
    string? Slug,
    string? RelatedLinkHref,
    string? RelatedLinkLabel,
    string? Status,
    int SortOrder);
