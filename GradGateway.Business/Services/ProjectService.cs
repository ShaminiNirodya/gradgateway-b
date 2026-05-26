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
            .Include(p => p.Images)
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
            .Include(p => p.Images)
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
            project.IsPublic = dto.IsPublic;
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
