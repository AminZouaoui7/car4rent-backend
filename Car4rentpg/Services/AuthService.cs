using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AdminLoginResponseDto?> AdminLoginAsync(AdminLoginDto dto, string? ipAddress)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return null;
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            var admin = await _context.AdminUsers
                .Include(a => a.RefreshTokens)
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email);

            if (admin == null)
                return null;

            if (!admin.IsActive)
                return null;

            if (admin.LockoutEndUtc.HasValue && admin.LockoutEndUtc > DateTime.UtcNow)
                return null;

            var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password);

            if (!validPassword)
            {
                admin.FailedLoginAttempts++;

                if (admin.FailedLoginAttempts >= 5)
                {
                    admin.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
                    admin.FailedLoginAttempts = 0;
                }

                await _context.SaveChangesAsync();
                return null;
            }

            admin.FailedLoginAttempts = 0;
            admin.LockoutEndUtc = null;
            admin.LastLoginAtUtc = DateTime.UtcNow;
            admin.LastLoginIp = ipAddress;

            var accessToken = _jwtService.GenerateAdminAccessToken(admin);
            var refreshTokenValue = _jwtService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                AdminUserId = admin.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AdminLoginResponseDto
            {
                Message = "Admin login successful.",
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<AdminLoginResponseDto?> RefreshTokenAsync(string refreshTokenValue)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenValue))
                return null;

            var refreshToken = await _context.RefreshTokens
                .Include(r => r.AdminUser)
                .FirstOrDefaultAsync(r => r.Token == refreshTokenValue);

            if (refreshToken == null || !refreshToken.IsActive || !refreshToken.AdminUser.IsActive)
                return null;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAtUtc = DateTime.UtcNow;

            var newRefreshTokenValue = _jwtService.GenerateRefreshToken();
            refreshToken.ReplacedByToken = newRefreshTokenValue;

            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenValue,
                AdminUserId = refreshToken.AdminUserId,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(newRefreshToken);

            var newAccessToken = _jwtService.GenerateAdminAccessToken(refreshToken.AdminUser);

            await _context.SaveChangesAsync();

            return new AdminLoginResponseDto
            {
                Message = "Token refreshed successfully.",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshTokenValue)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshTokenValue);

            if (refreshToken == null || refreshToken.IsRevoked)
                return false;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}