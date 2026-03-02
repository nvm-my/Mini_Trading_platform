using MongoDB.Driver;
using TradingPlatform.Models;

namespace TradingPlatform.Repositories
{
    public class FixMessageRepository
    {
        private readonly IMongoCollection<FixMessage> _collection;

        public FixMessageRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<FixMessage>("FixMessages");
        }

        public async Task CreateAsync(FixMessage message)
        {
            await _collection.InsertOneAsync(message);
        }

        public async Task<List<FixMessage>> GetByTradeIdAsync(string tradeId)
        {
            return await _collection.Find(x => x.TradeId == tradeId).ToListAsync();
        }

        public async Task<List<FixMessage>> GetByOrderIdAsync(string orderId)
        {
            return await _collection.Find(x => x.OrderId == orderId).ToListAsync();
        }
    }
}
