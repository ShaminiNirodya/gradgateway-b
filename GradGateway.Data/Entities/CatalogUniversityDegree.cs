namespace GradGateway.Data.Entities;

public class CatalogUniversityDegree
{
    public Guid UniversityId { get; set; }
    public CatalogUniversity University { get; set; } = null!;

    public Guid DegreeId { get; set; }
    public CatalogDegree Degree { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
