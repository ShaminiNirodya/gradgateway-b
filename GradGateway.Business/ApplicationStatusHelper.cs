using GradGateway.Data.Entities;

namespace GradGateway.Business;

public static class ApplicationStatusHelper
{
    public static bool TryParse(string? input, out ApplicationStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var value = input.Trim();
        if (Enum.TryParse<ApplicationStatus>(value, ignoreCase: true, out status))
            return true;

        switch (value.ToLowerInvariant())
        {
            case "new":
            case "new applied":
            case "applied":
            case "submitted":
                status = ApplicationStatus.Pending;
                return true;
            case "shortlisted":
            case "shortlist":
                status = ApplicationStatus.Shortlisted;
                return true;
            case "interviewed":
            case "interview":
                status = ApplicationStatus.Interviewed;
                return true;
            case "offers":
            case "offer":
            case "offer sent":
            case "offer received":
                status = ApplicationStatus.OfferSent;
                return true;
            case "offeraccepted":
            case "offer accepted":
            case "accepted":
                status = ApplicationStatus.OfferAccepted;
                return true;
            case "hired":
                status = ApplicationStatus.Hired;
                return true;
            case "rejected":
            case "reject":
                status = ApplicationStatus.Rejected;
                return true;
            default:
                return false;
        }
    }

    public static string ToApiString(ApplicationStatus status) => status.ToString();
}
