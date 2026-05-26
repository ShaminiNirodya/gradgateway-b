namespace GradGateway.Business.DTOs;

public record ProjectImageDto(
    Guid Id,
    string ImageUrl, // Data URI or URL to image
    int DisplayOrder
);
