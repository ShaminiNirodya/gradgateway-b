namespace GradGateway.Data.Entities;

public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public class StudentSkill
{
    public Guid Id { get; set; }

    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public SkillLevel ProficiencyLevel { get; set; } = SkillLevel.Beginner;
    public int YearsOfExperience { get; set; }
}
