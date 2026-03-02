using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Repositories;

namespace TradingPlatform.Controllers
{
    [ApiController]
    [Route("api/trades")]
    [Authorize]
    public class TradeController : ControllerBase
    {
        private readonly TradeRepository _tradeRepo;

        public TradeController(TradeRepository tradeRepo)
        {
            _tradeRepo = tradeRepo;
        }

        // Get all trades for logged-in user
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTrades()
        {
            var userId = User.FindFirst("nameid")?.Value;

            var trades = await _tradeRepo.GetTradesByUserAsync(userId);
            return Ok(trades);
        }
    }
}