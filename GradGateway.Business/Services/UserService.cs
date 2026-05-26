using Microsoft.EntityFrameworkCore;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;

namespace GradGateway.Business.Services;

public class UserService : IUserService
{
    private readonly GradGatewayDbContext _context;
    
    public UserService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponseDto?> GetOrCreateUserAsync(UserRegistrationDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == dto.FirebaseUid);
        
        if (user == null)
        {
            // Parse the role from the DTO
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var userRole))
            {
                throw new ArgumentException($"Invalid role: {dto.Role}");
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = dto.FirebaseUid,
                Email = dto.Email,
                Role = userRole,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return new UserResponseDto(user.Email, user.Role.ToString(), user.FirebaseUid);
    }

    public async Task<UserResponseDto?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        
        if (user == null)
        {
            return null;
        }

        return new UserResponseDto(user.Email, user.Role.ToString(), user.FirebaseUid);
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }
}
