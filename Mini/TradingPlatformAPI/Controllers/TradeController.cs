using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Controllers
{
    /// <summary>
    /// Provides access to executed trade records for the authenticated user.
    /// </summary>
    [ApiController]
    [Route("api/trades")]
    [Authorize]
    public class TradeController : ControllerBase
    {
        private readonly ITradeRepository _tradeRepo;

        public TradeController(ITradeRepository tradeRepo)
        {
            _tradeRepo = tradeRepo;
        }

        /// <summary>Returns all trades in which the authenticated user participated.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTrades()
        {
            var userId = User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var trades = await _tradeRepo.GetTradesByUserAsync(userId);
            return Ok(trades);
        }
    }
}
