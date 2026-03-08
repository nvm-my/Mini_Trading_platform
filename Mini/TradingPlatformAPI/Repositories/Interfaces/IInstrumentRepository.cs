using TradingPlatform.Models;

namespace TradingPlatform.Repositories.Interfaces;

/// <summary>
/// Defines persistence operations for <see cref="Instrument"/> documents.
/// </summary>
public interface IInstrumentRepository
{
    /// <summary>Returns all active instruments.</summary>
    Task<List<Instrument>> GetAllAsync();

    /// <summary>Retrieves an instrument by its unique identifier, or <c>null</c> if not found.</summary>
    Task<Instrument?> GetByIdAsync(string id);

    /// <summary>Creates a new instrument in the data store.</summary>
    Task CreateAsync(Instrument instrument);

    /// <summary>Persists changes to an existing instrument document.</summary>
    Task UpdateAsync(Instrument instrument);
}
