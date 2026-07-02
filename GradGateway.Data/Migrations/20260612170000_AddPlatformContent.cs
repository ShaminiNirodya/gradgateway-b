using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612170000_AddPlatformContent")]
    public partial class AddPlatformContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[PlatformContents]', N'U') IS NULL
BEGIN
    CREATE TABLE [PlatformContents] (
        [Id] uniqueidentifier NOT NULL,
        [ContentType] nvarchar(20) NOT NULL,
        [Section] nvarchar(20) NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [Summary] nvarchar(1000) NULL,
        [StepsJson] nvarchar(max) NULL,
        [Audiences] nvarchar(100) NOT NULL,
        [Category] nvarchar(120) NULL,
        [Slug] nvarchar(120) NULL,
        [RelatedLinkHref] nvarchar(500) NULL,
        [RelatedLinkLabel] nvarchar(200) NULL,
        [Status] nvarchar(20) NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PlatformContents] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_PlatformContents_ContentType_Section_Status] ON [PlatformContents] ([ContentType], [Section], [Status]);
    CREATE INDEX [IX_PlatformContents_SortOrder] ON [PlatformContents] ([SortOrder]);
END
");

            SeedContent(migrationBuilder);
        }

        private static void SeedContent(MigrationBuilder migrationBuilder)
        {
            var seedAt = "2026-02-01T00:00:00.0000000";

            void Insert(
                string id,
                string contentType,
                string section,
                string title,
                string body,
                string audiences,
                int sortOrder,
                string summary = null,
                string stepsJson = null,
                string category = null,
                string slug = null,
                string linkHref = null,
                string linkLabel = null)
            {
                var summarySql = summary == null ? "NULL" : $"N'{Escape(summary)}'";
                var stepsSql = stepsJson == null ? "NULL" : $"N'{Escape(stepsJson)}'";
                var categorySql = category == null ? "NULL" : $"N'{Escape(category)}'";
                var slugSql = slug == null ? "NULL" : $"N'{Escape(slug)}'";
                var linkHrefSql = linkHref == null ? "NULL" : $"N'{Escape(linkHref)}'";
                var linkLabelSql = linkLabel == null ? "NULL" : $"N'{Escape(linkLabel)}'";

                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [PlatformContents] WHERE [Id] = '{id}')
    INSERT INTO [PlatformContents] (
        [Id], [ContentType], [Section], [Title], [Body], [Summary], [StepsJson], [Audiences],
        [Category], [Slug], [RelatedLinkHref], [RelatedLinkLabel], [Status], [SortOrder], [CreatedAt], [UpdatedAt])
    VALUES (
        '{id}', N'{contentType}', N'{section}', N'{Escape(title)}', N'{Escape(body)}', {summarySql}, {stepsSql},
        N'{audiences}', {categorySql}, {slugSql}, {linkHrefSql}, {linkLabelSql},
        N'Published', {sortOrder}, '{seedAt}', '{seedAt}');
");
            }

            // Public FAQ (/faq)
            Insert("c1010001-0001-0001-0001-000000000001", "Faq", "Public",
                "Is GradGateway free for students?",
                "Yes. Creating a profile, publishing projects, applying to openings, and messaging companies is completely free for students.",
                "All", 1);
            Insert("c1010001-0001-0001-0001-000000000002", "Faq", "Public",
                "Who can register as a student?",
                "Any undergraduate or recent graduate of a Sri Lankan university. You'll add your university, degree, and graduation year when creating your profile.",
                "All", 2);
            Insert("c1010001-0001-0001-0001-000000000003", "Faq", "Public",
                "How do companies find me?",
                "Companies search the talent directory by skills, degree, university, and availability. Your skills come from your profile and the tech stacks of your published projects.",
                "All", 3);
            Insert("c1010001-0001-0001-0001-000000000004", "Faq", "Public",
                "How do I apply for an opening?",
                "Browse Openings in your dashboard, open a role, and submit your application with an optional cover letter. You can track the status under Applications.",
                "All", 4);
            Insert("c1010001-0001-0001-0001-000000000005", "Faq", "Public",
                "What happens after a company shortlists me?",
                "You'll get a notification and the company can message you directly, schedule an interview, or send a job offer — all visible in your dashboard.",
                "All", 5);
            Insert("c1010001-0001-0001-0001-000000000006", "Faq", "Public",
                "Can companies post any kind of role?",
                "Companies post internships, graduate roles, part-time and full-time positions. Our admin team monitors postings, and you can report anything suspicious via support.",
                "All", 6);
            Insert("c1010001-0001-0001-0001-000000000007", "Faq", "Public",
                "How do I delete my account or my data?",
                "Contact support through the contact page and we'll process your request in line with our privacy policy.",
                "All", 7);
            Insert("c1010001-0001-0001-0001-000000000008", "Faq", "Public",
                "I forgot my password. What do I do?",
                "Use the Forgot Password link on the login page. We'll email you a 6-digit verification code to reset your password.",
                "All", 8);

            // Help center FAQ
            Insert("c1020001-0001-0001-0001-000000000001", "Faq", "HelpCenter",
                "How do I get started as a student?",
                "Create an account, complete your profile, browse opportunities, and apply to roles that match your skills.",
                "Student,All", 1);
            Insert("c1020001-0001-0001-0001-000000000002", "Faq", "HelpCenter",
                "How do companies post jobs?",
                "Register as a company, complete your profile, then create opportunities from your dashboard.",
                "Company,All", 2);
            Insert("c1020001-0001-0001-0001-000000000003", "Faq", "HelpCenter",
                "How long until support replies?",
                "We aim to respond within 24–48 hours on business days. Urgent issues can be marked in your message.",
                "Student,Company,All", 3);
            Insert("c1020001-0001-0001-0001-000000000004", "Faq", "HelpCenter",
                "Where does my support message go?",
                "Every request from this page is saved to the platform database and shown in the admin Help & inquiries section.",
                "Student,Company,All", 4);

            // Contact FAQ
            Insert("c1030001-0001-0001-0001-000000000001", "Faq", "Contact",
                "Do I need a GradGateway account to contact you?",
                "No. The contact form is open to everyone. Your inquiry is sent to the admin team even if you have not registered yet.",
                "All", 1);
            Insert("c1030001-0001-0001-0001-000000000002", "Faq", "Contact",
                "How quickly will I receive a response?",
                "We typically respond within 24–48 business hours. Choose Support as the inquiry type for urgent issues.",
                "All", 2);
            Insert("c1030001-0001-0001-0001-000000000003", "Faq", "Contact",
                "What should I include in my message?",
                "Tell us who you are, what you need, and include links or context that help us understand your request.",
                "All", 3);
            Insert("c1030001-0001-0001-0001-000000000004", "Faq", "Contact",
                "Can universities or career centers reach out?",
                "Yes. Select Campus / University or Partnership and describe how you would like to collaborate.",
                "All", 4);

            // Guides
            Insert("c2010001-0001-0001-0001-000000000001", "Guide", "HelpCenter",
                "How to create your first project",
                "",
                "Student,All", 1,
                summary: "Showcase your work so companies can see what you have built.",
                stepsJson: @"[""Open My Projects from the student sidebar."",""Click Add project and enter a clear title and short description."",""List the tech stack you used (e.g. React, ASP.NET Core, SQL Server)."",""Add a repository or demo link if you have one."",""Set the project to public so it appears on your profile and in talent search.""]",
                category: "Profile & Projects",
                slug: "first-project",
                linkHref: "/dashboard/student/projects",
                linkLabel: "Go to My Projects");

            Insert("c2010001-0001-0001-0001-000000000002", "Guide", "HelpCenter",
                "Setting up your student profile",
                "",
                "Student,All", 2,
                summary: "A complete profile helps you stand out when applying to opportunities.",
                stepsJson: @"[""Go to Settings from your student dashboard."",""Fill in your full name, university, degree, and graduation year."",""Upload a profile photo and add your phone number."",""Add skills, GPA, and availability so recruiters can match you faster."",""Save changes — your profile is used on applications and in the talent directory.""]",
                category: "Profile & Projects",
                slug: "student-profile",
                linkHref: "/dashboard/student/settings",
                linkLabel: "Open student settings");

            Insert("c2010001-0001-0001-0001-000000000003", "Guide", "HelpCenter",
                "How to post and manage job opportunities",
                "",
                "Company,All", 3,
                summary: "Publish internships and graduate roles, then track applications in one place.",
                stepsJson: @"[""From the company dashboard, open Job Posts."",""Click Create opportunity and fill in title, description, skills, and location."",""Set the work mode (onsite, hybrid, or remote) and application deadline."",""Publish the post — it becomes visible to students in Openings."",""Edit or deactivate posts anytime from Job Posts; expired posts move off the live feed.""]",
                category: "For Companies",
                slug: "post-jobs",
                linkHref: "/dashboard/company/jobs",
                linkLabel: "Go to Job Posts");

            Insert("c2010001-0001-0001-0001-000000000004", "Guide", "HelpCenter",
                "How to find the right talent",
                "",
                "Company,All", 4,
                summary: "Search the student directory and review projects before you reach out.",
                stepsJson: @"[""Open Talent Search from the company sidebar."",""Filter by university, skills, or keywords that match your role."",""Open a student profile to view their projects, education, and experience."",""Start a conversation or wait for applications on your posted opportunities."",""Shortlist applicants from the Applications page when you are ready to interview.""]",
                category: "Hiring & Talent",
                slug: "find-talent",
                linkHref: "/dashboard/company/talent",
                linkLabel: "Open Talent Search");

            Insert("c2010001-0001-0001-0001-000000000005", "Guide", "HelpCenter",
                "Understanding applications and messaging",
                "",
                "Student,Company,All", 5,
                summary: "How applications, status updates, and messages work on GradGateway.",
                stepsJson: @"[""Students apply from Openings; each application is tied to one opportunity."",""Companies review applications and can update status (pending, shortlisted, hired, etc.)."",""Either side can start a conversation from Messages once there is an application or outreach."",""Notifications alert you to new messages, application updates, and interview invites."",""Keep checking Messages and Applications — timely replies improve hiring outcomes.""]",
                category: "Applications",
                slug: "applications-messaging",
                linkHref: "/dashboard/student/applications",
                linkLabel: "View my applications");
        }

        private static string Escape(string value) => value.Replace("'", "''");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[PlatformContents]', N'U') IS NOT NULL
    DROP TABLE [PlatformContents];
");
        }
    }
}
