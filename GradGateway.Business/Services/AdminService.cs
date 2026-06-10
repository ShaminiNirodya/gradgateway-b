using GradGateway.Business.DTOs;
using GradGateway.Business.Helpers;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class AdminService : IAdminService
{
    private readonly GradGatewayDbContext _context;
    private readonly IPlatformStatsService _platformStats;

    public AdminService(GradGatewayDbContext context, IPlatformStatsService platformStats)
    {
        _context = context;
        _platformStats = platformStats;
    }

    public async Task EnsureAdminAsync(string firebaseUid)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

        if (user == null || user.Role != UserRole.Admin || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Admin access required.");
        }
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var stats = await _platformStats.GetPlatformStatsAsync();
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var todaySl = DeadlineClock.TodayDateInSriLanka();

        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var suspendedUsers = await _context.Users.CountAsync(u => !u.IsActive);
        var studentAccounts = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        var companyAccounts = await _context.Users.CountAsync(u => u.Role == UserRole.Company);
        var adminAccounts = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
        var pendingCompanies = await _context.CompanyProfiles
            .CountAsync(c => c.VerificationStatus == CompanyVerificationStatus.Pending);
        var approvedCompanies = await _context.CompanyProfiles
            .CountAsync(c => c.VerificationStatus == CompanyVerificationStatus.Approved);
        var rejectedCompanies = await _context.CompanyProfiles
            .CountAsync(c => c.VerificationStatus == CompanyVerificationStatus.Rejected);
        var totalApplications = await _context.Applications.CountAsync();
        var hiredApplications = await _context.Applications
            .CountAsync(a => a.Status == ApplicationStatus.Hired);
        var signups7d = await _context.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo);
        var activeJobs = await _context.Opportunities.CountAsync(o =>
            o.IsActive && o.DeadlineAt.Date >= todaySl);
        var expiredJobs = await _context.Opportunities.CountAsync(o =>
            !o.IsActive || o.DeadlineAt.Date < todaySl);
        var openInquiries = await _context.SupportInquiries.CountAsync(i => i.Status == "Open");
        var totalInquiries = await _context.SupportInquiries.CountAsync();

        return new AdminDashboardDto(
            stats.TotalStudents,
            stats.TotalCompanies,
            stats.TotalProjects,
            stats.HiringRate,
            totalUsers,
            activeUsers,
            suspendedUsers,
            studentAccounts,
            companyAccounts,
            adminAccounts,
            pendingCompanies,
            approvedCompanies,
            rejectedCompanies,
            totalApplications,
            hiredApplications,
            signups7d,
            activeJobs,
            expiredJobs,
            openInquiries,
            totalInquiries);
    }

    public async Task<IReadOnlyList<AdminUserListItemDto>> GetUsersAsync(
        string? role,
        string? search,
        bool? activeOnly)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) &&
            Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole);
        }

        if (activeOnly == true)
        {
            query = query.Where(u => u.IsActive);
        }
        else if (activeOnly == false)
        {
            query = query.Where(u => !u.IsActive);
        }

        List<User> users;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            var matchingStudentUserIds = await _context.StudentProfiles.AsNoTracking()
                .Where(s => s.FullName.ToLower().Contains(term))
                .Select(s => s.UserId)
                .ToListAsync();
            var matchingCompanyUserIds = await _context.CompanyProfiles.AsNoTracking()
                .Where(c => c.CompanyName.ToLower().Contains(term))
                .Select(c => c.UserId)
                .ToListAsync();

            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                matchingStudentUserIds.Contains(u.Id) ||
                matchingCompanyUserIds.Contains(u.Id));
        }

        users = await query.OrderByDescending(u => u.CreatedAt).Take(500).ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();

        var students = await _context.StudentProfiles.AsNoTracking()
            .Where(s => userIds.Contains(s.UserId))
            .Select(s => new { s.UserId, s.Id, s.FullName, s.University, s.Degree })
            .ToListAsync();

        var companies = await _context.CompanyProfiles.AsNoTracking()
            .Where(c => userIds.Contains(c.UserId))
            .Select(c => new { c.UserId, c.Id, c.CompanyName })
            .ToListAsync();

        return users.Select(u =>
        {
            var student = students.FirstOrDefault(s => s.UserId == u.Id);
            var company = companies.FirstOrDefault(c => c.UserId == u.Id);
            var displayName = student?.FullName ?? company?.CompanyName;

            return new AdminUserListItemDto(
                u.Id,
                u.Email,
                u.Role.ToString(),
                u.IsActive,
                u.CreatedAt,
                displayName,
                student?.Id,
                company?.Id,
                student?.University,
                student?.Degree);
        }).ToList();
    }

    public async Task SetUserActiveAsync(Guid userId, bool isActive)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        if (user.Role == UserRole.Admin && !isActive)
        {
            var activeAdmins = await _context.Users.CountAsync(u =>
                u.Role == UserRole.Admin && u.IsActive && u.Id != userId);
            if (activeAdmins == 0)
            {
                throw new InvalidOperationException("Cannot suspend the last active admin account.");
            }
        }

        user.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        if (user.Role == UserRole.Admin)
        {
            var activeAdmins = await _context.Users.CountAsync(u =>
                u.Role == UserRole.Admin && u.IsActive && u.Id != userId);
            if (activeAdmins == 0)
            {
                throw new InvalidOperationException("Cannot remove the last active admin account.");
            }
        }

        user.IsActive = false;

        if (user.Role == UserRole.Company)
        {
            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (company != null)
            {
                var jobs = await _context.Opportunities
                    .Where(o => o.CompanyProfileId == company.Id && o.IsActive)
                    .ToListAsync();
                foreach (var job in jobs)
                {
                    job.IsActive = false;
                    job.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AdminCompanyListItemDto>> GetCompaniesAsync(string? status, string? search)
    {
        var query = _context.CompanyProfiles
            .AsNoTracking()
            .Include(c => c.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<CompanyVerificationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(c => c.VerificationStatus == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.CompanyName.ToLower().Contains(term) ||
                c.CompanyEmail.ToLower().Contains(term) ||
                c.User.Email.ToLower().Contains(term));
        }

        var companies = await query.OrderByDescending(c => c.CreatedAt).Take(500).ToListAsync();
        var companyIds = companies.Select(c => c.Id).ToList();
        var todaySl = DeadlineClock.TodayDateInSriLanka();

        var jobCounts = await _context.Opportunities
            .Where(o => companyIds.Contains(o.CompanyProfileId) && o.IsActive && o.DeadlineAt.Date >= todaySl)
            .GroupBy(o => o.CompanyProfileId)
            .Select(g => new { CompanyProfileId = g.Key, Count = g.Count() })
            .ToListAsync();

        return companies.Select(c =>
        {
            var jobs = jobCounts.FirstOrDefault(j => j.CompanyProfileId == c.Id)?.Count ?? 0;
            return new AdminCompanyListItemDto(
                c.Id,
                c.UserId,
                c.CompanyName,
                c.CompanyEmail,
                c.Industry,
                c.VerificationStatus.ToString(),
                c.VerificationRejectionReason,
                c.VerifiedAt,
                c.CreatedAt,
                jobs,
                c.User.Email,
                c.User.IsActive);
        }).ToList();
    }

    public async Task<IReadOnlyList<SupportInquiryListItemDto>> GetSupportInquiriesAsync(
        string? status,
        string? inquiryType,
        string? submitterRole)
    {
        var query = _context.SupportInquiries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(inquiryType))
        {
            query = query.Where(i => i.InquiryType == inquiryType);
        }

        if (!string.IsNullOrWhiteSpace(submitterRole))
        {
            query = query.Where(i => i.SubmitterRole == submitterRole);
        }

        var rows = await query.OrderByDescending(i => i.CreatedAt).Take(500).ToListAsync();
        return rows.Select(SupportInquiryService.ToDto).ToList();
    }

    public async Task MarkSupportInquiryReviewedAsync(Guid inquiryId)
    {
        var row = await _context.SupportInquiries.FirstOrDefaultAsync(i => i.Id == inquiryId)
            ?? throw new InvalidOperationException("Inquiry not found.");

        row.Status = "Reviewed";
        row.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSupportInquiryAsync(Guid inquiryId)
    {
        var row = await _context.SupportInquiries.FirstOrDefaultAsync(i => i.Id == inquiryId)
            ?? throw new InvalidOperationException("Inquiry not found.");

        _context.SupportInquiries.Remove(row);
        await _context.SaveChangesAsync();
    }

    public async Task<AdminPlatformSettingsDto> GetPlatformSettingsAsync()
    {
        var settings = await PlatformSettingsAccessor.GetOrCreateAsync(_context);
        return ToSettingsDto(settings);
    }

    public async Task<AdminPlatformSettingsDto> UpdatePlatformSettingsAsync(AdminUpdatePlatformSettingsDto dto)
    {
        var settings = await PlatformSettingsAccessor.GetOrCreateAsync(_context);
        settings.AllowRegistration = dto.AllowRegistration;
        settings.RequireCompanyVerification = dto.RequireCompanyVerification;
        settings.MaintenanceMode = dto.MaintenanceMode;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToSettingsDto(settings);
    }

    private static AdminPlatformSettingsDto ToSettingsDto(PlatformSettings settings) =>
        new(
            settings.AllowRegistration,
            settings.RequireCompanyVerification,
            settings.MaintenanceMode,
            settings.UpdatedAt);
}
