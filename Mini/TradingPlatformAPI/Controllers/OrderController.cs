using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.DTOs;
using TradingPlatform.Services;

namespace TradingPlatform.Controllers
{
    /// <summary>
    /// Manages order placement and cancellation. All endpoints require authentication.
    /// </summary>
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

        /// <summary>
        /// Places a new order for the authenticated user and immediately triggers matching.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(OrderDTO dto)
        {
            var userId = User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _orderService.PlaceOrder(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// Cancels an existing open order.
        /// Returns <c>404 Not Found</c> when the order does not exist.
        /// </summary>
        /// <param name="id">Order identifier.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(string id)
        {
            await _orderService.CancelOrder(id);
            return Ok(new { message = "Order cancelled." });
        }
    }
}
