using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Models;
using TradingPlatform.Repositories;

namespace TradingPlatform.Controllers
{
    [ApiController]
    [Route("api/instruments")]
    public class InstrumentController : ControllerBase
    {
        private readonly InstrumentRepository _instrumentRepo;

        public InstrumentController(InstrumentRepository instrumentRepo)
        {
            _instrumentRepo = instrumentRepo;
        }

        // Get all active instruments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instruments = await _instrumentRepo.GetAllAsync();
            return Ok(instruments);
        }

        // Admin adds new instrument
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Instrument instrument)
        {
            await _instrumentRepo.CreateAsync(instrument);
            return Ok(instrument);
        }

        // Admin updates price
        [HttpPut("{id}/price")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePrice(string id, decimal newPrice)
        {
            var instrument = await _instrumentRepo.GetByIdAsync(id);
            if (instrument == null)
                return NotFound();

            instrument.CurrentPrice = newPrice;
            await _instrumentRepo.UpdateAsync(instrument);

            return Ok(instrument);
        }
    }
}