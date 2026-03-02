using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.DTOs;
using TradingPlatform.Services;

namespace TradingPlatform.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // Place new order
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(OrderDTO dto)
        {
            var userId = User.FindFirst("nameid")?.Value;

            var result = await _orderService.PlaceOrder(userId, dto);

            return Ok(result);
        }

        // Cancel order
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(string id)
        {
            await _orderService.CancelOrder(id);
            return Ok("Order cancelled");
        }
    }
}