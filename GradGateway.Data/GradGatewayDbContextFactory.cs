using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GradGateway.Data;

/// <summary>
/// Enables <c>dotnet ef</c> commands when run from the Data project or with --project GradGateway.Data.
/// </summary>
public class GradGatewayDbContextFactory : IDesignTimeDbContextFactory<GradGatewayDbContext>
{
    public GradGatewayDbContext CreateDbContext(string[] args)
    {
        var apiDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "GradGateway.Api");
        if (!Directory.Exists(apiDir))
            apiDir = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Check GradGateway.Api/appsettings.json.");

        var options = new DbContextOptionsBuilder<GradGatewayDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new GradGatewayDbContext(options);
    }
}
