using TradingPlatform.Models;

namespace TradingPlatform.Repositories.Interfaces;

/// <summary>
/// Defines persistence operations for <see cref="FixMessage"/> documents.
/// </summary>
public interface IFixMessageRepository
{
    /// <summary>Creates a new FIX message record in the data store.</summary>
    Task CreateAsync(FixMessage message);

    /// <summary>Returns all FIX messages associated with the given trade identifier.</summary>
    Task<List<FixMessage>> GetByTradeIdAsync(string tradeId);

    /// <summary>Returns all FIX messages associated with the given order identifier.</summary>
    Task<List<FixMessage>> GetByOrderIdAsync(string orderId);
}
