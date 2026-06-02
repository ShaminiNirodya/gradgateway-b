using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IPlatformStatsService
{
    Task<PlatformStatsDto> GetPlatformStatsAsync();
}
