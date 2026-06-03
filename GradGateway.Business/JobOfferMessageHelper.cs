using System.Text.Json;

namespace GradGateway.Business;

public static class JobOfferMessageHelper
{
    public const string Prefix = "JOB_OFFER::";

    public static bool TryGetApplicationId(string? content, out Guid applicationId)
    {
        applicationId = default;
        if (string.IsNullOrWhiteSpace(content) || !content.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(content[Prefix.Length..]);
            if (doc.RootElement.TryGetProperty("applicationId", out var idEl))
            {
                var raw = idEl.ValueKind == JsonValueKind.String ? idEl.GetString() : idEl.GetRawText();
                if (Guid.TryParse(raw?.Trim('"'), out applicationId))
                    return true;
            }
        }
        catch
        {
            // Ignore malformed offer payloads.
        }

        return false;
    }

    public static bool TryGetPosition(string? content, out string position)
    {
        position = string.Empty;
        if (string.IsNullOrWhiteSpace(content) || !content.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(content[Prefix.Length..]);
            if (doc.RootElement.TryGetProperty("position", out var posEl))
            {
                position = posEl.GetString()?.Trim() ?? string.Empty;
                return !string.IsNullOrEmpty(position);
            }
        }
        catch
        {
            // Ignore malformed offer payloads.
        }

        return false;
    }
}
