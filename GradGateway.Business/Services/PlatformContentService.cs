using System.Text.Json;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class PlatformContentService : IPlatformContentService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Faq",
        "Guide",
        "Article",
        "Legal",
    };

    private static readonly HashSet<string> AllowedSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "Public",
        "HelpCenter",
        "Contact",
        "Legal",
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Published",
        "Draft",
    };

    private static readonly HashSet<string> AllowedAudiences = new(StringComparer.OrdinalIgnoreCase)
    {
        "Student",
        "Company",
        "All",
    };

    private readonly GradGatewayDbContext _context;

    public PlatformContentService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PublicPlatformContentDto>> GetPublishedAsync(
        string? contentType = null,
        string? section = null,
        string? audience = null,
        string? slug = null)
    {
        var query = _context.PlatformContents
            .AsNoTracking()
            .Where(c => c.Status == "Published");

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var type = NormalizeContentType(contentType);
            query = query.Where(c => c.ContentType == type);
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            var sec = NormalizeSection(section);
            query = query.Where(c => c.Section == sec);
        }

        if (!string.IsNullOrWhiteSpace(slug))
        {
            var normalizedSlug = slug.Trim();
            query = query.Where(c => c.Slug == normalizedSlug);
        }

        var rows = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(audience))
        {
            var aud = NormalizeAudience(audience);
            rows = rows
                .Where(c => ParseAudiences(c.Audiences).Contains(aud) || ParseAudiences(c.Audiences).Contains("All"))
                .ToList();
        }

        return rows.Select(ToPublicDto).ToList();
    }

    public async Task<IReadOnlyList<AdminPlatformContentListItemDto>> GetAdminListAsync(
        string? contentType = null,
        string? section = null,
        string? status = null)
    {
        var query = _context.PlatformContents
            .AsNoTracking()
            .Where(c => c.Section != "StudentDashboard");

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var type = NormalizeContentType(contentType);
            query = query.Where(c => c.ContentType == type);
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            var sec = NormalizeSection(section);
            query = query.Where(c => c.Section == sec);
        }

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = NormalizeStatus(status);
            query = query.Where(c => c.Status == normalized);
        }

        var rows = await query
            .OrderBy(c => c.ContentType)
            .ThenBy(c => c.Section)
            .ThenBy(c => c.SortOrder)
            .ThenByDescending(c => c.UpdatedAt)
            .ToListAsync();

        return rows.Select(ToAdminDto).ToList();
    }

    public async Task<AdminPlatformContentListItemDto> CreateAsync(AdminCreatePlatformContentDto dto)
    {
        ValidateDto(dto.ContentType, dto.Section, dto.Title, dto.Body, dto.Status, dto.Audiences);

        var entity = new PlatformContent
        {
            Id = Guid.NewGuid(),
            ContentType = NormalizeContentType(dto.ContentType),
            Section = NormalizeSection(dto.Section),
            Title = dto.Title.Trim(),
            Body = dto.Body.Trim(),
            Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim(),
            StepsJson = SerializeSteps(dto.Steps),
            Audiences = SerializeAudiences(dto.Audiences),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim(),
            Slug = string.IsNullOrWhiteSpace(dto.Slug) ? null : dto.Slug.Trim(),
            RelatedLinkHref = string.IsNullOrWhiteSpace(dto.RelatedLinkHref) ? null : dto.RelatedLinkHref.Trim(),
            RelatedLinkLabel = string.IsNullOrWhiteSpace(dto.RelatedLinkLabel) ? null : dto.RelatedLinkLabel.Trim(),
            Status = NormalizeStatus(dto.Status ?? "Draft"),
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.PlatformContents.Add(entity);
        await _context.SaveChangesAsync();
        return ToAdminDto(entity);
    }

    public async Task<AdminPlatformContentListItemDto> UpdateAsync(Guid id, AdminUpdatePlatformContentDto dto)
    {
        ValidateDto(dto.ContentType, dto.Section, dto.Title, dto.Body, dto.Status, dto.Audiences);

        var entity = await _context.PlatformContents.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("Content item not found.");

        if (string.Equals(entity.Section, "StudentDashboard", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This content is managed by the platform and cannot be edited.");
        }

        entity.ContentType = NormalizeContentType(dto.ContentType);
        entity.Section = NormalizeSection(dto.Section);
        entity.Title = dto.Title.Trim();
        entity.Body = dto.Body.Trim();
        entity.Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim();
        entity.StepsJson = SerializeSteps(dto.Steps);
        entity.Audiences = SerializeAudiences(dto.Audiences);
        entity.Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        entity.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? null : dto.Slug.Trim();
        entity.RelatedLinkHref = string.IsNullOrWhiteSpace(dto.RelatedLinkHref) ? null : dto.RelatedLinkHref.Trim();
        entity.RelatedLinkLabel = string.IsNullOrWhiteSpace(dto.RelatedLinkLabel) ? null : dto.RelatedLinkLabel.Trim();
        entity.Status = NormalizeStatus(dto.Status ?? entity.Status);
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToAdminDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.PlatformContents.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("Content item not found.");

        if (string.Equals(entity.Section, "StudentDashboard", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This content is managed by the platform and cannot be deleted.");
        }

        _context.PlatformContents.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private static void ValidateDto(
        string contentType,
        string section,
        string title,
        string body,
        string? status,
        IReadOnlyList<string>? audiences)
    {
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException("Invalid content type.");
        }

        if (!AllowedSections.Contains(section))
        {
            throw new ArgumentException("Invalid section.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(body)
            && !string.Equals(contentType, "Guide", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Body is required.");
        }

        if (!string.IsNullOrWhiteSpace(status) && !AllowedStatuses.Contains(status))
        {
            throw new ArgumentException("Invalid status.");
        }

        foreach (var audience in audiences ?? Array.Empty<string>())
        {
            if (!AllowedAudiences.Contains(audience))
            {
                throw new ArgumentException($"Invalid audience: {audience}");
            }
        }
    }

    private static string NormalizeContentType(string value) =>
        AllowedContentTypes.First(t => t.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeSection(string value) =>
        AllowedSections.First(s => s.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeStatus(string value) =>
        AllowedStatuses.First(s => s.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeAudience(string value) =>
        AllowedAudiences.First(a => a.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static string SerializeAudiences(IReadOnlyList<string>? audiences)
    {
        var list = (audiences ?? Array.Empty<string>())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => NormalizeAudience(a.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return list.Count == 0 ? "All" : string.Join(",", list);
    }

    private static string? SerializeSteps(IReadOnlyList<string>? steps)
    {
        var list = (steps ?? Array.Empty<string>())
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return list.Count == 0 ? null : JsonSerializer.Serialize(list);
    }

    private static IReadOnlyList<string> ParseAudiences(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new[] { "All" };
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static IReadOnlyList<string> ParseSteps(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static PublicPlatformContentDto ToPublicDto(PlatformContent entity) =>
        new(
            entity.Id,
            entity.ContentType,
            entity.Section,
            entity.Title,
            entity.Body,
            entity.Summary,
            ParseSteps(entity.StepsJson),
            ParseAudiences(entity.Audiences),
            entity.Category,
            entity.Slug,
            entity.RelatedLinkHref,
            entity.RelatedLinkLabel,
            entity.SortOrder);

    private static AdminPlatformContentListItemDto ToAdminDto(PlatformContent entity) =>
        new(
            entity.Id,
            entity.ContentType,
            entity.Section,
            entity.Title,
            entity.Body,
            entity.Summary,
            ParseSteps(entity.StepsJson),
            ParseAudiences(entity.Audiences),
            entity.Category,
            entity.Slug,
            entity.RelatedLinkHref,
            entity.RelatedLinkLabel,
            entity.Status,
            entity.SortOrder,
            entity.CreatedAt,
            entity.UpdatedAt);
}
