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
        return await QueryProjectsForStudentAsync(student, excludeSeededDemo: true);
    }
//Throws when a project is not found or a concurrency conflict happens.
    public async Task<List<ProjectResponseDto>> GetProjectsByStudentProfileIdAsync(string firebaseUid, Guid studentProfileId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
            ?? throw new InvalidOperationException("User not found.");

        if (user.Role is not (UserRole.Company or UserRole.Admin))
            throw new InvalidOperationException("Only company users can view student portfolio projects.");

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == studentProfileId)
            ?? throw new InvalidOperationException("Student profile not found.");

        // Companies see the full portfolio (including seeded samples); students' own list hides demo seeds.
        return await QueryProjectsForStudentAsync(student, excludeSeededDemo: false);
    }

    private async Task<List<ProjectResponseDto>> QueryProjectsForStudentAsync(
        StudentProfile student,
        bool excludeSeededDemo)
    {
        var demoRepoUrls = new[]
        {
            "https://github.com/gradgateway/demo-portfolio",
            "https://github.com/gradgateway/lanka-transit-insights"
        };

        var query = _context.Projects
            .Include(p => p.Images)
            .Where(p => p.StudentProfileId == student.Id);

        if (excludeSeededDemo)
        {
            query = query.Where(p =>
                !demoRepoUrls.Contains(p.RepositoryUrl ?? string.Empty) &&
                p.Title != "Lanka Transit Insights" &&
                p.Title != $"{student.FullName} - Internship Portfolio");
        }

        var rows = await query
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
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.StudentProfileId == student.Id &&
                !demoRepoUrls.Contains(p.RepositoryUrl ?? string.Empty) &&
                p.Title != "Lanka Transit Insights" &&
                p.Title != $"{student.FullName} - Internship Portfolio");

        return row == null ? null : ToResponse(row, student.FullName);
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid projectId)
    {
        var row = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.StudentProfile)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        return row == null ? null : ToResponse(row, row.StudentProfile?.FullName ?? "GradGateway Student");
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
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add images if provided
        if (dto.Images != null && dto.Images.Count > 0)
        {
            for (int i = 0; i < dto.Images.Count; i++)
            {
                var imageDto = dto.Images[i];
                try
                {
                    var projectImage = new ProjectImage
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        ImageUrl = imageDto.Url, // Firebase Storage URL
                        ImageMimeType = imageDto.MimeType,
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow
                    };

                    project.Images.Add(projectImage);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other images
                    Console.WriteLine($"Failed to process image {i}: {ex.Message}");
                }
            }
        }

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return ToResponse(project, student.FullName);
    }

    public async Task<ProjectResponseDto> UpdateProjectAsync(string firebaseUid, Guid projectId, UpdateProjectDto dto)
    {
        var student = await GetStudentProfileAsync(firebaseUid);

        var project = await _context.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.StudentProfileId == student.Id);

        if (project == null)
            throw new InvalidOperationException("Project not found.");

        try
        {
            // Update basic fields
            project.Title = dto.Title;
            project.Description = dto.Description;
            project.TechStack = dto.TechStack;
            project.RepositoryUrl = dto.RepositoryUrl;
            project.DemoUrl = dto.DemoUrl;
            project.IsPublic = true;
            project.UpdatedAt = DateTime.UtcNow;

            // Handle image deletions - delete from DB first
            if (dto.DeleteImageIds != null && dto.DeleteImageIds.Count > 0)
            {
                var imagesToDelete = await _context.ProjectImages
                    .Where(img => dto.DeleteImageIds.Contains(img.Id) && img.ProjectId == projectId)
                    .ToListAsync();
                
                foreach (var image in imagesToDelete)
                {
                    _context.ProjectImages.Remove(image);
                }
            }

            // Save changes for deletions and basic field updates
            await _context.SaveChangesAsync();

            // Reload to get fresh state
            project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new InvalidOperationException("Project was deleted or no longer accessible.");

            // Handle new image additions
            if (dto.NewImages != null && dto.NewImages.Count > 0)
            {
                var maxDisplayOrder = project.Images.Any() ? project.Images.Max(img => img.DisplayOrder) : -1;

                for (int i = 0; i < dto.NewImages.Count; i++)
                {
                    var imageDto = dto.NewImages[i];
                    var projectImage = new ProjectImage
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        ImageUrl = imageDto.Url,
                        ImageMimeType = imageDto.MimeType,
                        DisplayOrder = maxDisplayOrder + i + 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ProjectImages.Add(projectImage);
                }

                // Save new images
                await _context.SaveChangesAsync();
            }

            // Reload final state
            project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new InvalidOperationException("Project was deleted or no longer accessible.");

            return ToResponse(project, student.FullName);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"[ERROR] Concurrency conflict during project update: {ex.Message}");
            throw new InvalidOperationException("Project was modified by another user. Please refresh and try again.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to update project: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<bool> DeleteProjectAsync(string firebaseUid, Guid projectId)
    {
        var student = await GetStudentProfileAsync(firebaseUid);

        var project = await _context.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.StudentProfileId == student.Id);

        if (project == null)
            return false;

        // Remove all associated images first
        if (project.Images.Any())
        {
            _context.ProjectImages.RemoveRange(project.Images);
        }

        // Remove the project
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
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
        var images = p.Images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => new ProjectImageDto(
                img.Id,
                img.ImageUrl, // Firebase Storage URL - no conversion needed
                img.DisplayOrder
            ))
            .ToList();

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
            p.UpdatedAt,
            images
        );
    }
}
