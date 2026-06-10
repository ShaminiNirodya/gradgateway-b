namespace GradGateway.Data.Entities;

public enum UserRole
{
    Admin,
    Student,
    Company
}

public class User
{
    public Guid Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty; // Link to Firebase
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
