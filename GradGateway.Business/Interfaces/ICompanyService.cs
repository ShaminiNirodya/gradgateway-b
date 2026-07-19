using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface ICompanyService
{
    Task<CompanyProfileResponseDto> RegisterOrUpdateCompanyAsync(CompanyRegistrationDto dto);
    Task<CompanyProfileResponseDto?> GetCompanyByFirebaseUidAsync(string firebaseUid);
    Task<CompanyPublicProfileDto?> GetPublicCompanyProfileAsync(Guid companyProfileId);
}
