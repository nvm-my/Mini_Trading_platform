using MongoDB.Driver;
using TradingPlatform.Models;

namespace TradingPlatform.Repositories
{
    public class TradeRepository
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