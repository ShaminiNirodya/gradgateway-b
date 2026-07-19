namespace GradGateway.Business.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string FromAddress { get; set; } = "noreply@gradgateway.lk";
    public string FromName { get; set; } = "GradGateway";
    public bool Enabled { get; set; }
}
