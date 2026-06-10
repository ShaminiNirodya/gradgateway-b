using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;

namespace GradGateway.Business.Services;

public class SupportInquiryService : ISupportInquiryService
{
    private readonly GradGatewayDbContext _context;

    public SupportInquiryService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<SupportInquiryListItemDto> SubmitAsync(SubmitSupportInquiryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Type) ||
            string.IsNullOrWhiteSpace(dto.Message))
        {
            throw new ArgumentException("Name, email, type, and message are required.");
        }

        var row = new SupportInquiry
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            InquiryType = dto.Type.Trim(),
            Message = dto.Message.Trim(),
            AttachmentName = string.IsNullOrWhiteSpace(dto.AttachmentName) ? null : dto.AttachmentName.Trim(),
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        _context.SupportInquiries.Add(row);
        await _context.SaveChangesAsync();

        return ToDto(row);
    }

    internal static SupportInquiryListItemDto ToDto(SupportInquiry row) =>
        new(
            row.Id,
            row.Name,
            row.Email,
            row.Phone,
            row.InquiryType,
            row.Message,
            row.AttachmentName,
            row.Status,
            row.CreatedAt,
            row.ReviewedAt);
}
