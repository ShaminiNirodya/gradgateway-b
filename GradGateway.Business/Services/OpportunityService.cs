using GradGateway.Business.DTOs;
using GradGateway.Business.Helpers;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class OpportunityService : IOpportunityService
{
    private readonly GradGatewayDbContext _context;
    private readonly IDeadlineNotificationProcessor _deadlineProcessor;
    private readonly IInterviewPlanService _interviewPlanService;

    public OpportunityService(
        GradGatewayDbContext context,
        IDeadlineNotificationProcessor deadlineProcessor,
        IInterviewPlanService interviewPlanService)
    {
        _context = context;
        _deadlineProcessor = deadlineProcessor;
        _interviewPlanService = interviewPlanService;
    }

    public async Task<OpportunityResponseDto> CreateOpportunityAsync(string firebaseUid, CreateOpportunityRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can create opportunities.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        if (!Enum.TryParse<OpportunityType>(dto.OpportunityType, true, out var type))
            throw new ArgumentException("Invalid opportunity type.");

        if (!Enum.TryParse<WorkMode>(dto.WorkMode, true, out var mode))
            throw new ArgumentException("Invalid work mode.");

        var deadlineUtc = DateTime.SpecifyKind(dto.DeadlineAt.Date, DateTimeKind.Utc);
        if (deadlineUtc.Date < DeadlineClock.TodayDateInSriLanka())
            throw new ArgumentException("Deadline must be today or a future date.");

        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = company.Id,
            Title = dto.Title,
            Description = dto.Description,
            OpportunityType = type,
            WorkMode = mode,
            Location = dto.Location,
            RequiredSkills = dto.RequiredSkills,
            MonthlyStipendLkr = dto.MonthlyStipendLkr,
            DeadlineAt = deadlineUtc,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        return ToResponse(opportunity, company.CompanyName, company.LogoDataUrl);
    }

    public async Task<List<OpportunityResponseDto>> GetActiveOpportunitiesAsync()
    {
        var feed = await GetStudentOpeningsFeedAsync();
        return feed.Active;
    }

    public async Task<StudentOpeningsFeedDto> GetStudentOpeningsFeedAsync()
    {
        await EnsureDemoOpportunitiesAsync();
        await AutoExpireOpportunitiesAsync();

        var todaySl = DeadlineClock.TodayDateInSriLanka();

        var rows = await _context.Opportunities
            .Include(o => o.CompanyProfile)
                .ThenInclude(c => c.User)
            .Where(o =>
                o.IsActive &&
                o.DeadlineAt.Date >= todaySl &&
                o.CompanyProfile.User.IsActive)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var expiredCount = await _context.Opportunities.CountAsync(o =>
            !o.IsActive || o.DeadlineAt.Date < todaySl);

        return new StudentOpeningsFeedDto(
            rows.Select(o => ToResponse(o, o.CompanyProfile.CompanyName, o.CompanyProfile.LogoDataUrl)).ToList(),
            expiredCount);
    }

    public async Task<int> GetExpiredOpportunitiesCountAsync()
    {
        var feed = await GetStudentOpeningsFeedAsync();
        return feed.ExpiredCount;
    }

    public async Task<List<OpportunityResponseDto>> GetCompanyOpportunitiesAsync(string firebaseUid)
    {
        await AutoExpireOpportunitiesAsync();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can access company opportunities.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        var rows = await _context.Opportunities
            .Where(o => o.CompanyProfileId == company.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return rows.Select(o => ToResponse(o, company.CompanyName, company.LogoDataUrl)).ToList();
    }

    public async Task<OpportunityResponseDto?> GetOpportunityByIdAsync(Guid id)
    {
        var row = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == id);

        return row == null ? null : ToResponse(row, row.CompanyProfile.CompanyName, row.CompanyProfile.LogoDataUrl);
    }

    private static OpportunityResponseDto ToResponse(Opportunity o, string companyName, string? companyLogoUrl = null)
    {
        return new OpportunityResponseDto(
            o.Id,
            o.CompanyProfileId,
            companyName,
            companyLogoUrl,
            o.Title,
            o.Description,
            o.OpportunityType.ToString(),
            o.WorkMode.ToString(),
            o.Location,
            o.RequiredSkills,
            o.MonthlyStipendLkr,
            o.DeadlineAt,
            o.IsActive,
            o.CreatedAt
        );
    }

    private async Task EnsureDemoOpportunitiesAsync()
    {
        if (await _context.Opportunities.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var companyProfiles = await _context.CompanyProfiles
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        if (!companyProfiles.Any())
        {
            return;
        }

        var seeds = new[]
        {
            new { Title = "Software Engineering Intern", Desc = "Build internal tools with .NET and React.", Skills = "C#, ASP.NET Core, React, SQL Server", Type = OpportunityType.Internship, Mode = WorkMode.Hybrid, Location = "Colombo", Stipend = 80000m, Days = 45 },
            new { Title = "Data Analytics Intern", Desc = "Work with telecom data and BI pipelines.", Skills = "Python, SQL, Power BI, Statistics", Type = OpportunityType.Internship, Mode = WorkMode.Onsite, Location = "Battaramulla", Stipend = 90000m, Days = 55 },
            new { Title = "Associate Software Engineer", Desc = "Graduate role for cloud-native service development.", Skills = "Java, Microservices, Docker, Kubernetes", Type = OpportunityType.GraduateRole, Mode = WorkMode.Hybrid, Location = "Colombo", Stipend = 185000m, Days = 65 },
            new { Title = "QA Automation Intern", Desc = "Automate regression suites with Playwright and CI.", Skills = "Playwright, Selenium, C#, CI/CD", Type = OpportunityType.Internship, Mode = WorkMode.Remote, Location = "Sri Lanka", Stipend = 70000m, Days = 50 }
        };

        for (var i = 0; i < seeds.Length; i++)
        {
            var company = companyProfiles[i % companyProfiles.Count];
            _context.Opportunities.Add(new Opportunity
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = company.Id,
                Title = seeds[i].Title,
                Description = seeds[i].Desc,
                OpportunityType = seeds[i].Type,
                WorkMode = seeds[i].Mode,
                Location = seeds[i].Location,
                RequiredSkills = seeds[i].Skills,
                MonthlyStipendLkr = seeds[i].Stipend,
                DeadlineAt = now.AddDays(seeds[i].Days),
                IsActive = true,
                CreatedAt = now.AddDays(-(i + 1) * 2),
                UpdatedAt = now.AddDays(-(i + 1))
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task AutoExpireOpportunitiesAsync()
    {
        var now = DateTime.UtcNow;

        var todaySl = DeadlineClock.TodayDateInSriLanka();
        var expiredActiveRows = await _context.Opportunities
            .Where(o => o.IsActive && o.DeadlineAt.Date < todaySl)
            .ToListAsync();

        if (!expiredActiveRows.Any())
        {
            return;
        }

        foreach (var item in expiredActiveRows)
        {
            item.IsActive = false;
            item.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        await _deadlineProcessor.ProcessExpiredOpportunityDeadlinesAsync();
    }

    public Task<ScheduleInterviewsResultDto> ScheduleInterviewsAsync(
        string firebaseUid,
        Guid opportunityId,
        ScheduleInterviewsRequestDto dto)
        => _interviewPlanService.SavePlanAsync(firebaseUid, opportunityId, dto);
}
