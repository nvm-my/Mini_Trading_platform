using TradingPlatform.Models;

namespace TradingPlatform.Repositories.Interfaces;

/// <summary>
/// Defines persistence operations for <see cref="Order"/> documents.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Creates a new order in the data store.</summary>
    Task CreateAsync(Order order);

    /// <summary>Retrieves an order by its unique identifier, or <c>null</c> if not found.</summary>
    Task<Order?> GetByIdAsync(string id);

    /// <summary>Persists changes to an existing order document.</summary>
    Task UpdateAsync(Order order);

    /// <summary>
    /// Returns all open orders for a given instrument and side (BUY or SELL).
    /// </summary>
    Task<List<Order>> GetOpenOrdersByInstrumentAndSide(string instrumentId, string side);
}
