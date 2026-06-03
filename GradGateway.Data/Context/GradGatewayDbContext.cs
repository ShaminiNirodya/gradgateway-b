using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using GradGateway.Data.Entities;

namespace GradGateway.Data.Context;

public class GradGatewayDbContext : DbContext
{
    public GradGatewayDbContext(DbContextOptions<GradGatewayDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<CompanyProfile> CompanyProfiles { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<SavedOpportunity> SavedOpportunities { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectImage> ProjectImages { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<StudentSkill> StudentSkills { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<OpportunityInterviewPlan> OpportunityInterviewPlans { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<CompanyTeamMember> CompanyTeamMembers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var seedCreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var seedUpdatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var adminUserId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef");
        var studentUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var studentUserId2 = Guid.Parse("11111111-2222-3333-4444-666666666666");
        var studentUserId3 = Guid.Parse("11111111-2222-3333-4444-777777777777");
        var studentUserId4 = Guid.Parse("11111111-2222-3333-4444-888888888888");

        var companyUserId = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa");
        var companyUserId2 = Guid.Parse("66666666-7777-8888-9999-bbbbbbbbbbbb");
        var companyUserId3 = Guid.Parse("66666666-7777-8888-9999-cccccccccccc");
        var companyUserId4 = Guid.Parse("66666666-7777-8888-9999-dddddddddddd");

        var studentProfileId = Guid.Parse("bbbbbbbb-1111-2222-3333-444444444444");
        var studentProfileId2 = Guid.Parse("bbbbbbbb-1111-2222-3333-555555555555");
        var studentProfileId3 = Guid.Parse("bbbbbbbb-1111-2222-3333-666666666666");
        var studentProfileId4 = Guid.Parse("bbbbbbbb-1111-2222-3333-777777777777");

        var companyProfileId = Guid.Parse("cccccccc-1111-2222-3333-444444444444");
        var companyProfileId2 = Guid.Parse("cccccccc-1111-2222-3333-555555555555");
        var companyProfileId3 = Guid.Parse("cccccccc-1111-2222-3333-666666666666");
        var companyProfileId4 = Guid.Parse("cccccccc-1111-2222-3333-777777777777");

        var opportunityId1 = Guid.Parse("dddddddd-1111-2222-3333-444444444444");
        var opportunityId2 = Guid.Parse("dddddddd-1111-2222-3333-555555555555");
        var opportunityId3 = Guid.Parse("dddddddd-1111-2222-3333-666666666666");
        var opportunityId4 = Guid.Parse("dddddddd-1111-2222-3333-777777777777");

        var applicationId1 = Guid.Parse("eeeeeeee-1111-2222-3333-444444444444");
        var applicationId2 = Guid.Parse("eeeeeeee-1111-2222-3333-555555555555");
        var applicationId3 = Guid.Parse("eeeeeeee-1111-2222-3333-666666666666");

        var savedId1 = Guid.Parse("ffffffff-1111-2222-3333-444444444444");
        var savedId2 = Guid.Parse("ffffffff-1111-2222-3333-555555555555");

        var conversationId1 = Guid.Parse("abababab-1111-2222-3333-444444444444");
        var conversationId2 = Guid.Parse("abababab-1111-2222-3333-555555555555");

        var messageId1 = Guid.Parse("acacacac-1111-2222-3333-444444444444");
        var messageId2 = Guid.Parse("acacacac-1111-2222-3333-555555555555");
        var messageId3 = Guid.Parse("acacacac-1111-2222-3333-666666666666");

        var notificationId1 = Guid.Parse("adadadad-1111-2222-3333-444444444444");
        var notificationId2 = Guid.Parse("adadadad-1111-2222-3333-555555555555");
        var notificationId3 = Guid.Parse("adadadad-1111-2222-3333-666666666666");

        var projectId1 = Guid.Parse("aeaeaeae-1111-2222-3333-444444444444");
        var projectId2 = Guid.Parse("aeaeaeae-1111-2222-3333-555555555555");
        var projectId3 = Guid.Parse("aeaeaeae-1111-2222-3333-666666666666");

        var skillId1 = Guid.Parse("afafafaf-1111-2222-3333-444444444444");
        var skillId2 = Guid.Parse("afafafaf-1111-2222-3333-555555555555");
        var skillId3 = Guid.Parse("afafafaf-1111-2222-3333-666666666666");
        var skillId4 = Guid.Parse("afafafaf-1111-2222-3333-777777777777");
        var skillId5 = Guid.Parse("afafafaf-1111-2222-3333-888888888888");

        var studentSkillId1 = Guid.Parse("b0b0b0b0-1111-2222-3333-444444444444");
        var studentSkillId2 = Guid.Parse("b0b0b0b0-1111-2222-3333-555555555555");
        var studentSkillId3 = Guid.Parse("b0b0b0b0-1111-2222-3333-666666666666");
        var studentSkillId4 = Guid.Parse("b0b0b0b0-1111-2222-3333-777777777777");
        var studentSkillId5 = Guid.Parse("b0b0b0b0-1111-2222-3333-888888888888");

        var interviewId1 = Guid.Parse("b1b1b1b1-1111-2222-3333-444444444444");
        var interviewId2 = Guid.Parse("b1b1b1b1-1111-2222-3333-555555555555");

        var documentId1 = Guid.Parse("b2b2b2b2-1111-2222-3333-444444444444");
        var documentId2 = Guid.Parse("b2b2b2b2-1111-2222-3333-555555555555");
        var documentId3 = Guid.Parse("b2b2b2b2-1111-2222-3333-666666666666");

        // Seed users
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminUserId,
            FirebaseUid = "CffvlJMXIlUM3FRQzNyu80hXBkU2", 
            Email = "admin@gradgateway.com",
            Role = UserRole.Admin,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = studentUserId,
            FirebaseUid = "demo-student-uid-001",
            Email = "student.demo@uom.lk",
            Role = UserRole.Student,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = studentUserId2,
            FirebaseUid = "demo-student-uid-002",
            Email = "nethmi.perera@eng.pdn.ac.lk",
            Role = UserRole.Student,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = studentUserId3,
            FirebaseUid = "demo-student-uid-003",
            Email = "sahan.jayasinghe@stu.cmb.ac.lk",
            Role = UserRole.Student,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = studentUserId4,
            FirebaseUid = "demo-student-uid-004",
            Email = "tharushi.senanayake@jfn.ac.lk",
            Role = UserRole.Student,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = companyUserId,
            FirebaseUid = "demo-company-uid-001",
            Email = "company.demo@sample.lk",
            Role = UserRole.Company,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = companyUserId2,
            FirebaseUid = "demo-company-uid-002",
            Email = "careers@dialog.lk",
            Role = UserRole.Company,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = companyUserId3,
            FirebaseUid = "demo-company-uid-003",
            Email = "internships@wso2.com",
            Role = UserRole.Company,
            CreatedAt = seedCreatedAt
        },
        new User
        {
            Id = companyUserId4,
            FirebaseUid = "demo-company-uid-004",
            Email = "hr@virtusa.com",
            Role = UserRole.Company,
            CreatedAt = seedCreatedAt
        });

        // Add unique index on FirebaseUid
        modelBuilder.Entity<User>()
            .HasIndex(u => u.FirebaseUid)
            .IsUnique();

        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ToEmail).HasMaxLength(320);
            entity.Property(e => e.TemplateType).HasMaxLength(100);
            entity.Property(e => e.Purpose).HasMaxLength(120);
            entity.Property(e => e.Provider).HasMaxLength(80);
            entity.Property(e => e.Status).HasMaxLength(40);
            entity.Property(e => e.ProviderMessageId).HasMaxLength(200);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasIndex(p => p.StudentId).IsUnique();
            entity.HasIndex(p => p.UserId).IsUnique();

            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Gpa)
                .HasPrecision(3, 2);
        });

