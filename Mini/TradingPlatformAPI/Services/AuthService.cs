using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TradingPlatform.Models;
using TradingPlatform.Repositories;
using TradingPlatform.Config;

namespace TradingPlatform.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserRepository repo, JwtSettings settings)
        {
            _userRepo = repo;
            _jwtSettings = settings;
        }

        public async Task<string> Register(User user)
        {
            // Simple hash (replace with BCrypt in production)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _userRepo.CreateAsync(user);

            return GenerateToken(user);
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new Exception("Invalid credentials");

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