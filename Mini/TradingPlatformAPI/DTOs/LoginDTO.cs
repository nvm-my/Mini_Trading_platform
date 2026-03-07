using System.ComponentModel.DataAnnotations;

namespace TradingPlatform.DTOs
{
    /// <summary>Payload for user login.</summary>
    public class LoginDTO
    {
        /// <summary>Registered e-mail address.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Account password.</summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
