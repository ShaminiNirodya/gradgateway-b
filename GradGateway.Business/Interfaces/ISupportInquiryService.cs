using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface ISupportInquiryService
{
    Task<SupportInquiryListItemDto> SubmitAsync(SubmitSupportInquiryDto dto);
}
