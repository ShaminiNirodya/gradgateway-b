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

        var userAgg = await _context.Users
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(u => u.IsActive),
                Suspended = g.Count(u => !u.IsActive),
                Students = g.Count(u => u.Role == UserRole.Student),
                Companies = g.Count(u => u.Role == UserRole.Company),
                Admins = g.Count(u => u.Role == UserRole.Admin),
                Signups7d = g.Count(u => u.CreatedAt >= sevenDaysAgo),
            })
            .FirstOrDefaultAsync();

        var applicationAgg = await _context.Applications
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Hired = g.Count(a => a.Status == ApplicationStatus.Hired),
            })
            .FirstOrDefaultAsync();

        var activeJobs = await _context.Opportunities.AsNoTracking().CountAsync(o =>
            o.IsActive && o.DeadlineAt.Date >= todaySl);
        var expiredJobs = await _context.Opportunities.AsNoTracking().CountAsync(o =>
            !o.IsActive || o.DeadlineAt.Date < todaySl);
        var openInquiries = await _context.SupportInquiries.AsNoTracking().CountAsync(i => i.Status == "Open");
        var totalInquiries = await _context.SupportInquiries.AsNoTracking().CountAsync();
        var pendingTestimonials = await _context.Testimonials.AsNoTracking().CountAsync(t => t.Status == "Pending");

        return new AdminDashboardDto(
            stats.TotalStudents,
            stats.TotalCompanies,
            stats.TotalProjects,
            stats.HiringRate,
            userAgg?.Total ?? 0,
            userAgg?.Active ?? 0,
            userAgg?.Suspended ?? 0,
            userAgg?.Students ?? 0,
            userAgg?.Companies ?? 0,
            userAgg?.Admins ?? 0,
            applicationAgg?.Total ?? 0,
            applicationAgg?.Hired ?? 0,
            userAgg?.Signups7d ?? 0,
            activeJobs,
            expiredJobs,
            openInquiries,
            totalInquiries,
            pendingTestimonials);
    }

    public async Task<AdminAnalyticsDto> GetAnalyticsAsync()
    {
        var dashboard = await GetDashboardAsync();
        var stats = await _platformStats.GetPlatformStatsAsync();
        var today = DateTime.UtcNow.Date;
        var sevenDaysAgo = today.AddDays(-7);

        var userRows = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.CreatedAt })
            .ToListAsync();

        var signupsByWeek = Enumerable.Range(0, 8)
            .Select(i => today.AddDays(-7 * (7 - i)))
            .Select(weekStart =>
            {
                var weekEnd = weekStart.AddDays(7);
                return new AnalyticsDataPointDto(
                    weekStart.ToString("MMM d"),
                    userRows.Count(u => u.CreatedAt.Date >= weekStart && u.CreatedAt.Date < weekEnd),
                    weekStart);
            })
            .ToList();

        var applicationRows = await _context.Applications
            .AsNoTracking()
            .Select(a => new { a.StudentProfileId, a.Status, a.AppliedAt })
            .ToListAsync();

        var applicationsByWeek = Enumerable.Range(0, 8)
            .Select(i => today.AddDays(-7 * (7 - i)))
            .Select(weekStart =>
            {
                var weekEnd = weekStart.AddDays(7);
                return new AnalyticsDataPointDto(
                    weekStart.ToString("MMM d"),
                    applicationRows.Count(a => a.AppliedAt.Date >= weekStart && a.AppliedAt.Date < weekEnd),
                    weekStart);
            })
            .ToList();

        var applicationsByStatus = Enum.GetValues<ApplicationStatus>()
            .Select(status => new AnalyticsCountDto(
                status.ToString(),
                applicationRows.Count(a => a.Status == status)))
            .Where(row => row.Value > 0)
            .OrderByDescending(row => row.Value)
            .ToList();

        var topIndustries = await _context.CompanyProfiles
            .AsNoTracking()
            .GroupBy(c => c.Industry)
            .Select(g => new AnalyticsCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Value)
            .Take(6)
            .ToListAsync();

        var studentProfiles = await _context.StudentProfiles
            .AsNoTracking()
            .Select(s => new { s.Id, s.University, s.Degree })
            .ToListAsync();

        var studentBreakdownByUniversity = studentProfiles
            .Where(s => !string.IsNullOrWhiteSpace(s.University))
            .GroupBy(s => s.University.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var studentIds = g.Select(s => s.Id).ToHashSet();
                var universityApplications = applicationRows
                    .Where(a => studentIds.Contains(a.StudentProfileId))
                    .ToList();
                var universityTotalApplications = universityApplications.Count;
                var universityHiredApplications = universityApplications.Count(a => a.Status == ApplicationStatus.Hired);

                var degrees = g
                    .Where(s => !string.IsNullOrWhiteSpace(s.Degree))
                    .GroupBy(s => s.Degree.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(dg =>
                    {
                        var degreeStudentIds = dg.Select(s => s.Id).ToHashSet();
                        var degreeApplications = applicationRows
                            .Where(a => degreeStudentIds.Contains(a.StudentProfileId))
                            .ToList();
                        var degreeTotalApplications = degreeApplications.Count;
                        var degreeHiredApplications = degreeApplications.Count(a => a.Status == ApplicationStatus.Hired);

                        return new DegreeStudentAnalyticsDto(
                            dg.Key,
                            dg.Count(),
                            degreeTotalApplications == 0
                                ? 0m
                                : Math.Round((decimal)degreeHiredApplications / degreeTotalApplications * 100m, 2));
                    })
                    .OrderByDescending(d => d.StudentCount)
                    .ThenBy(d => d.Degree, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new UniversityStudentAnalyticsDto(
                    g.Key,
                    g.Count(),
                    universityTotalApplications == 0
                        ? 0m
                        : Math.Round((decimal)universityHiredApplications / universityTotalApplications * 100m, 2),
                    degrees);
            })
            .OrderByDescending(u => u.StudentCount)
            .ThenBy(u => u.University, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pendingTestimonials = await _context.Testimonials.CountAsync(t => t.Status == "Pending");
        var publishedTestimonials = await _context.Testimonials.CountAsync(t => t.Status == "Published");

        return new AdminAnalyticsDto(
            dashboard.TotalStudents,
            dashboard.TotalCompanies,
            dashboard.TotalApplications,
            dashboard.HiredApplications,
            dashboard.ActiveJobPosts,
            dashboard.SignupsLast7Days,
            dashboard.OpenSupportInquiries,
            pendingTestimonials,
            publishedTestimonials,
            stats.HiringRate,
            signupsByWeek,
            applicationsByWeek,
            applicationsByStatus,
            topIndustries,
            studentBreakdownByUniversity);
    }

    public async Task<PagedResultDto<AdminUserListItemDto>> GetUsersAsync(
        string? role,
        string? search,
        bool? activeOnly,
        int page = 1,
        int pageSize = Pagination.DefaultPageSize)
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

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync();
        users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();

        var students = await _context.StudentProfiles.AsNoTracking()
            .Where(s => userIds.Contains(s.UserId))
            .Select(s => new { s.UserId, s.Id, s.FullName, s.University, s.Degree })
            .ToListAsync();

        var companies = await _context.CompanyProfiles.AsNoTracking()
            .Where(c => userIds.Contains(c.UserId))
            .Select(c => new { c.UserId, c.Id, c.CompanyName })
            .ToListAsync();

        var items = users.Select(u =>
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

        return new PagedResultDto<AdminUserListItemDto>(items, total, normalizedPage, normalizedPageSize);
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

    public async Task<PagedResultDto<AdminCompanyListItemDto>> GetCompaniesAsync(
        string? status,
        string? search,
        int page = 1,
        int pageSize = Pagination.DefaultPageSize)
    {
        var query = _context.CompanyProfiles
            .AsNoTracking()
            .Include(c => c.User)
            .AsQueryable();

        if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.User.IsActive);
        }
        else if (string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => !c.User.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.CompanyName.ToLower().Contains(term) ||
                c.CompanyEmail.ToLower().Contains(term) ||
                c.User.Email.ToLower().Contains(term));
        }

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync();
        var companies = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();
        var companyIds = companies.Select(c => c.Id).ToList();
        var todaySl = DeadlineClock.TodayDateInSriLanka();

        var jobCounts = await _context.Opportunities
            .AsNoTracking()
            .Where(o => companyIds.Contains(o.CompanyProfileId) && o.IsActive && o.DeadlineAt.Date >= todaySl)
            .GroupBy(o => o.CompanyProfileId)
            .Select(g => new { CompanyProfileId = g.Key, Count = g.Count() })
            .ToListAsync();

        var items = companies.Select(c =>
        {
            var jobs = jobCounts.FirstOrDefault(j => j.CompanyProfileId == c.Id)?.Count ?? 0;
            return new AdminCompanyListItemDto(
                c.Id,
                c.UserId,
                c.CompanyName,
                c.CompanyEmail,
                c.Industry,
                c.CreatedAt,
                jobs,
                c.User.Email,
                c.User.IsActive);
        }).ToList();

        return new PagedResultDto<AdminCompanyListItemDto>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<PagedResultDto<SupportInquiryListItemDto>> GetSupportInquiriesAsync(
        string? status,
        string? inquiryType,
        string? submitterRole,
        int page = 1,
        int pageSize = Pagination.DefaultPageSize)
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

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var items = rows.Select(SupportInquiryService.ToDto).ToList();
        return new PagedResultDto<SupportInquiryListItemDto>(items, total, normalizedPage, normalizedPageSize);
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

    public async Task<PagedResultDto<AdminEmailLogItemDto>> GetEmailLogsAsync(
        string? search,
        string? status,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.Set<EmailLog>()
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.ToEmail.Contains(term) ||
                x.User.Email.Contains(term) ||
                x.TemplateType.Contains(term) ||
                x.Purpose.Contains(term));
        }

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var items = rows.Select(x => new AdminEmailLogItemDto(
            x.Id,
            x.User.Email,
            x.ToEmail,
            x.TemplateType,
            x.Purpose,
            x.Provider,
            x.Status,
            x.Error,
            x.CreatedAt,
            x.SentAt)).ToList();

        return new PagedResultDto<AdminEmailLogItemDto>(items, total, normalizedPage, normalizedPageSize);
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
        settings.MaintenanceMode = dto.MaintenanceMode;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToSettingsDto(settings);
    }

    private static AdminPlatformSettingsDto ToSettingsDto(PlatformSettings settings) =>
        new(
            settings.AllowRegistration,
            settings.MaintenanceMode,
            settings.UpdatedAt);
}
