namespace GradGateway.Business;

/// <summary>Detects student replies to direct job offers in chat.</summary>
public static class JobOfferResponseMatcher
{
    public const string InterviewAcceptMessage =
        "I am open for an interview. Please share the available dates and times.";

    public const string DeclineMessage =
        "Thank you for the offer. I am not moving forward with this opportunity at this time.";

    public static bool TryParse(string? content, out bool accepted)
    {
        accepted = false;
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var trimmed = content.Trim();

        if (string.Equals(trimmed, InterviewAcceptMessage, StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("open for an interview", StringComparison.OrdinalIgnoreCase))
        {
            accepted = true;
            return true;
        }

        if (string.Equals(trimmed, DeclineMessage, StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("not moving forward with this opportunity", StringComparison.OrdinalIgnoreCase))
        {
            accepted = false;
            return true;
        }

        return false;
    }
}
