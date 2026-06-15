using System.Text.Json;
using GradGateway.Data.Entities;

namespace GradGateway.Data.Seeding;

/// <summary>Default FAQs, guides, and articles for public and help pages.</summary>
public static class PlatformContentSeed
{
    private static readonly DateTime SeedAt = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<PlatformContent> GetAll()
    {
        var items = new List<PlatformContent>();
        items.AddRange(GetPublicFaqs());
        items.AddRange(GetHelpCenterFaqs());
        items.AddRange(GetContactFaqs());
        items.AddRange(GetGuides());
        items.AddRange(GetArticles());
        items.AddRange(LegalContentSeed.GetAll());
        return items;
    }

    private static PlatformContent Faq(
        Guid id,
        string section,
        string title,
        string body,
        string audiences,
        int sortOrder) =>
        new()
        {
            Id = id,
            ContentType = "Faq",
            Section = section,
            Title = title,
            Body = body,
            Audiences = audiences,
            Status = "Published",
            SortOrder = sortOrder,
            CreatedAt = SeedAt,
            UpdatedAt = SeedAt,
        };

    private static PlatformContent Guide(
        Guid id,
        string title,
        string summary,
        IReadOnlyList<string> steps,
        string audiences,
        int sortOrder,
        string category,
        string slug,
        string linkHref,
        string linkLabel) =>
        new()
        {
            Id = id,
            ContentType = "Guide",
            Section = "HelpCenter",
            Title = title,
            Body = string.Empty,
            Summary = summary,
            StepsJson = JsonSerializer.Serialize(steps),
            Audiences = audiences,
            Category = category,
            Slug = slug,
            RelatedLinkHref = linkHref,
            RelatedLinkLabel = linkLabel,
            Status = "Published",
            SortOrder = sortOrder,
            CreatedAt = SeedAt,
            UpdatedAt = SeedAt,
        };

    private static PlatformContent Article(
        Guid id,
        string section,
        string title,
        string summary,
        string body,
        string audiences,
        int sortOrder,
        string? category = null,
        string? slug = null) =>
        new()
        {
            Id = id,
            ContentType = "Article",
            Section = section,
            Title = title,
            Summary = summary,
            Body = body,
            Audiences = audiences,
            Category = category,
            Slug = slug,
            Status = "Published",
            SortOrder = sortOrder,
            CreatedAt = SeedAt,
            UpdatedAt = SeedAt,
        };

