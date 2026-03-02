using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Repositories;

namespace TradingPlatform.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // Requires JWT
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepo;

        public UserController(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // Get current logged-in user profile
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst("nameid")?.Value;

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // Admin can view any user
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}