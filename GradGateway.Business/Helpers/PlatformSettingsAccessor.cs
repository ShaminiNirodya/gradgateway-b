using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Helpers;

public static class PlatformSettingsAccessor
{
    public static readonly Guid SingletonId = Guid.Parse("f0f0f0f0-1111-2222-3333-444444444444");

    public static async Task<PlatformSettings> GetOrCreateAsync(GradGatewayDbContext context)
    {
        var row = await context.PlatformSettings.FirstOrDefaultAsync(s => s.Id == SingletonId);
        if (row != null)
        {
            return row;
        }

        row = new PlatformSettings
        {
            Id = SingletonId,
            AllowRegistration = true,
            MaintenanceMode = false,
            UpdatedAt = DateTime.UtcNow
        };
        context.PlatformSettings.Add(row);
        await context.SaveChangesAsync();
        return row;
    }
}