    private static IEnumerable<PlatformContent> GetPublicFaqs()
    {
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000001"),
            "Public",
            "Is GradGateway free for students?",
            "Yes. Creating a profile, publishing projects, applying to openings, and messaging companies is completely free for students.",
            "All",
            1);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000002"),
            "Public",
            "Who can register as a student?",
            "Any undergraduate or recent graduate of a Sri Lankan university. You'll add your university, degree, and graduation year when creating your profile.",
            "All",
            2);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000003"),
            "Public",
            "How do companies find me?",
            "Companies search the talent directory by skills, degree, university, and availability. Your skills come from your profile and the tech stacks of your published projects.",
            "All",
            3);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000004"),
            "Public",
            "How do I apply for an opening?",
            "Browse Openings in your dashboard, open a role, and submit your application with an optional cover letter. You can track the status under Applications.",
            "All",
            4);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000005"),
            "Public",
            "What happens after a company shortlists me?",
            "You'll get a notification and the company can message you directly, schedule an interview, or send a job offer — all visible in your dashboard.",
            "All",
            5);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000006"),
            "Public",
            "Can companies post any kind of role?",
            "Companies post internships, graduate roles, part-time and full-time positions. Our admin team monitors postings, and you can report anything suspicious via support.",
            "All",
            6);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000007"),
            "Public",
            "How do I delete my account or my data?",
            "Contact support through the contact page and we'll process your request in line with our privacy policy.",
            "All",
            7);
        yield return Faq(
            Guid.Parse("c1010001-0001-0001-0001-000000000008"),
            "Public",
            "I forgot my password. What do I do?",
            "Use the Forgot Password link on the login page. We'll email you a 6-digit verification code to reset your password.",
            "All",
            8);
    }

    private static IEnumerable<PlatformContent> GetHelpCenterFaqs()
    {
        yield return Faq(
            Guid.Parse("c1020001-0001-0001-0001-000000000001"),
            "HelpCenter",
            "How do I get started as a student?",
            "Create an account, complete your profile, browse opportunities, and apply to roles that match your skills.",
            "Student,All",
            1);
        yield return Faq(
            Guid.Parse("c1020001-0001-0001-0001-000000000002"),
            "HelpCenter",
            "How do companies post jobs?",
            "Register as a company, complete your profile, then create opportunities from your dashboard.",
            "Company,All",
            2);
        yield return Faq(
            Guid.Parse("c1020001-0001-0001-0001-000000000003"),
            "HelpCenter",
            "How long until support replies?",
            "We aim to respond within 24–48 hours on business days. Urgent issues can be marked in your message.",
            "Student,Company,All",
            3);
        yield return Faq(
            Guid.Parse("c1020001-0001-0001-0001-000000000004"),
            "HelpCenter",
            "Where does my support message go?",
            "Every request from this page is saved to the platform database and shown in the admin Help & inquiries section.",
            "Student,Company,All",
            4);
    }

    private static IEnumerable<PlatformContent> GetContactFaqs()
    {
        yield return Faq(
            Guid.Parse("c1030001-0001-0001-0001-000000000001"),
            "Contact",
            "Do I need a GradGateway account to contact you?",
            "No. The contact form is open to everyone. Your inquiry is sent to the admin team even if you have not registered yet.",
            "All",
            1);
        yield return Faq(
            Guid.Parse("c1030001-0001-0001-0001-000000000002"),
            "Contact",
            "How quickly will I receive a response?",
            "We typically respond within 24–48 business hours. Choose Support as the inquiry type for urgent issues.",
            "All",
            2);
        yield return Faq(
            Guid.Parse("c1030001-0001-0001-0001-000000000003"),
            "Contact",
            "What should I include in my message?",
            "Tell us who you are, what you need, and include links or context that help us understand your request.",
            "All",
            3);
        yield return Faq(
            Guid.Parse("c1030001-0001-0001-0001-000000000004"),
            "Contact",
            "Can universities or career centers reach out?",
            "Yes. Select Campus / University or Partnership and describe how you would like to collaborate.",
            "All",
            4);
    }

    private static IEnumerable<PlatformContent> GetGuides()
    {
        yield return Guide(
            Guid.Parse("c2010001-0001-0001-0001-000000000001"),
            "How to create your first project",
            "Showcase your work so companies can see what you have built.",
            new[]
            {
                "Open My Projects from the student sidebar.",
                "Click Add project and enter a clear title and short description.",
                "List the tech stack you used (e.g. React, ASP.NET Core, SQL Server).",
                "Add a repository or demo link if you have one.",
                "Set the project to public so it appears on your profile and in talent search.",
            },
            "Student,All",
            1,
            "Profile & Projects",
            "first-project",
            "/dashboard/student/projects",
            "Go to My Projects");

        yield return Guide(
            Guid.Parse("c2010001-0001-0001-0001-000000000002"),
            "Setting up your student profile",
            "A complete profile helps you stand out when applying to opportunities.",
            new[]
            {
                "Go to Settings from your student dashboard.",
                "Fill in your full name, university, degree, and graduation year.",
                "Upload a profile photo and add your phone number.",
                "Add skills, GPA, and availability so recruiters can match you faster.",
                "Save changes — your profile is used on applications and in the talent directory.",
            },
            "Student,All",
            2,
            "Profile & Projects",
            "student-profile",
            "/dashboard/student/settings",
            "Open student settings");

        yield return Guide(
            Guid.Parse("c2010001-0001-0001-0001-000000000003"),
            "How to post and manage job opportunities",
            "Publish internships and graduate roles, then track applications in one place.",
            new[]
            {
                "From the company dashboard, open Job Posts.",
                "Click Create opportunity and fill in title, description, skills, and location.",
                "Set the work mode (onsite, hybrid, or remote) and application deadline.",
                "Publish the post — it becomes visible to students in Openings.",
                "Edit or deactivate posts anytime from Job Posts; expired posts move off the live feed.",
            },
            "Company,All",
            3,
            "For Companies",
            "post-jobs",
            "/dashboard/company/jobs",
            "Go to Job Posts");

        yield return Guide(
            Guid.Parse("c2010001-0001-0001-0001-000000000004"),
            "How to find the right talent",
            "Search the student directory and review projects before you reach out.",
            new[]
            {
                "Open Talent Search from the company sidebar.",
                "Filter by university, skills, or keywords that match your role.",
                "Open a student profile to view their projects, education, and experience.",
                "Start a conversation or wait for applications on your posted opportunities.",
                "Shortlist applicants from the Applications page when you are ready to interview.",
            },
            "Company,All",
            4,
            "Hiring & Talent",
            "find-talent",
            "/dashboard/company/talent",
            "Open Talent Search");

        yield return Guide(
            Guid.Parse("c2010001-0001-0001-0001-000000000005"),
            "Understanding applications and messaging",
            "How applications, status updates, and messages work on GradGateway.",
            new[]
            {
                "Students apply from Openings; each application is tied to one opportunity.",
                "Companies review applications and can update status (pending, shortlisted, hired, etc.).",
                "Either side can start a conversation from Messages once there is an application or outreach.",
                "Notifications alert you to new messages, application updates, and interview invites.",
                "Keep checking Messages and Applications — timely replies improve hiring outcomes.",
            },
            "Student,Company,All",
            5,
            "Applications",
            "applications-messaging",
            "/dashboard/student/applications",
            "View my applications");
    }

    private static IEnumerable<PlatformContent> GetArticles()
    {
        yield return Article(
            Guid.Parse("c3010001-0001-0001-0001-000000000001"),
            "HelpCenter",
            "Tips for a standout student CV",
            "Small changes that help recruiters understand your fit quickly.",
            "Lead with your degree, university, and graduation year. Highlight 2–3 projects with links to repos or demos. List skills that match the roles you want, and keep descriptions concise — recruiters often scan profiles in under a minute.",
            "Student,All",
            1,
            "Profile & Projects",
            "student-cv-tips");

        yield return Article(
            Guid.Parse("c3010001-0001-0001-0001-000000000002"),
            "HelpCenter",
            "Writing job posts that attract graduates",
            "Clear roles get better applications.",
            "Use a specific title, state whether the role is internship or graduate, and mention the tech stack honestly. Include location or remote policy, stipend or salary range if possible, and a realistic deadline so students can plan applications.",
            "Company,All",
            2,
            "For Companies",
            "job-post-tips");

        yield return Article(
            Guid.Parse("c3010001-0001-0001-0001-000000000003"),
            "HelpCenter",
            "Staying safe when sharing your profile",
            "How GradGateway handles your data and visibility.",
            "You control which projects are public. Companies only see what you publish and what you submit in applications. Report suspicious messages or postings via support — our admin team reviews every inquiry.",
            "Student,Company,All",
            3,
            "Technical Support",
            "profile-safety");
    }
}
