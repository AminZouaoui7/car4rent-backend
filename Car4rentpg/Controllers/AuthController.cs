using Car4rentpg.DTOs;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [EnableRateLimiting("admin-login")]
        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _authService.AdminLoginAsync(dto, ipAddress);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid admin email or password."
                });
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid or expired refresh token."
                });
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var revoked = await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);

            if (!revoked)
            {
                return BadRequest(new
                {
                    message = "Refresh token not found or already revoked."
                });
            }

            return Ok(new
            {
                message = "Logged out successfully."
            });
        }
    }
}