using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GradGateway.Data.Seeding;

public static class PlatformContentSeeder
{
    public static async Task SeedAsync(GradGatewayDbContext context, ILogger? logger = null)
    {
        if (!await context.Database.CanConnectAsync())
        {
            return;
        }

        var seedItems = PlatformContentSeed.GetAll();
        var existingIds = await context.PlatformContents
            .AsNoTracking()
            .Select(c => c.Id)
            .ToListAsync();

        var existingIdSet = existingIds.ToHashSet();
        var toAdd = seedItems.Where(item => !existingIdSet.Contains(item.Id)).ToList();

        if (toAdd.Count == 0)
        {
            return;
        }

        context.PlatformContents.AddRange(toAdd);
        await context.SaveChangesAsync();

        logger?.LogInformation(
            "Seeded {Count} platform content items (FAQs, guides, articles, legal pages).",
            toAdd.Count);
    }
}
