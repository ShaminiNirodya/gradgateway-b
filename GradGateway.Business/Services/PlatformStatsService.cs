using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class PlatformStatsService : IPlatformStatsService
{
    private readonly GradGatewayDbContext _context;

    public PlatformStatsService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformStatsDto> GetPlatformStatsAsync()
    {
        // Get total number of students
        var totalStudents = await _context.StudentProfiles.CountAsync();

        // Get total number of companies
        var totalCompanies = await _context.CompanyProfiles.CountAsync();

        // Get total number of projects
        var totalProjects = await _context.Projects.CountAsync();

        // Calculate hiring rate (hired / total applications)
        var totalApplications = await _context.Applications.CountAsync();
        var hiredApplications = await _context.Applications
            .Where(a => a.Status == ApplicationStatus.Hired)
            .CountAsync();

        var hiringRate = totalApplications > 0 
            ? Math.Round((decimal)hiredApplications / totalApplications * 100, 1)
            : 0;

        return new PlatformStatsDto(
            totalStudents,
            totalCompanies,
            totalProjects,
            hiringRate
        );
    }
}
