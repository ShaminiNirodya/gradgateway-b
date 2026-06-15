using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class TestimonialService : ITestimonialService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Published",
        "Rejected",
    };

    private readonly GradGatewayDbContext _context;

    public TestimonialService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PublicTestimonialDto>> GetPublishedAsync(int? limit = null)
    {
        var rows = await _context.Testimonials
            .AsNoTracking()
            .Where(t => t.Status == "Published")
            .ToListAsync();

        var studentSide = rows
            .Where(IsStudentPerspective)
            .OrderByDescending(t => t.PublishedAt ?? t.CreatedAt)
            .ToList();

        var companySide = rows
            .Where(t => !IsStudentPerspective(t))
            .OrderByDescending(t => t.PublishedAt ?? t.CreatedAt)
            .ToList();

        var balanced = new List<Testimonial>();
        var studentIndex = 0;
        var companyIndex = 0;
        var max = limit is > 0 ? limit.Value : int.MaxValue;

        while (balanced.Count < max && (studentIndex < studentSide.Count || companyIndex < companySide.Count))
        {
            if (studentIndex < studentSide.Count)
            {
                balanced.Add(studentSide[studentIndex++]);
                if (balanced.Count >= max) break;
            }

            if (companyIndex < companySide.Count)
            {
                balanced.Add(companySide[companyIndex++]);
            }
        }

        return balanced
            .Select(t => new PublicTestimonialDto(t.Id, t.Quote, t.AuthorName, t.AuthorRole))
            .ToList();
    }

    private static bool IsStudentPerspective(Testimonial testimonial)
    {
        if (string.Equals(testimonial.SubmitterRole, "Student", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(testimonial.SubmitterRole, "Company", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Legacy seeded rows without submitter metadata.
        return testimonial.AuthorName.Contains("Undergraduate", StringComparison.OrdinalIgnoreCase)
            || testimonial.AuthorName.Contains("Student", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<TestimonialListItemDto> SubmitAsync(SubmitTestimonialDto dto, Guid? submittedByUserId)
    {
        ValidateContent(dto.Quote, dto.AuthorName, dto.AuthorRole);

        var row = new Testimonial
        {
            Id = Guid.NewGuid(),
            Quote = dto.Quote.Trim(),
            AuthorName = dto.AuthorName.Trim(),
            AuthorRole = dto.AuthorRole.Trim(),
            Status = "Pending",
            SortOrder = await NextSortOrderAsync(),
            SubmitterEmail = NormalizeEmail(dto.Email),
            SubmitterRole = NormalizeOptional(dto.SubmitterRole),
            SubmittedByUserId = submittedByUserId,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Testimonials.Add(row);
        await _context.SaveChangesAsync();

        return ToDto(row);
    }

    public async Task<PagedResultDto<TestimonialListItemDto>> GetAdminListAsync(
        string? status,
        int page,
        int pageSize)
    {
        var normalizedPage = Math.Max(1, page);
        var (pageNum, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        normalizedPage = pageNum;

        var query = _context.Testimonials.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) &&
            !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedStatus = NormalizeStatus(status);
            query = query.Where(t => t.Status == normalizedStatus);
        }

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var items = rows.Select(ToDto).ToList();
        return new PagedResultDto<TestimonialListItemDto>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<TestimonialListItemDto> CreateAdminAsync(AdminCreateTestimonialDto dto)
    {
        ValidateContent(dto.Quote, dto.AuthorName, dto.AuthorRole);

        var status = string.IsNullOrWhiteSpace(dto.Status) ? "Published" : NormalizeStatus(dto.Status);
        var now = DateTime.UtcNow;

        var row = new Testimonial
        {
            Id = Guid.NewGuid(),
            Quote = dto.Quote.Trim(),
            AuthorName = dto.AuthorName.Trim(),
            AuthorRole = dto.AuthorRole.Trim(),
            Status = status,
            SortOrder = dto.SortOrder ?? await NextSortOrderAsync(),
            CreatedAt = now,
            PublishedAt = status == "Published" ? now : null,
            ReviewedAt = status != "Pending" ? now : null,
        };

        _context.Testimonials.Add(row);
        await _context.SaveChangesAsync();

        return ToDto(row);
    }

    public async Task<TestimonialListItemDto> UpdateAsync(Guid id, AdminUpdateTestimonialDto dto)
    {
        ValidateContent(dto.Quote, dto.AuthorName, dto.AuthorRole);

        var row = await _context.Testimonials.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new InvalidOperationException("Testimonial not found.");

        row.Quote = dto.Quote.Trim();
        row.AuthorName = dto.AuthorName.Trim();
        row.AuthorRole = dto.AuthorRole.Trim();
        row.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync();
        return ToDto(row);
    }

    public async Task<TestimonialListItemDto> SetStatusAsync(Guid id, string status)
    {
        var normalizedStatus = NormalizeStatus(status);
        var row = await _context.Testimonials.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new InvalidOperationException("Testimonial not found.");

        var now = DateTime.UtcNow;
        row.Status = normalizedStatus;
        row.ReviewedAt = now;

        if (normalizedStatus == "Published")
        {
            row.PublishedAt = now;
        }
        else
        {
            row.PublishedAt = null;
        }

        await _context.SaveChangesAsync();
        return ToDto(row);
    }

    public async Task DeleteAsync(Guid id)
    {
        var row = await _context.Testimonials.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new InvalidOperationException("Testimonial not found.");

        _context.Testimonials.Remove(row);
        await _context.SaveChangesAsync();
    }

    private async Task<int> NextSortOrderAsync()
    {
        var max = await _context.Testimonials.MaxAsync(t => (int?)t.SortOrder) ?? 0;
        return max + 1;
    }

    private static void ValidateContent(string quote, string authorName, string authorRole)
    {
        if (string.IsNullOrWhiteSpace(quote) ||
            string.IsNullOrWhiteSpace(authorName) ||
            string.IsNullOrWhiteSpace(authorRole))
        {
            throw new ArgumentException("Quote, display name, and role are required.");
        }

        if (quote.Trim().Length > 500)
        {
            throw new ArgumentException("Quote must be 500 characters or fewer.");
        }

        if (authorName.Trim().Length > 120 || authorRole.Trim().Length > 120)
        {
            throw new ArgumentException("Display name and role must be 120 characters or fewer.");
        }
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new ArgumentException("Status must be Pending, Published, or Rejected.");
        }

        return AllowedStatuses.First(s => s.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    internal static TestimonialListItemDto ToDto(Testimonial row) =>
        new(
            row.Id,
            row.Quote,
            row.AuthorName,
            row.AuthorRole,
            row.Status,
            row.SortOrder,
            row.SubmitterEmail,
            row.SubmitterRole,
            row.SubmittedByUserId,
            row.CreatedAt,
            row.PublishedAt,
            row.ReviewedAt);
}
