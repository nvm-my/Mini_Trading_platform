using MongoDB.Driver;
using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Repositories
{
    /// <inheritdoc cref="IInstrumentRepository"/>
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly IMongoCollection<Instrument> _collection;

        public InstrumentRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Instrument>("Instruments");
        }

        public async Task<List<Instrument>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Instrument?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Instrument instrument)
        {
            await _collection.InsertOneAsync(instrument);
        }

        public async Task UpdateAsync(Instrument instrument)
        {
            await _collection.ReplaceOneAsync(x => x.Id == instrument.Id, instrument);
        }
    }
}