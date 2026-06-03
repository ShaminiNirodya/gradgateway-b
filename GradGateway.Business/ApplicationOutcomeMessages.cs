using GradGateway.Data.Entities;

namespace GradGateway.Business;

public static class ApplicationOutcomeMessages
{
    /// <summary>Send a thoughtful rejection note after the student has been interviewed or accepted an offer.</summary>
    public static bool ShouldSendRejectionNotice(ApplicationStatus previousStatus) =>
        previousStatus is ApplicationStatus.Interviewed or ApplicationStatus.OfferAccepted;

    public static string BuildHiredMessage(string studentName, string jobTitle, string companyName)
    {
        var firstName = studentName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? studentName;
        return
            $"Congratulations, {firstName}! 🎉\n\n" +
            $"{companyName} is delighted to offer you the role of {jobTitle}. " +
            "Your hard work and interview performance made a strong impression on the team.\n\n" +
            "Please check your inbox for any next steps from the company. We are excited to see you take this next step in your career!";
    }

    public static string BuildRejectedMessage(string studentName, string jobTitle, string companyName)
    {
        var firstName = studentName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? studentName;
        return
            $"Hi {firstName},\n\n" +
            $"Thank you for interviewing for {jobTitle} with {companyName}. " +
            "After careful consideration, the team has decided to move forward with other candidates for this role.\n\n" +
            "We genuinely appreciate the time and effort you invested in the process. " +
            "Please stay encouraged — the right opportunity is out there, and we wish you every success in your job search.";
    }
}
