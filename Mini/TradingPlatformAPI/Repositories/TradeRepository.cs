using MongoDB.Driver;
using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Repositories
{
    /// <inheritdoc cref="ITradeRepository"/>
    public class TradeRepository : ITradeRepository
    {
        private readonly IMongoCollection<Trade> _collection;

        public TradeRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Trade>("Trades");
        }

        public async Task CreateAsync(Trade trade)
        {
            await _collection.InsertOneAsync(trade);
        }

        public async Task<List<Trade>> GetTradesByUserAsync(string userId)
        {
            return await _collection.Find(t =>
                t.BuyOrderId == userId ||
                t.SellOrderId == userId)
                .ToListAsync();
        }
    }
}