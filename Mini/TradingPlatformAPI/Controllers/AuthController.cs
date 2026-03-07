using Microsoft.AspNetCore.Mvc;
using TradingPlatform.DTOs;
using TradingPlatform.Models;
using TradingPlatform.Services;

namespace TradingPlatform.Controllers
{
    /// <summary>
    /// Handles user registration and authentication, returning JWT tokens on success.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account and returns a JWT token.
        /// </summary>
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
            return Ok(new { token });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token on success.
        /// Returns <c>401 Unauthorized</c> when credentials are invalid.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var token = await _authService.Login(dto.Email, dto.Password);
            return Ok(new { token });
        }
    }
}
