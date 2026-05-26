using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class EmailLogService : IEmailLogService
{
    private readonly GradGatewayDbContext _context;

    public EmailLogService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<EmailLogResponseDto> TrackAsync(string firebaseUid, EmailLogTrackRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        var status = string.IsNullOrWhiteSpace(dto.Status) ? "Simulated" : dto.Status.Trim();

        var log = new EmailLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ToEmail = dto.ToEmail.Trim(),
            TemplateType = dto.TemplateType.Trim(),
            Purpose = dto.Purpose.Trim(),
            Provider = string.IsNullOrWhiteSpace(dto.Provider) ? "FirebaseAuth" : dto.Provider.Trim(),
            Status = status,
            ProviderMessageId = string.IsNullOrWhiteSpace(dto.ProviderMessageId) ? null : dto.ProviderMessageId.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(dto.PayloadJson) ? null : dto.PayloadJson,
            Error = string.IsNullOrWhiteSpace(dto.Error) ? null : dto.Error,
            CreatedAt = DateTime.UtcNow,
            SentAt = dto.SentAt,
        };

        _context.Set<EmailLog>().Add(log);
        await _context.SaveChangesAsync();

        return new EmailLogResponseDto(
            log.Id,
            log.ToEmail,
            log.TemplateType,
            log.Purpose,
            log.Provider,
            log.Status,
            log.ProviderMessageId,
            log.PayloadJson,
            log.Error,
            log.CreatedAt,
            log.SentAt
        );
    }

    public async Task<List<EmailLogResponseDto>> GetMyLogsAsync(string firebaseUid, int take = 100)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        var rows = await _context.Set<EmailLog>()
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync();

        return rows.Select(x => new EmailLogResponseDto(
            x.Id,
            x.ToEmail,
            x.TemplateType,
            x.Purpose,
            x.Provider,
            x.Status,
            x.ProviderMessageId,
            x.PayloadJson,
            x.Error,
            x.CreatedAt,
            x.SentAt
        )).ToList();
    }
}
