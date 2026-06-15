using GradGateway.Data.Entities;

namespace GradGateway.Data.Seeding;

/// <summary>Default legal pages (privacy, terms, cookies).</summary>
public static class LegalContentSeed
{
    private static readonly DateTime SeedAt = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private const string LastUpdated = "<p><em>Last updated: May 2026</em></p>";

    public static IEnumerable<PlatformContent> GetAll()
    {
        yield return Legal(
            Guid.Parse("c4010001-0001-0001-0001-000000000001"),
            "privacy-policy",
            "Privacy Policy",
            PrivacyPolicyBody,
            1);

        yield return Legal(
            Guid.Parse("c4010001-0001-0001-0001-000000000002"),
            "terms-of-service",
            "Terms of Service",
            TermsOfServiceBody,
            2);

        yield return Legal(
            Guid.Parse("c4010001-0001-0001-0001-000000000003"),
            "cookies",
            "Cookie Policy",
            CookiePolicyBody,
            3);
    }

    private static PlatformContent Legal(Guid id, string slug, string title, string body, int sortOrder) =>
        new()
        {
            Id = id,
            ContentType = "Legal",
            Section = "Legal",
            Title = title,
            Body = body.Trim(),
            Summary = null,
            StepsJson = null,
            Audiences = "All",
            Category = null,
            Slug = slug,
            RelatedLinkHref = null,
            RelatedLinkLabel = null,
            Status = "Published",
            SortOrder = sortOrder,
            CreatedAt = SeedAt,
            UpdatedAt = SeedAt,
        };

    private static readonly string PrivacyPolicyBody = """
<section><h2>1. Introduction</h2>
<p>GradGateway ("we," "us," "our," or "Company") is committed to protecting your privacy. This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you visit our website and use our services.</p></section>
<section><h2>2. Information We Collect</h2>
<p>We may collect information about you in a variety of ways. The information we may collect on the site includes:</p>
<ul>
<li><strong>Personal Data:</strong> Name, email address, phone number, profile information, and other details you provide when registering or using our services</li>
<li><strong>Educational Information:</strong> Academic records, skills, projects, and portfolio information</li>
<li><strong>Professional Information:</strong> Resume, work experience, and company details</li>
<li><strong>Technical Data:</strong> IP address, browser type, operating system, and usage data collected through cookies and similar technologies</li>
</ul></section>
<section><h2>3. Use of Your Information</h2>
<p>Having accurate information about you permits us to provide you with a smooth, efficient, and customized experience. Specifically, we may use information collected about you via the site to:</p>
<ul>
<li>Create and manage your user account</li>
<li>Process your transactions and send related information</li>
<li>Generate analytics to improve our services</li>
<li>Send promotional communications (with your consent)</li>
<li>Respond to your inquiries and support requests</li>
<li>Connect students with recruitment opportunities</li>
</ul></section>
<section><h2>4. Disclosure of Your Information</h2>
<p>We may share your information in the following situations:</p>
<ul>
<li><strong>By Law or to Protect Rights:</strong> When required by law or to protect our rights, privacy, safety, or property</li>
<li><strong>Third-Party Service Providers:</strong> We may share your information with vendors, consultants, and other service providers who need access to such information to carry out work</li>
<li><strong>Business Transfers:</strong> Your information may be transferred as part of a merger, sale, or acquisition</li>
<li><strong>Recruitment Matching:</strong> Your profile information may be shared with companies for recruitment purposes (only with your consent)</li>
</ul></section>
<section><h2>5. Security of Your Information</h2>
<p>We use administrative, technical, and physical security measures to protect your personal information. However, no method of transmission over the Internet or electronic storage is 100% secure. While we strive to use commercially acceptable means to protect your personal information, we cannot guarantee its absolute security.</p></section>
<section><h2>6. Contact Us</h2>
<p>If you have questions or comments about this Privacy Policy, please contact us at:</p>
<p><strong>GradGateway</strong><br/>Email: privacy@gradgateway.lk</p></section>
<section><h2>7. Changes to This Privacy Policy</h2>
<p>We reserve the right to modify this privacy policy at any time. Changes and clarifications will take effect immediately upon their posting to the website. If we make material changes to this policy, we will notify you here that it has been updated.</p></section>
""" + LastUpdated;

