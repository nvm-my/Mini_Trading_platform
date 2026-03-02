using Microsoft.AspNetCore.Mvc;
using TradingPlatform.DTOs;
using TradingPlatform.Models;
using TradingPlatform.Services;

namespace TradingPlatform.Controllers
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Role = dto.Role
            };

            var token = await _authService.Register(user);
            return Ok(token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var token = await _authService.Login(dto.Email, dto.Password);
            return Ok(token);
        }
    }
}