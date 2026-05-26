using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class ProjectService : IProjectService
{
    private readonly GradGatewayDbContext _context;

    public ProjectService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectResponseDto>> GetMyProjectsAsync(string firebaseUid)
    {
        var student = await GetStudentProfileAsync(firebaseUid);

        var demoRepoUrls = new[]
        {
            "https://github.com/gradgateway/demo-portfolio",
            "https://github.com/gradgateway/lanka-transit-insights"
        };

        var rows = await _context.Projects
            .Where(p => p.StudentProfileId == student.Id)
            .Where(p =>
                !demoRepoUrls.Contains(p.RepositoryUrl ?? string.Empty) &&
                p.Title != "Lanka Transit Insights" &&
                p.Title != $"{student.FullName} - Internship Portfolio")
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return rows.Select(p => ToResponse(p, student.FullName)).ToList();
    }

    public async Task<ProjectResponseDto?> GetMyProjectByIdAsync(string firebaseUid, Guid projectId)
    {
        var student = await GetStudentProfileAsync(firebaseUid);

        var demoRepoUrls = new[]
        {
            "https://github.com/gradgateway/demo-portfolio",
            "https://github.com/gradgateway/lanka-transit-insights"
        };

        var row = await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.StudentProfileId == student.Id &&
                !demoRepoUrls.Contains(p.RepositoryUrl ?? string.Empty) &&
                p.Title != "Lanka Transit Insights" &&
                p.Title != $"{student.FullName} - Internship Portfolio");

        return row == null ? null : ToResponse(row, student.FullName);
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(string firebaseUid, CreateProjectDto dto)
    {
        var student = await GetStudentProfileAsync(firebaseUid);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            StudentProfileId = student.Id,
            Title = dto.Title,
            Description = dto.Description,
            TechStack = dto.TechStack,
            RepositoryUrl = dto.RepositoryUrl,
            DemoUrl = dto.DemoUrl,
            IsPublic = dto.IsPublic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return ToResponse(project, student.FullName);
    }

    private async Task<StudentProfile> GetStudentProfileAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            throw new InvalidOperationException("Only student users can access projects.");

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            throw new InvalidOperationException("Student profile not found.");

        return student;
    }

    private static ProjectResponseDto ToResponse(Project p, string studentName)
    {
        return new ProjectResponseDto(
            p.Id,
            p.StudentProfileId,
            studentName,
            p.Title,
            p.Description,
            p.TechStack,
            p.RepositoryUrl,
            p.DemoUrl,
            p.IsPublic,
            p.CreatedAt,
            p.UpdatedAt
        );
    }
}
