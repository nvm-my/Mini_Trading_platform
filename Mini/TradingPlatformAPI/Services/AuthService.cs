using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TradingPlatform.Config;
using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Services
{
    /// <summary>
    /// Handles user registration and login, including JWT token generation.
    /// </summary>
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository repo, JwtSettings settings)
        {
            _userRepo = repo;
            _jwtSettings = settings;
        }

        /// <summary>
        /// Registers a new user by hashing the password and persisting the user document,
        /// then returns a signed JWT token.
        /// </summary>
        public async Task<string> Register(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _userRepo.CreateAsync(user);
            return GenerateToken(user);
        }

        /// <summary>
        /// Validates credentials and returns a signed JWT token on success.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
        public async Task<string> Login(string email, string password)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
