using TradingPlatform.Models;

namespace TradingPlatform.Repositories.Interfaces;

/// <summary>
/// Defines persistence operations for <see cref="Trade"/> documents.
/// </summary>
public interface ITradeRepository
{
    /// <summary>Creates a new trade record in the data store.</summary>
    Task CreateAsync(Trade trade);

    /// <summary>Returns all trades in which the specified user participated.</summary>
    Task<List<Trade>> GetTradesByUserAsync(string userId);
}
