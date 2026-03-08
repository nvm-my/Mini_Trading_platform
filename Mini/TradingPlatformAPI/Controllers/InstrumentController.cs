using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Controllers
{
    /// <summary>
    /// Manages tradeable instruments. Listing is public; create and price-update require an Admin role.
    /// </summary>
    [ApiController]
    [Route("api/instruments")]
    public class InstrumentController : ControllerBase
    {
        private readonly IInstrumentRepository _instrumentRepo;

        public InstrumentController(IInstrumentRepository instrumentRepo)
        {
            _instrumentRepo = instrumentRepo;
        }

        /// <summary>Returns all instruments.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instruments = await _instrumentRepo.GetAllAsync();
            return Ok(instruments);
        }

        /// <summary>Creates a new instrument. Requires the <c>Admin</c> role.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Instrument instrument)
        {
            await _instrumentRepo.CreateAsync(instrument);
            return Ok(instrument);
        }

        /// <summary>Updates the current price of an instrument. Requires the <c>Admin</c> role.</summary>
        /// <param name="id">Instrument identifier.</param>
        /// <param name="newPrice">New price (must be positive).</param>
        [HttpPut("{id}/price")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePrice(string id, [FromQuery] decimal newPrice)
        {
            if (newPrice <= 0)
                return BadRequest("Price must be a positive value.");

            var instrument = await _instrumentRepo.GetByIdAsync(id);
            if (instrument == null)
                return NotFound();

            instrument.CurrentPrice = newPrice;
            await _instrumentRepo.UpdateAsync(instrument);

            return Ok(instrument);
        }
    }
}