    private static readonly string TermsOfServiceBody = """
<section><h2>1. Agreement to Terms</h2>
<p>By accessing and using the GradGateway website and services, you accept and agree to be bound by and comply with these Terms of Service. If you do not agree to abide by the above, please do not use this service.</p></section>
<section><h2>2. Use License</h2>
<p>Permission is granted to temporarily download one copy of the materials (information or software) on GradGateway for personal, non-commercial transitory viewing only. This is the grant of a license, not a transfer of title, and under this license you may not:</p>
<ul>
<li>Modify or copy the materials</li>
<li>Use the materials for any commercial purpose or for any public display</li>
<li>Attempt to decompile or reverse engineer any software contained on GradGateway</li>
<li>Remove any copyright or other proprietary notations from the materials</li>
<li>Transfer the materials to another person or "mirror" the materials on any other server</li>
<li>Violate any applicable laws or regulations related to the access to or use of GradGateway</li>
</ul></section>
<section><h2>3. Disclaimer</h2>
<p>The materials on GradGateway are provided on an 'as is' basis. GradGateway makes no warranties, expressed or implied, and hereby disclaims and negates all other warranties including, without limitation, implied warranties or conditions of merchantability, fitness for a particular purpose, or non-infringement of intellectual property or other violation of rights.</p></section>
<section><h2>4. Limitations</h2>
<p>In no event shall GradGateway or its suppliers be liable for any damages (including, without limitation, damages for loss of data or profit, or due to business interruption) arising out of the use or inability to use the materials on GradGateway, even if GradGateway or an authorized representative has been notified orally or in writing of the possibility of such damage.</p></section>
<section><h2>5. Accuracy of Materials</h2>
<p>The materials appearing on GradGateway could include technical, typographical, or photographic errors. GradGateway does not warrant that any of the materials on the website are accurate, complete, or current. GradGateway may make changes to the materials contained on the website at any time without notice.</p></section>
<section><h2>6. Links</h2>
<p>GradGateway has not reviewed all of the sites linked to its website and is not responsible for the contents of any such linked site. The inclusion of any link does not imply endorsement by GradGateway of the site. Use of any such linked website is at the user's own risk.</p></section>
<section><h2>7. Modifications</h2>
<p>GradGateway may revise these terms of service for the website at any time without notice. By using this website, you are agreeing to be bound by the then current version of these terms of service.</p></section>
<section><h2>8. Governing Law</h2>
<p>These terms and conditions are governed by and construed in accordance with the laws of Sri Lanka, and you irrevocably submit to the exclusive jurisdiction of the courts in that location.</p></section>
<section><h2>9. User Responsibilities</h2>
<p>As a user of GradGateway, you agree to:</p>
<ul>
<li>Provide accurate and complete information during registration</li>
<li>Maintain the confidentiality of your account password</li>
<li>Accept responsibility for all activities that occur under your account</li>
<li>Not engage in any conduct that restricts or inhibits anyone's use or enjoyment of the website</li>
<li>Not post or transmit obscene, abusive, hateful, or discriminatory content</li>
<li>Not violate any laws or third-party rights</li>
</ul></section>
<section><h2>10. Termination</h2>
<p>GradGateway reserves the right to terminate your account and access to the website at any time, without notice, for conduct that we believe violates these Terms of Service or is harmful to other users, our business, or third parties.</p></section>
<section><h2>11. Contact Information</h2>
<p>If you have any questions about these Terms of Service, please contact us at:</p>
<p><strong>GradGateway</strong><br/>Email: support@gradgateway.lk</p></section>
""" + LastUpdated;

    private static readonly string CookiePolicyBody = """
<section><h2>1. What are cookies?</h2>
<p>Cookies are small text files stored on your device when you visit a website. They help the site remember your preferences and keep you signed in between pages.</p></section>
<section><h2>2. How GradGateway uses cookies</h2>
<p>We keep our use of cookies and similar storage minimal and functional:</p>
<ul>
<li><strong>Authentication:</strong> Firebase Authentication stores tokens in your browser to keep you signed in securely. A lightweight role cookie helps route you to the correct dashboard.</li>
<li><strong>Session preferences:</strong> We use browser session storage to remember your profile data during a visit, reducing repeated network requests.</li>
<li><strong>No advertising cookies:</strong> We do not use third-party advertising or cross-site tracking cookies.</li>
</ul></section>
<section><h2>3. Managing cookies</h2>
<p>You can clear or block cookies through your browser settings at any time. Note that blocking authentication cookies will prevent you from staying signed in to GradGateway.</p></section>
<section><h2>4. Changes to this policy</h2>
<p>We may update this Cookie Policy as the platform evolves. Material changes will be reflected on this page.</p></section>
<section><h2>5. Contact Us</h2>
<p>If you have questions about this Cookie Policy, please contact us at:</p>
<p><strong>GradGateway</strong><br/>Email: privacy@gradgateway.lk</p></section>
""" + LastUpdated;
}
