using System.ComponentModel.DataAnnotations;

namespace TradingPlatform.DTOs
{
    /// <summary>Payload for user registration.</summary>
    public class RegisterDTO
    {
        /// <summary>Display name of the user.</summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Unique e-mail address used for login.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Plain-text password; hashed before storage.</summary>
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Role assigned to the user. Allowed values: <c>Admin</c>, <c>Client</c>.</summary>
        [Required]
        [RegularExpression("^(Admin|Client)$", ErrorMessage = "Role must be 'Admin' or 'Client'.")]
        public string Role { get; set; } = string.Empty;
    }
}
