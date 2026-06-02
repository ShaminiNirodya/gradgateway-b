using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using System.Text.Json;

namespace GradGateway.Business.Services;

public class StudentService : IStudentService
{
    private readonly GradGatewayDbContext _context;

    public StudentService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<StudentProfileResponseDto> RegisterOrUpdateStudentAsync(StudentRegistrationDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == dto.FirebaseUid);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = dto.FirebaseUid,
                Email = dto.Email,
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw MapStudentRegistrationDbError(ex);
            }
        }
        else if (user.Role != UserRole.Student)
        {
            throw new InvalidOperationException("User role mismatch. This account is not a student.");
        }

        if (!int.TryParse(dto.GradYear, out var gradYear))
        {
            throw new ArgumentException("Invalid graduation year.");
        }

        if (!decimal.TryParse(dto.Gpa, out var gpa))
        {
            throw new ArgumentException("Invalid GPA.");
        }

        var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
        {
            profile = new StudentProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = dto.FullName,
                Phone = dto.Phone,
                PhotoDataUrl = dto.PhotoDataUrl,
                University = dto.University,
                StudentId = ResolveStudentId(dto.StudentId, user.Id),
                Degree = dto.Degree,
                FieldOfMajor = dto.FieldOfMajor ?? string.Empty,
                GradYear = gradYear,
                CurrentYear = dto.CurrentYear,
                Gpa = gpa,
                Availability = dto.Availability ?? "Available Now",
                CertificationsJson = SerializeStringList(dto.Certifications),
                AwardsJson = SerializeStringList(dto.Awards),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.StudentProfiles.Add(profile);
        }
        else
        {
            profile.FullName = dto.FullName;
            profile.Phone = dto.Phone;
            profile.PhotoDataUrl = dto.PhotoDataUrl;
            profile.University = dto.University;
            profile.StudentId = string.IsNullOrWhiteSpace(dto.StudentId)
                ? profile.StudentId
                : dto.StudentId.Trim();
            profile.Degree = dto.Degree;
            if (dto.FieldOfMajor != null)
            {
                profile.FieldOfMajor = dto.FieldOfMajor;
            }
            profile.GradYear = gradYear;
            profile.CurrentYear = dto.CurrentYear;
            profile.Gpa = gpa;
            profile.Availability = dto.Availability ?? profile.Availability;
            if (dto.Certifications != null)
            {
                profile.CertificationsJson = SerializeStringList(dto.Certifications);
            }
            if (dto.Awards != null)
            {
                profile.AwardsJson = SerializeStringList(dto.Awards);
            }
            profile.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw MapStudentRegistrationDbError(ex);
        }

        return new StudentProfileResponseDto(
            user.Email,
            user.FirebaseUid,
            profile.FullName,
            profile.Phone,
            profile.PhotoDataUrl,
            profile.University,
            profile.StudentId,
            profile.Degree,
            profile.FieldOfMajor,
            profile.GradYear,
            profile.CurrentYear,
            profile.Gpa,
            profile.Availability,
            DeserializeStringList(profile.CertificationsJson),
            DeserializeStringList(profile.AwardsJson)
        );
    }

    private static string ResolveStudentId(string? requestedStudentId, Guid userId)
    {
        if (!string.IsNullOrWhiteSpace(requestedStudentId))
        {
            return requestedStudentId.Trim();
        }

        return $"GG{DateTime.UtcNow:yyyy}{Math.Abs(userId.GetHashCode()) % 100000:D5}";
    }

    private static InvalidOperationException MapStudentRegistrationDbError(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        if (message.Contains("IX_StudentProfiles_StudentId", StringComparison.OrdinalIgnoreCase)
            || message.Contains("StudentProfiles", StringComparison.OrdinalIgnoreCase) && message.Contains("StudentId", StringComparison.OrdinalIgnoreCase))
        {
            return new InvalidOperationException("Student ID already exists. Please use a different Student ID.");
        }

        if (message.Contains("IX_Users_FirebaseUid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Users", StringComparison.OrdinalIgnoreCase) && message.Contains("FirebaseUid", StringComparison.OrdinalIgnoreCase))
        {
            return new InvalidOperationException("This account already exists. Please log in instead.");
        }

        return new InvalidOperationException("Could not save student profile. Please check your details and try again.");
    }

    public async Task<StudentProfileResponseDto?> GetStudentByFirebaseUidAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null) return null;

        var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null || user.Role != UserRole.Student)
        {
            return null;
        }

        await CleanupLegacyStudentDemoDataAsync(user, profile);

        return new StudentProfileResponseDto(
            user.Email,
            user.FirebaseUid,
            profile.FullName,
            profile.Phone,
            profile.PhotoDataUrl,
            profile.University,
            profile.StudentId,
            profile.Degree,
            profile.FieldOfMajor,
            profile.GradYear,
            profile.CurrentYear,
            profile.Gpa,
            profile.Availability,
            DeserializeStringList(profile.CertificationsJson),
            DeserializeStringList(profile.AwardsJson)
        );
    }

    private static string? SerializeStringList(IEnumerable<string>? values)
    {
        if (values == null)
        {
            return null;
        }

        var list = values
            .Select(value => value?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToList();

        return list.Count == 0 ? null : JsonSerializer.Serialize(list);
    }

    private async Task CleanupLegacyStudentDemoDataAsync(User studentUser, StudentProfile studentProfile)
    {
        var seededProjectTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"{studentProfile.FullName} - Internship Portfolio",
            "Lanka Transit Insights"
        };

        var seededRepoUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "https://github.com/gradgateway/demo-portfolio",
            "https://github.com/gradgateway/lanka-transit-insights"
        };

        var seededCoverLetters = new HashSet<string>(StringComparer.Ordinal)
        {
            "I’m excited to contribute and quickly adapt to your engineering workflows.",
            "My project experience and data background align with this internship."
        };

        var seededMessageContents = new HashSet<string>(StringComparer.Ordinal)
        {
            "Hi! I applied for this role and wanted to ask about the next interview step.",
            "Thanks for reaching out. We’ll finalize shortlisting by this weekend."
        };

        var projectsToRemove = await _context.Projects
            .Where(p => p.StudentProfileId == studentProfile.Id)
            .Where(p =>
                seededProjectTitles.Contains(p.Title) ||
                (!string.IsNullOrWhiteSpace(p.RepositoryUrl) && seededRepoUrls.Contains(p.RepositoryUrl)))
            .ToListAsync();

        var applicationsToRemove = await _context.Applications
            .Where(a => a.StudentProfileId == studentProfile.Id)
            .Where(a => !string.IsNullOrWhiteSpace(a.CoverLetter) && seededCoverLetters.Contains(a.CoverLetter!))
            .ToListAsync();

        var studentConversationIds = await _context.Conversations
            .Where(c => c.StudentProfileId == studentProfile.Id)
            .Select(c => c.Id)
            .ToListAsync();

        var messagesToRemove = await _context.Messages
            .Where(m => studentConversationIds.Contains(m.ConversationId))
            .Where(m => seededMessageContents.Contains(m.Content))
            .ToListAsync();

        if (projectsToRemove.Count == 0 && applicationsToRemove.Count == 0 && messagesToRemove.Count == 0)
        {
            return;
        }

        if (projectsToRemove.Count > 0)
        {
            _context.Projects.RemoveRange(projectsToRemove);
        }

        if (applicationsToRemove.Count > 0)
        {
            _context.Applications.RemoveRange(applicationsToRemove);
        }

        if (messagesToRemove.Count > 0)
        {
            _context.Messages.RemoveRange(messagesToRemove);
        }

        await _context.SaveChangesAsync();

        var emptyConversations = await _context.Conversations
            .Where(c => c.StudentProfileId == studentProfile.Id)
            .Where(c => !_context.Messages.Any(m => m.ConversationId == c.Id))
            .ToListAsync();

        if (emptyConversations.Count > 0)
        {
            _context.Conversations.RemoveRange(emptyConversations);
            await _context.SaveChangesAsync();
        }
    }

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)
                   ?.Where(value => !string.IsNullOrWhiteSpace(value))
                   .Select(value => value.Trim())
                   .ToList()
                     ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public async Task<List<StudentDirectoryItemDto>> GetStudentDirectoryAsync(string? query)
    {
        var term = (query ?? string.Empty).Trim().ToLower();

        var rows = await _context.StudentProfiles
            .Include(s => s.User)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();

        var profileIds = rows.Select(r => r.Id).ToList();
        
        // Get skills from both StudentSkills table and project tech stacks
        var studentSkills = await _context.StudentSkills
            .Include(ss => ss.Skill)
            .Where(ss => profileIds.Contains(ss.StudentProfileId))
            .ToListAsync();

        var projects = await _context.Projects
            .Where(p => profileIds.Contains(p.StudentProfileId))
            .Select(p => new { p.StudentProfileId, p.TechStack })
            .ToListAsync();

        var skillMap = new Dictionary<Guid, string>();
        
        foreach (var profileId in profileIds)
        {
            var skillSet = new HashSet<string>();
            
            // Add skills from StudentSkills table
            var directSkills = studentSkills
                .Where(ss => ss.StudentProfileId == profileId)
                .Select(ss => ss.Skill.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name));
            
            foreach (var skill in directSkills)
            {
                skillSet.Add(skill);
            }
            
            // Add skills from project tech stacks
            var projectSkills = projects
                .Where(p => p.StudentProfileId == profileId && !string.IsNullOrWhiteSpace(p.TechStack))
                .SelectMany(p => p.TechStack.Split(','))
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));
            
            foreach (var skill in projectSkills)
            {
                skillSet.Add(skill);
            }
            
            skillMap[profileId] = string.Join(", ", skillSet.OrderBy(s => s));
        }

        var data = rows.Select(s =>
        {
            var skillText = skillMap.TryGetValue(s.Id, out var value) ? value : string.Empty;
            return new StudentDirectoryItemDto(
                s.Id,
                s.FullName,
                s.University,
                s.Degree,
                s.FieldOfMajor,
                s.GradYear,
                s.CurrentYear,
                s.Gpa,
                s.User.Email,
                skillText,
                s.PhotoDataUrl,
                s.Availability
            );
        });

        if (string.IsNullOrWhiteSpace(term))
        {
            return data.ToList();
        }

        return data
            .Where(d =>
                d.FullName.ToLower().Contains(term) ||
                d.University.ToLower().Contains(term) ||
                d.Degree.ToLower().Contains(term) ||
                d.FieldOfMajor.ToLower().Contains(term) ||
                d.Skills.ToLower().Contains(term)
            )
            .ToList();
    }

    private async Task EnsureStudentDemoDataAsync(User studentUser, StudentProfile studentProfile)
    {
        await EnsureGlobalDemoOpportunitiesAsync();

        var now = DateTime.UtcNow;

        if (!await _context.Projects.AnyAsync(p => p.StudentProfileId == studentProfile.Id))
        {
            _context.Projects.AddRange(
                new Project
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = studentProfile.Id,
                    Title = $"{studentProfile.FullName} - Internship Portfolio",
                    Description = "End-to-end portfolio app with profile management, applications, and analytics.",
                    TechStack = "Next.js, ASP.NET Core, SQL Server, Tailwind CSS",
                    RepositoryUrl = "https://github.com/gradgateway/demo-portfolio",
                    DemoUrl = "https://portfolio.gradgateway.lk/demo",
                    IsPublic = true,
                    CreatedAt = now.AddDays(-21),
                    UpdatedAt = now.AddDays(-3)
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = studentProfile.Id,
                    Title = "Lanka Transit Insights",
                    Description = "Data dashboard for route demand trends and delay prediction in Sri Lanka.",
                    TechStack = "Python, Power BI, PostgreSQL, FastAPI",
                    RepositoryUrl = "https://github.com/gradgateway/lanka-transit-insights",
                    DemoUrl = null,
                    IsPublic = true,
                    CreatedAt = now.AddDays(-30),
                    UpdatedAt = now.AddDays(-5)
                }
            );
        }

        var opportunities = await _context.Opportunities
            .Where(o => o.IsActive)
            .OrderByDescending(o => o.CreatedAt)
            .Take(3)
            .ToListAsync();

        if (!await _context.Applications.AnyAsync(a => a.StudentProfileId == studentProfile.Id) && opportunities.Any())
        {
            var first = opportunities.First();
            _context.Applications.Add(new Application
            {
                Id = Guid.NewGuid(),
                OpportunityId = first.Id,
                StudentProfileId = studentProfile.Id,
                CoverLetter = "I’m excited to contribute and quickly adapt to your engineering workflows.",
                Status = ApplicationStatus.Pending,
                AppliedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-6)
            });

            if (opportunities.Count > 1)
            {
                var second = opportunities[1];
                _context.Applications.Add(new Application
                {
                    Id = Guid.NewGuid(),
                    OpportunityId = second.Id,
                    StudentProfileId = studentProfile.Id,
                    CoverLetter = "My project experience and data background align with this internship.",
                    Status = ApplicationStatus.Shortlisted,
                    AppliedAt = now.AddDays(-10),
                    UpdatedAt = now.AddDays(-4)
                });
            }
        }

        var firstOpportunity = opportunities.FirstOrDefault();
        if (firstOpportunity != null)
        {
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.StudentProfileId == studentProfile.Id && c.CompanyProfileId == firstOpportunity.CompanyProfileId);

            if (existingConversation == null)
            {
                var companyUserId = await _context.CompanyProfiles
                    .Where(c => c.Id == firstOpportunity.CompanyProfileId)
                    .Select(c => c.UserId)
                    .FirstOrDefaultAsync();

                if (companyUserId != Guid.Empty)
                {
                    var conversation = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        StudentProfileId = studentProfile.Id,
                        CompanyProfileId = firstOpportunity.CompanyProfileId,
                        OpportunityId = firstOpportunity.Id,
                        CreatedAt = now.AddDays(-2),
                        LastMessageAt = now.AddHours(-8)
                    };

                    _context.Conversations.Add(conversation);

                    _context.Messages.AddRange(
                        new Message
                        {
                            Id = Guid.NewGuid(),
                            ConversationId = conversation.Id,
                            SenderUserId = studentUser.Id,
                            Content = "Hi! I applied for this role and wanted to ask about the next interview step.",
                            IsRead = true,
                            SentAt = now.AddHours(-10)
                        },
                        new Message
                        {
                            Id = Guid.NewGuid(),
                            ConversationId = conversation.Id,
                            SenderUserId = companyUserId,
                            Content = "Thanks for reaching out. We’ll finalize shortlisting by this weekend.",
                            IsRead = false,
                            SentAt = now.AddHours(-8)
                        }
                    );
                }
            }
        }

        await EnsureStudentSkillsAsync(studentProfile.Id);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureStudentSkillsAsync(Guid studentProfileId)
    {
        if (await _context.StudentSkills.AnyAsync(s => s.StudentProfileId == studentProfileId))
        {
            return;
        }

        var required = new[]
        {
            ("React", "Frontend"),
            ("TypeScript", "Frontend"),
            ("ASP.NET Core", "Backend"),
            ("SQL Server", "Database")
        };

        var skills = new List<Skill>();
        foreach (var (name, category) in required)
        {
            var found = await _context.Skills.FirstOrDefaultAsync(s => s.Name == name);
            if (found == null)
            {
                found = new Skill
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Category = category
                };
                _context.Skills.Add(found);
            }

            skills.Add(found);
        }

        _context.StudentSkills.AddRange(skills.Select((skill, index) => new StudentSkill
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            SkillId = skill.Id,
            ProficiencyLevel = index < 2 ? SkillLevel.Advanced : SkillLevel.Intermediate,
            YearsOfExperience = index < 2 ? 2 : 1
        }));
    }

    private async Task EnsureGlobalDemoOpportunitiesAsync()
    {
        if (await _context.Opportunities.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var companySpecs = new[]
        {
            new { Name = "Dialog Axiata PLC", Email = "careers@dialog.lk", Recruiter = "Kasun Rodrigo", Uid = "runtime-demo-company-dialog" },
            new { Name = "WSO2 Lanka (Pvt) Ltd", Email = "internships@wso2.com", Recruiter = "Ishara Fernando", Uid = "runtime-demo-company-wso2" },
            new { Name = "Virtusa (Pvt) Ltd", Email = "hr@virtusa.com", Recruiter = "Dinithi Abeywickrama", Uid = "runtime-demo-company-virtusa" }
        };

        var companyProfiles = new List<CompanyProfile>();

        foreach (var spec in companySpecs)
        {
            var existingProfile = await _context.CompanyProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CompanyEmail == spec.Email || c.CompanyName == spec.Name);

            if (existingProfile != null)
            {
                companyProfiles.Add(existingProfile);
                continue;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == spec.Email || u.FirebaseUid == spec.Uid);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FirebaseUid = spec.Uid,
                    Email = spec.Email,
                    Role = UserRole.Company,
                    CreatedAt = now
                };
                _context.Users.Add(user);
            }

            var profile = new CompanyProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyName = spec.Name,
                CompanyEmail = spec.Email,
                Phone = "+94110000000",
                Website = "https://www.example.com",
                Industry = "Software & IT",
                RecruiterName = spec.Recruiter,
                RecruiterEmail = spec.Email,
                RecruiterPhone = "+94770000000",
                Position = "Talent Acquisition",
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.CompanyProfiles.Add(profile);
            companyProfiles.Add(profile);
        }

        if (!companyProfiles.Any())
        {
            return;
        }

        var openings = new[]
        {
            new { Title = "Software Engineering Intern", Desc = "Build internal tools with .NET and React.", Skills = "C#, ASP.NET Core, React, SQL Server", Type = OpportunityType.Internship, Mode = WorkMode.Hybrid, Location = "Colombo", Stipend = 80000m, Days = 45 },
            new { Title = "Data Analytics Intern", Desc = "Work with telecom data and BI pipelines.", Skills = "Python, SQL, Power BI, Statistics", Type = OpportunityType.Internship, Mode = WorkMode.Onsite, Location = "Battaramulla", Stipend = 90000m, Days = 55 },
            new { Title = "Associate Software Engineer", Desc = "Graduate role for cloud-native service development.", Skills = "Java, Microservices, Docker, Kubernetes", Type = OpportunityType.GraduateRole, Mode = WorkMode.Hybrid, Location = "Colombo", Stipend = 185000m, Days = 65 },
            new { Title = "QA Automation Intern", Desc = "Automate regression suites with Playwright and CI.", Skills = "Playwright, Selenium, C#, CI/CD", Type = OpportunityType.Internship, Mode = WorkMode.Remote, Location = "Sri Lanka", Stipend = 70000m, Days = 50 }
        };

        for (var i = 0; i < openings.Length; i++)
        {
            var company = companyProfiles[i % companyProfiles.Count];
            _context.Opportunities.Add(new Opportunity
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = company.Id,
                Title = openings[i].Title,
                Description = openings[i].Desc,
                OpportunityType = openings[i].Type,
                WorkMode = openings[i].Mode,
                Location = openings[i].Location,
                RequiredSkills = openings[i].Skills,
                MonthlyStipendLkr = openings[i].Stipend,
                DeadlineAt = now.AddDays(openings[i].Days),
                IsActive = true,
                CreatedAt = now.AddDays(-(i + 1) * 2),
                UpdatedAt = now.AddDays(-(i + 1))
            });
        }

        await _context.SaveChangesAsync();
    }
}
