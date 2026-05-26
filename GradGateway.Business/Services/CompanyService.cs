using Microsoft.EntityFrameworkCore;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;

namespace GradGateway.Business.Services;

public class CompanyService : ICompanyService
{
    private readonly GradGatewayDbContext _context;

    public CompanyService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfileResponseDto> RegisterOrUpdateCompanyAsync(CompanyRegistrationDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == dto.FirebaseUid);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = dto.FirebaseUid,
                Email = dto.Email,
                Role = UserRole.Company,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else if (user.Role != UserRole.Company)
        {
            throw new InvalidOperationException("User role mismatch. This account is not a company.");
        }

        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
        {
            profile = new CompanyProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyName = dto.CompanyName,
                CompanyEmail = dto.CompanyEmail,
                Phone = dto.Phone,
                Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website,
                Industry = dto.Industry,
                LogoDataUrl = dto.LogoDataUrl,
                RecruiterName = dto.RecruiterName,
                RecruiterEmail = dto.RecruiterEmail,
                RecruiterPhone = dto.RecruiterPhone,
                Position = dto.Position,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CompanyProfiles.Add(profile);
        }
        else
        {
            profile.CompanyName = dto.CompanyName;
            profile.CompanyEmail = dto.CompanyEmail;
            profile.Phone = dto.Phone;
            profile.Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website;
            profile.Industry = dto.Industry;
            profile.LogoDataUrl = dto.LogoDataUrl;
            profile.RecruiterName = dto.RecruiterName;
            profile.RecruiterEmail = dto.RecruiterEmail;
            profile.RecruiterPhone = dto.RecruiterPhone;
            profile.Position = dto.Position;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new CompanyProfileResponseDto(
            user.Email,
            user.FirebaseUid,
            profile.CompanyName,
            profile.CompanyEmail,
            profile.Phone,
            profile.Website,
            profile.Industry,
            profile.LogoDataUrl,
            profile.RecruiterName,
            profile.RecruiterEmail,
            profile.RecruiterPhone,
            profile.Position
        );
    }

    public async Task<CompanyProfileResponseDto?> GetCompanyByFirebaseUidAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null) return null;

        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null) return null;

        return new CompanyProfileResponseDto(
            user.Email,
            user.FirebaseUid,
            profile.CompanyName,
            profile.CompanyEmail,
            profile.Phone,
            profile.Website,
            profile.Industry,
            profile.LogoDataUrl,
            profile.RecruiterName,
            profile.RecruiterEmail,
            profile.RecruiterPhone,
            profile.Position
        );
    }
}
