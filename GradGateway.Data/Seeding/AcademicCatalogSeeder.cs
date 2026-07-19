using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GradGateway.Data.Seeding;

public static class AcademicCatalogSeeder
{
    public static async Task SeedAsync(GradGatewayDbContext context, ILogger? logger = null)
    {
        if (!await context.Database.CanConnectAsync())
        {
            return;
        }

        if (await context.CatalogUniversities.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var degreeNames = AcademicCatalogSeed.UniversityDegrees.Values
            .SelectMany(d => d)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n)
            .ToList();

        var degrees = new Dictionary<string, CatalogDegree>(StringComparer.Ordinal);
        var sort = 0;
        foreach (var name in degreeNames)
        {
            degrees[name] = new CatalogDegree
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsActive = true,
                SortOrder = sort++,
                CreatedAt = now,
                UpdatedAt = now,
            };
        }

        context.CatalogDegrees.AddRange(degrees.Values);

        sort = 0;
        foreach (var (universityName, offeredDegrees) in AcademicCatalogSeed.UniversityDegrees.OrderBy(k => k.Key))
        {
            var university = new CatalogUniversity
            {
                Id = Guid.NewGuid(),
                Name = universityName,
                IsActive = true,
                SortOrder = sort++,
                CreatedAt = now,
                UpdatedAt = now,
            };

            foreach (var degreeName in offeredDegrees)
            {
                if (!degrees.TryGetValue(degreeName, out var degree))
                {
                    continue;
                }

                university.Offerings.Add(new CatalogUniversityDegree
                {
                    UniversityId = university.Id,
                    University = university,
                    DegreeId = degree.Id,
                    Degree = degree,
                    IsActive = true,
                });
            }

            context.CatalogUniversities.Add(university);
        }

        await context.SaveChangesAsync();

        logger?.LogInformation(
            "Seeded academic catalog with {UniversityCount} universities and {DegreeCount} degrees.",
            AcademicCatalogSeed.UniversityDegrees.Count,
            degrees.Count);
    }
}