        modelBuilder.Entity<StudentProfile>().HasData(new StudentProfile
        {
            Id = studentProfileId,
            UserId = studentUserId,
            FullName = "Demo Student",
            Phone = "+94771234567",
            PhotoDataUrl = null,
            University = "University of Moratuwa",
            StudentId = "UOM2024001",
            Degree = "BSc (Hons) in IT",
            GradYear = 2027,
            Gpa = 3.45m,
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        },
        new StudentProfile
        {
            Id = studentProfileId2,
            UserId = studentUserId2,
            FullName = "Nethmi Perera",
            Phone = "+94712223344",
            PhotoDataUrl = null,
            University = "University of Peradeniya",
            StudentId = "PDN2023123",
            Degree = "BSc Engineering",
            GradYear = 2026,
            Gpa = 3.82m,
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        },
        new StudentProfile
        {
            Id = studentProfileId3,
            UserId = studentUserId3,
            FullName = "Sahan Jayasinghe",
            Phone = "+94773334455",
            PhotoDataUrl = null,
            University = "University of Colombo",
            StudentId = "UOC2022110",
            Degree = "BSc (Hons) in Computer Science",
            GradYear = 2025,
            Gpa = 3.67m,
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        },
        new StudentProfile
        {
            Id = studentProfileId4,
            UserId = studentUserId4,
            FullName = "Tharushi Senanayake",
            Phone = "+94764445566",
            PhotoDataUrl = null,
            University = "University of Jaffna",
            StudentId = "UOJ2023567",
            Degree = "BSc in Information Technology",
            GradYear = 2027,
            Gpa = 3.29m,
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.HasIndex(p => p.UserId).IsUnique();

            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompanyTeamMember>(entity =>
        {
            entity.HasIndex(m => new { m.CompanyProfileId, m.Email }).IsUnique();
            entity.HasIndex(m => m.InvitationToken).IsUnique();

            entity.Property(m => m.Name).HasMaxLength(120);
            entity.Property(m => m.Email).HasMaxLength(320);
            entity.Property(m => m.Role).HasMaxLength(80);
            entity.Property(m => m.Status).HasMaxLength(24);
            entity.Property(m => m.InvitationToken).HasMaxLength(200);

            entity.HasOne(m => m.CompanyProfile)
                .WithMany()
                .HasForeignKey(m => m.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.InvitedByUser)
                .WithMany()
                .HasForeignKey(m => m.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompanyProfile>().HasData(new CompanyProfile
        {
            Id = companyProfileId,
            UserId = companyUserId,
            CompanyName = "Demo Technologies (Pvt) Ltd",
            CompanyEmail = "careers@demotech.lk",
            Phone = "+94114567890",
            Website = "https://demotech.lk",
            Industry = "Software & IT",
            LogoDataUrl = null,
            RecruiterName = "Anjana Silva",
            RecruiterEmail = "anjana@demotech.lk",
            RecruiterPhone = "+94770111222",
            Position = "HR Manager",
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        },
        new CompanyProfile
        {
            Id = companyProfileId2,
            UserId = companyUserId2,
            CompanyName = "Dialog Axiata PLC",
            CompanyEmail = "talent@dialog.lk",
            Phone = "+94117722345",
            Website = "https://www.dialog.lk",
            Industry = "Telecommunications",
            LogoDataUrl = null,
            RecruiterName = "Kasun Rodrigo",
            RecruiterEmail = "kasun.rodrigo@dialog.lk",
            RecruiterPhone = "+94771239876",
            Position = "Talent Acquisition Executive",
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        },
        new CompanyProfile
        {
            Id = companyProfileId3,
            UserId = companyUserId3,
            CompanyName = "WSO2 Lanka (Pvt) Ltd",
            CompanyEmail = "campus@wso2.com",
            Phone = "+94112678901",
            Website = "https://wso2.com",
            Industry = "Software & IT",
            LogoDataUrl = null,
            RecruiterName = "Ishara Fernando",
            RecruiterEmail = "ishara.fernando@wso2.com",
            RecruiterPhone = "+94712340987",
            Position = "Campus Recruiter",
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        },
        new CompanyProfile
        {
            Id = companyProfileId4,
            UserId = companyUserId4,
            CompanyName = "Virtusa (Pvt) Ltd",
            CompanyEmail = "university.relations@virtusa.com",
            Phone = "+94117890123",
            Website = "https://www.virtusa.com",
            Industry = "Software & IT",
            LogoDataUrl = null,
            RecruiterName = "Dinithi Abeywickrama",
            RecruiterEmail = "dinithi.abeywickrama@virtusa.com",
            RecruiterPhone = "+94770123456",
            Position = "Associate Manager - Talent",
            CreatedAt = seedCreatedAt,
            UpdatedAt = seedUpdatedAt
        });

        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasIndex(p => p.CompanyProfileId);
            entity.HasIndex(p => p.DeadlineAt);

            entity.HasOne(p => p.CompanyProfile)
                .WithMany()
                .HasForeignKey(p => p.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.MonthlyStipendLkr)
                .HasPrecision(12, 2);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasIndex(p => p.OpportunityId);
            entity.HasIndex(p => p.StudentProfileId);
            entity.HasIndex(p => new { p.OpportunityId, p.StudentProfileId })
                .IsUnique()
                .HasFilter("[OpportunityId] IS NOT NULL");

            entity.HasOne(p => p.Opportunity)
                .WithMany()
                .HasForeignKey(p => p.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.StudentProfile)
                .WithMany()
                .HasForeignKey(p => p.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SavedOpportunity>(entity =>
        {
            entity.HasIndex(p => p.StudentProfileId);
            entity.HasIndex(p => p.OpportunityId);
            entity.HasIndex(p => new { p.StudentProfileId, p.OpportunityId }).IsUnique();

            entity.HasOne(p => p.StudentProfile)
                .WithMany()
                .HasForeignKey(p => p.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Opportunity)
                .WithMany()
                .HasForeignKey(p => p.OpportunityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasIndex(p => p.StudentProfileId);
            entity.HasIndex(p => p.CompanyProfileId);
            entity.HasIndex(p => p.OpportunityId);

            entity.HasOne(p => p.StudentProfile)
                .WithMany()
                .HasForeignKey(p => p.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.CompanyProfile)
                .WithMany()
                .HasForeignKey(p => p.CompanyProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Opportunity)
                .WithMany()
                .HasForeignKey(p => p.OpportunityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(p => p.ConversationId);
            entity.HasIndex(p => p.SenderUserId);
            entity.HasIndex(p => p.SentAt);

            entity.HasOne(p => p.Conversation)
                .WithMany()
                .HasForeignKey(p => p.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.SenderUser)
                .WithMany()
                .HasForeignKey(p => p.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.IsRead);
            entity.HasIndex(p => p.CreatedAt);

            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => p.StudentProfileId);
            entity.HasIndex(p => p.IsPublic);

            entity.HasOne(p => p.StudentProfile)
                .WithMany()
                .HasForeignKey(p => p.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasIndex(p => p.Name).IsUnique();
        });

        modelBuilder.Entity<StudentSkill>(entity =>
        {
            entity.HasIndex(p => p.StudentProfileId);
            entity.HasIndex(p => p.SkillId);
            entity.HasIndex(p => new { p.StudentProfileId, p.SkillId }).IsUnique();

            entity.HasOne(p => p.StudentProfile)
                .WithMany()
                .HasForeignKey(p => p.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Skill)
                .WithMany()
                .HasForeignKey(p => p.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OpportunityInterviewPlan>(entity =>
        {
            entity.HasIndex(p => p.OpportunityId).IsUnique();

            entity.HasOne(p => p.Opportunity)
                .WithMany()
                .HasForeignKey(p => p.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasIndex(p => p.ApplicationId).IsUnique();
            entity.HasIndex(p => p.ScheduledAt);

            entity.HasOne(p => p.Application)
                .WithMany()
                .HasForeignKey(p => p.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(p => p.StudentProfileId);
            entity.HasIndex(p => p.FileType);

            entity.HasOne(p => p.StudentProfile)
                .WithMany()
                .HasForeignKey(p => p.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Opportunity>().HasData(
            new Opportunity
            {
                Id = opportunityId1,
                CompanyProfileId = companyProfileId,
                Title = "Software Engineering Intern",
                Description = "Internship for 3rd year undergraduates with C# and React exposure.",
                OpportunityType = OpportunityType.Internship,
                WorkMode = WorkMode.Hybrid,
                Location = "Colombo 03",
                RequiredSkills = "C#, ASP.NET Core, React, SQL Server",
                MonthlyStipendLkr = 75000m,
                DeadlineAt = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Opportunity
            {
                Id = opportunityId2,
                CompanyProfileId = companyProfileId2,
                Title = "Data Analytics Intern",
                Description = "Hands-on analytics internship with telecom datasets and Power BI.",
                OpportunityType = OpportunityType.Internship,
                WorkMode = WorkMode.Onsite,
                Location = "Battaramulla",
                RequiredSkills = "Python, SQL, Power BI, Statistics",
                MonthlyStipendLkr = 85000m,
                DeadlineAt = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Opportunity
            {
                Id = opportunityId3,
                CompanyProfileId = companyProfileId3,
                Title = "Associate Software Engineer",
                Description = "Entry-level role for fresh graduates interested in cloud-native development.",
                OpportunityType = OpportunityType.GraduateRole,
                WorkMode = WorkMode.Hybrid,
                Location = "Colombo 07",
                RequiredSkills = "Java, Microservices, Docker, Kubernetes",
                MonthlyStipendLkr = 180000m,
                DeadlineAt = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Opportunity
            {
                Id = opportunityId4,
                CompanyProfileId = companyProfileId4,
                Title = "QA Automation Intern",
                Description = "Internship focused on test automation for enterprise applications.",
                OpportunityType = OpportunityType.Internship,
                WorkMode = WorkMode.Remote,
                Location = "Sri Lanka",
                RequiredSkills = "Selenium, Playwright, C#, CI/CD",
                MonthlyStipendLkr = 70000m,
                DeadlineAt = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<Application>().HasData(
            new Application
            {
                Id = applicationId1,
                OpportunityId = opportunityId1,
                StudentProfileId = studentProfileId,
                CoverLetter = "I am eager to contribute to ASP.NET Core projects and learn from your team.",
                Status = ApplicationStatus.Pending,
                AppliedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Application
            {
                Id = applicationId2,
                OpportunityId = opportunityId2,
                StudentProfileId = studentProfileId2,
                CoverLetter = "My data analytics coursework and projects align with this opportunity.",
                Status = ApplicationStatus.Shortlisted,
                AppliedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Application
            {
                Id = applicationId3,
                OpportunityId = opportunityId3,
                StudentProfileId = studentProfileId3,
                CoverLetter = "I have strong backend foundations and cloud fundamentals.",
                Status = ApplicationStatus.Pending,
                AppliedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<SavedOpportunity>().HasData(
            new SavedOpportunity
            {
                Id = savedId1,
                StudentProfileId = studentProfileId,
                OpportunityId = opportunityId2,
                SavedAt = seedCreatedAt
            },
            new SavedOpportunity
            {
                Id = savedId2,
                StudentProfileId = studentProfileId4,
                OpportunityId = opportunityId4,
                SavedAt = seedCreatedAt
            }
        );

        modelBuilder.Entity<Conversation>().HasData(
            new Conversation
            {
                Id = conversationId1,
                StudentProfileId = studentProfileId,
                CompanyProfileId = companyProfileId,
                OpportunityId = opportunityId1,
                CreatedAt = seedCreatedAt,
                LastMessageAt = seedUpdatedAt
            },
            new Conversation
            {
                Id = conversationId2,
                StudentProfileId = studentProfileId2,
                CompanyProfileId = companyProfileId2,
                OpportunityId = opportunityId2,
                CreatedAt = seedCreatedAt,
                LastMessageAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<Message>().HasData(
            new Message
            {
                Id = messageId1,
                ConversationId = conversationId1,
                SenderUserId = studentUserId,
                Content = "Good morning, I submitted my application. Could you share the interview timeline?",
                IsRead = true,
                SentAt = seedCreatedAt
            },
            new Message
            {
                Id = messageId2,
                ConversationId = conversationId1,
                SenderUserId = companyUserId,
                Content = "Thanks! Shortlisting will be completed by next week.",
                IsRead = true,
                SentAt = seedUpdatedAt
            },
            new Message
            {
                Id = messageId3,
                ConversationId = conversationId2,
                SenderUserId = companyUserId2,
                Content = "Please upload your updated CV and transcript.",
                IsRead = false,
                SentAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<Notification>().HasData(
            new Notification
            {
                Id = notificationId1,
                UserId = studentUserId,
                Type = NotificationType.Application,
                Title = "Application Submitted",
                Body = "Your application for Software Engineering Intern was submitted successfully.",
                IsRead = true,
                CreatedAt = seedCreatedAt
            },
            new Notification
            {
                Id = notificationId2,
                UserId = studentUserId2,
                Type = NotificationType.Application,
                Title = "Application Shortlisted",
                Body = "You have been shortlisted for Data Analytics Intern at Dialog Axiata PLC.",
                IsRead = false,
                CreatedAt = seedUpdatedAt
            },
            new Notification
            {
                Id = notificationId3,
                UserId = companyUserId,
                Type = NotificationType.Message,
                Title = "New Candidate Message",
                Body = "You received a new message from Demo Student.",
                IsRead = false,
                CreatedAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = projectId1,
                StudentProfileId = studentProfileId,
                Title = "GradGateway Portfolio",
                Description = "A full-stack web platform for student recruitment workflow.",
                TechStack = "Next.js, ASP.NET Core, SQL Server",
                RepositoryUrl = "https://github.com/demo/gradgateway-portfolio",
                DemoUrl = "https://portfolio.demo.lk/gradgateway",
                IsPublic = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Project
            {
                Id = projectId2,
                StudentProfileId = studentProfileId2,
                Title = "Lanka Bus Analytics",
                Description = "Data analytics dashboard for Sri Lankan intercity bus utilization.",
                TechStack = "Python, Power BI, PostgreSQL",
                RepositoryUrl = "https://github.com/demo/lanka-bus-analytics",
                DemoUrl = null,
                IsPublic = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Project
            {
                Id = projectId3,
                StudentProfileId = studentProfileId3,
                Title = "Campus Event Hub",
                Description = "Mobile-first event management system for university societies.",
                TechStack = "React Native, Firebase, Node.js",
                RepositoryUrl = "https://github.com/demo/campus-event-hub",
                DemoUrl = "https://eventhub.demo.lk",
                IsPublic = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = skillId1, Name = "C#", Category = "Backend" },
            new Skill { Id = skillId2, Name = "ASP.NET Core", Category = "Backend" },
            new Skill { Id = skillId3, Name = "React", Category = "Frontend" },
            new Skill { Id = skillId4, Name = "SQL Server", Category = "Database" },
            new Skill { Id = skillId5, Name = "Power BI", Category = "Data" }
        );

        modelBuilder.Entity<StudentSkill>().HasData(
            new StudentSkill
            {
                Id = studentSkillId1,
                StudentProfileId = studentProfileId,
                SkillId = skillId1,
                ProficiencyLevel = SkillLevel.Advanced,
                YearsOfExperience = 2
            },
            new StudentSkill
            {
                Id = studentSkillId2,
                StudentProfileId = studentProfileId,
                SkillId = skillId2,
                ProficiencyLevel = SkillLevel.Intermediate,
                YearsOfExperience = 1
            },
            new StudentSkill
            {
                Id = studentSkillId3,
                StudentProfileId = studentProfileId2,
                SkillId = skillId5,
                ProficiencyLevel = SkillLevel.Advanced,
                YearsOfExperience = 2
            },
            new StudentSkill
            {
                Id = studentSkillId4,
                StudentProfileId = studentProfileId3,
                SkillId = skillId3,
                ProficiencyLevel = SkillLevel.Advanced,
                YearsOfExperience = 3
            },
            new StudentSkill
            {
                Id = studentSkillId5,
                StudentProfileId = studentProfileId3,
                SkillId = skillId4,
                ProficiencyLevel = SkillLevel.Intermediate,
                YearsOfExperience = 2
            }
        );

        modelBuilder.Entity<Interview>().HasData(
            new Interview
            {
                Id = interviewId1,
                ApplicationId = applicationId1,
                ScheduledAt = new DateTime(2026, 3, 10, 4, 0, 0, DateTimeKind.Utc),
                Mode = InterviewMode.Online,
                MeetingLink = "https://meet.google.com/demo-gradgateway-1",
                Location = null,
                Status = InterviewStatus.Scheduled,
                Notes = "First-round technical interview",
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            },
            new Interview
            {
                Id = interviewId2,
                ApplicationId = applicationId2,
                ScheduledAt = new DateTime(2026, 3, 12, 5, 30, 0, DateTimeKind.Utc),
                Mode = InterviewMode.Onsite,
                MeetingLink = null,
                Location = "Dialog HQ, Battaramulla",
                Status = InterviewStatus.Scheduled,
                Notes = "Case study and discussion",
                CreatedAt = seedCreatedAt,
                UpdatedAt = seedUpdatedAt
            }
        );

        modelBuilder.Entity<Document>().HasData(
            new Document
            {
                Id = documentId1,
                StudentProfileId = studentProfileId,
                FileName = "Demo_Student_CV.pdf",
                FileType = "CV",
                FileUrl = "https://storage.gradgateway.lk/docs/demo-student-cv.pdf",
                IsPublic = false,
                UploadedAt = seedCreatedAt
            },
            new Document
            {
                Id = documentId2,
                StudentProfileId = studentProfileId2,
                FileName = "Nethmi_Perera_Transcript.pdf",
                FileType = "Transcript",
                FileUrl = "https://storage.gradgateway.lk/docs/nethmi-transcript.pdf",
                IsPublic = false,
                UploadedAt = seedCreatedAt
            },
            new Document
            {
                Id = documentId3,
                StudentProfileId = studentProfileId3,
                FileName = "Sahan_Portfolio.pdf",
                FileType = "Portfolio",
                FileUrl = "https://storage.gradgateway.lk/docs/sahan-portfolio.pdf",
                IsPublic = true,
                UploadedAt = seedUpdatedAt
            }
        );
    }
}
