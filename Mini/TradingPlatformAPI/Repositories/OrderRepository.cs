using MongoDB.Driver;
using TradingPlatform.Models;

namespace TradingPlatform.Repositories
{
    public class OrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(IMongoDatabase database)
        {
            _orders = database.GetCollection<Order>("Orders");
        }

        public async Task CreateAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
        }

        public async Task<Order> GetByIdAsync(string id)
        {
            return await _orders.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            await _orders.ReplaceOneAsync(x => x.Id == order.Id, order);
        }

        public async Task<List<Order>> GetOpenOrdersByInstrumentAndSide(
            string instrumentId,
            string side)
        {
            return await _orders.Find(x =>
                x.InstrumentId == instrumentId &&
                x.Side == side &&
                x.Status == "OPEN")
                .ToListAsync();
        }
    }
}