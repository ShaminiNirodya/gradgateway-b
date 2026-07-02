using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GradGateway.Business.Services;

public class PlatformStatsService : IPlatformStatsService
{
    private const string CacheKey = "platform_stats_v1";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly GradGatewayDbContext _context;
    private readonly IMemoryCache _cache;

    public PlatformStatsService(GradGatewayDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public Task<PlatformStatsDto> GetPlatformStatsAsync()
    {
        return _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await ComputePlatformStatsAsync();
        })!;
    }

    private async Task<PlatformStatsDto> ComputePlatformStatsAsync()
    {
        var totalStudents = await _context.StudentProfiles.AsNoTracking().CountAsync();
        var totalCompanies = await _context.CompanyProfiles.AsNoTracking().CountAsync();
        var totalProjects = await _context.Projects.AsNoTracking().CountAsync();

        var totalApplications = await _context.Applications.AsNoTracking().CountAsync();
        var hiredApplications = await _context.Applications
            .AsNoTracking()
            .CountAsync(a => a.Status == ApplicationStatus.Hired);

        var hiringRate = totalApplications > 0
            ? Math.Round((decimal)hiredApplications / totalApplications * 100, 1)
            : 0;

        return new PlatformStatsDto(totalStudents, totalCompanies, totalProjects, hiringRate);
    }
}
