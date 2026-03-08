using TradingPlatform.Models;

namespace TradingPlatform.Repositories.Interfaces;

/// <summary>
/// Defines persistence operations for <see cref="User"/> documents.
/// </summary>
public interface IUserRepository
{
    /// <summary>Creates a new user in the data store.</summary>
    Task CreateAsync(User user);

    /// <summary>Retrieves a user by their e-mail address, or <c>null</c> if not found.</summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Retrieves a user by their unique identifier, or <c>null</c> if not found.</summary>
    Task<User?> GetByIdAsync(string id);

    /// <summary>Persists changes to an existing user document.</summary>
    Task UpdateAsync(User user);
}
