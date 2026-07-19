using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class AcademicCatalogService : IAcademicCatalogService
{
    private readonly GradGatewayDbContext _context;

    public AcademicCatalogService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CatalogUniversityAdminDto>> GetAdminUniversitiesAsync(bool includeHidden = true)
    {
        var query = _context.CatalogUniversities.AsNoTracking();
        if (!includeHidden)
        {
            query = query.Where(u => u.IsActive);
        }

        var rows = await query
            .OrderBy(u => u.SortOrder)
            .ThenBy(u => u.Name)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.IsActive,
                u.SortOrder,
                u.UpdatedAt,
                DegreeCount = u.Offerings.Count(o => o.IsActive && o.Degree.IsActive),
            })
            .ToListAsync();

        return rows
            .Select(u => new CatalogUniversityAdminDto(
                u.Id,
                u.Name,
                u.IsActive,
                u.SortOrder,
                u.DegreeCount,
                u.UpdatedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<CatalogDegreeAdminDto>> GetAdminDegreesAsync(bool includeHidden = true)
    {
        var query = _context.CatalogDegrees.AsNoTracking();
        if (!includeHidden)
        {
            query = query.Where(d => d.IsActive);
        }

        var rows = await query
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.IsActive,
                d.SortOrder,
                d.UpdatedAt,
                UniversityCount = d.Offerings.Count(o => o.IsActive && o.University.IsActive),
            })
            .ToListAsync();

        return rows
            .Select(d => new CatalogDegreeAdminDto(
                d.Id,
                d.Name,
                d.IsActive,
                d.SortOrder,
                d.UniversityCount,
                d.UpdatedAt))
            .ToList();
    }

    public async Task<CatalogUniversityDetailDto?> GetAdminUniversityAsync(Guid id)
    {
        var university = await _context.CatalogUniversities
            .AsNoTracking()
            .Include(u => u.Offerings)
            .ThenInclude(o => o.Degree)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (university == null)
        {
            return null;
        }

        return MapUniversityDetail(university);
    }

    public async Task<CatalogUniversityAdminDto> CreateUniversityAsync(UpsertCatalogUniversityDto dto)
    {
        var name = NormalizeName(dto.Name);
        await EnsureUniqueUniversityNameAsync(name, null);

        var now = DateTime.UtcNow;
        var entity = new CatalogUniversity
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _context.CatalogUniversities.Add(entity);
        await _context.SaveChangesAsync();

        return new CatalogUniversityAdminDto(entity.Id, entity.Name, entity.IsActive, entity.SortOrder, 0, entity.UpdatedAt);
    }

    public async Task<CatalogUniversityAdminDto> UpdateUniversityAsync(Guid id, UpsertCatalogUniversityDto dto)
    {
        var entity = await _context.CatalogUniversities.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("University not found.");

        var name = NormalizeName(dto.Name);
        await EnsureUniqueUniversityNameAsync(name, id);

        entity.Name = name;
        entity.IsActive = dto.IsActive;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var degreeCount = await _context.CatalogUniversityDegrees
            .CountAsync(o => o.UniversityId == id && o.IsActive && o.Degree.IsActive);

        return new CatalogUniversityAdminDto(entity.Id, entity.Name, entity.IsActive, entity.SortOrder, degreeCount, entity.UpdatedAt);
    }

    public async Task DeleteUniversityAsync(Guid id)
    {
        var entity = await _context.CatalogUniversities.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("University not found.");

        if (await IsUniversityReferencedAsync(entity.Name))
        {
            throw new InvalidOperationException(
                "This university is used on student profiles. Hide it instead of deleting.");
        }

        _context.CatalogUniversityDegrees.RemoveRange(
            await _context.CatalogUniversityDegrees.Where(o => o.UniversityId == id).ToListAsync());
        _context.CatalogUniversities.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<CatalogUniversityAdminDto> SetUniversityActiveAsync(Guid id, bool isActive)
    {
        var entity = await _context.CatalogUniversities.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("University not found.");

        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var degreeCount = await _context.CatalogUniversityDegrees
            .CountAsync(o => o.UniversityId == id && o.IsActive && o.Degree.IsActive);

        return new CatalogUniversityAdminDto(entity.Id, entity.Name, entity.IsActive, entity.SortOrder, degreeCount, entity.UpdatedAt);
    }

    public async Task<CatalogUniversityDetailDto> SetUniversityDegreesAsync(Guid id, SetCatalogUniversityDegreesDto dto)
    {
        var university = await _context.CatalogUniversities
            .Include(u => u.Offerings)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("University not found.");

        var requestedIds = dto.DegreeIds?.Distinct().ToList() ?? [];
        if (requestedIds.Count > 0)
        {
            var existingDegreeIds = await _context.CatalogDegrees
                .Where(d => requestedIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            if (existingDegreeIds.Count != requestedIds.Count)
            {
                throw new InvalidOperationException("One or more degrees were not found.");
            }
        }

        var existing = university.Offerings.ToDictionary(o => o.DegreeId);
        foreach (var offering in university.Offerings)
        {
            offering.IsActive = requestedIds.Contains(offering.DegreeId);
        }

        foreach (var degreeId in requestedIds)
        {
            if (!existing.ContainsKey(degreeId))
            {
                university.Offerings.Add(new CatalogUniversityDegree
                {
                    UniversityId = id,
                    DegreeId = degreeId,
                    IsActive = true,
                });
            }
        }

        university.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var refreshed = await _context.CatalogUniversities
            .AsNoTracking()
            .Include(u => u.Offerings)
            .ThenInclude(o => o.Degree)
            .FirstAsync(u => u.Id == id);

        return MapUniversityDetail(refreshed);
    }

    public async Task<CatalogDegreeAdminDto> CreateDegreeAsync(UpsertCatalogDegreeDto dto)
    {
        var name = NormalizeName(dto.Name);
        await EnsureUniqueDegreeNameAsync(name, null);

        var now = DateTime.UtcNow;
        var entity = new CatalogDegree
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _context.CatalogDegrees.Add(entity);
        await _context.SaveChangesAsync();

        return new CatalogDegreeAdminDto(entity.Id, entity.Name, entity.IsActive, entity.SortOrder, 0, entity.UpdatedAt);
    }

    public async Task<CatalogDegreeAdminDto> UpdateDegreeAsync(Guid id, UpsertCatalogDegreeDto dto)
    {
        var entity = await _context.CatalogDegrees.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException("Degree not found.");

        var name = NormalizeName(dto.Name);
        await EnsureUniqueDegreeNameAsync(name, id);

        entity.Name = name;
        entity.IsActive = dto.IsActive;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var universityCount = await _context.CatalogUniversityDegrees
            .CountAsync(o => o.DegreeId == id && o.IsActive && o.University.IsActive);

        return new CatalogDegreeAdminDto(entity.Id, entity.Name, entity.IsActive, entity.SortOrder, universityCount, entity.UpdatedAt);
    }

    public async Task DeleteDegreeAsync(Guid id)
    {
        var entity = await _context.CatalogDegrees.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException("Degree not found.");

        if (await IsDegreeReferencedAsync(entity.Name))
        {
            throw new InvalidOperationException(
                "This degree is used on student profiles. Hide it instead of deleting.");
        }

        _context.CatalogUniversityDegrees.RemoveRange(
            await _context.CatalogUniversityDegrees.Where(o => o.DegreeId == id).ToListAsync());
        _context.CatalogDegrees.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<CatalogDegreeAdminDto> SetDegreeActiveAsync(Guid id, bool isActive)
    {
        var entity = await _context.CatalogDegrees.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException("Degree not found.");

        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var universityCount = await _context.CatalogUniversityDegrees
            .CountAsync(o => o.DegreeId == id && o.IsActive && o.University.IsActive);

        return new CatalogDegreeAdminDto(entity.Id, entity.Name, entity.IsActive, entity.SortOrder, universityCount, entity.UpdatedAt);
    }

    public async Task<PublicAcademicCatalogDto> GetPublicCatalogAsync()
    {
        var universities = await _context.CatalogUniversities
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.SortOrder)
            .ThenBy(u => u.Name)
            .Include(u => u.Offerings)
            .ThenInclude(o => o.Degree)
            .ToListAsync();

        var publicUniversities = universities
            .Select(u => new PublicUniversityCatalogDto(
                u.Id,
                u.Name,
                u.Offerings
                    .Where(o => o.IsActive && o.Degree.IsActive)
                    .Select(o => o.Degree.Name)
                    .OrderBy(n => n)
                    .ToList()))
            .ToList();

        var degrees = await _context.CatalogDegrees
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Name)
            .Select(d => d.Name)
            .ToListAsync();

        return new PublicAcademicCatalogDto(publicUniversities, degrees);
    }

    private static CatalogUniversityDetailDto MapUniversityDetail(CatalogUniversity university) =>
        new(
            university.Id,
            university.Name,
            university.IsActive,
            university.SortOrder,
            university.Offerings
                .OrderBy(o => o.Degree.SortOrder)
                .ThenBy(o => o.Degree.Name)
                .Select(o => new CatalogDegreeLinkDto(
                    o.DegreeId,
                    o.Degree.Name,
                    o.Degree.IsActive,
                    o.IsActive))
                .ToList(),
            university.UpdatedAt);

    private static string NormalizeName(string name) =>
        string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Name is required.") : name.Trim();

    private async Task EnsureUniqueUniversityNameAsync(string name, Guid? excludeId)
    {
        var exists = await _context.CatalogUniversities.AnyAsync(u =>
            u.Name == name && (!excludeId.HasValue || u.Id != excludeId.Value));
        if (exists)
        {
            throw new InvalidOperationException("A university with this name already exists.");
        }
    }

    private async Task EnsureUniqueDegreeNameAsync(string name, Guid? excludeId)
    {
        var exists = await _context.CatalogDegrees.AnyAsync(d =>
            d.Name == name && (!excludeId.HasValue || d.Id != excludeId.Value));
        if (exists)
        {
            throw new InvalidOperationException("A degree with this name already exists.");
        }
    }

    private Task<bool> IsUniversityReferencedAsync(string name) =>
        _context.StudentProfiles.AsNoTracking().AnyAsync(s => s.University == name);

    private Task<bool> IsDegreeReferencedAsync(string name) =>
        _context.StudentProfiles.AsNoTracking().AnyAsync(s => s.Degree == name);
}
