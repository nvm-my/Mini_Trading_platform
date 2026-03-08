using System.ComponentModel.DataAnnotations;

namespace TradingPlatform.DTOs
{
    /// <summary>Payload for placing a new order.</summary>
    public class OrderDTO
    {
        /// <summary>MongoDB ObjectId of the instrument to trade.</summary>
        [Required]
        public string InstrumentId { get; set; } = string.Empty;

        /// <summary>Order direction. Allowed values: <c>BUY</c>, <c>SELL</c>.</summary>
        [Required]
        [RegularExpression("^(BUY|SELL)$", ErrorMessage = "Side must be 'BUY' or 'SELL'.")]
        public string Side { get; set; } = string.Empty;

        /// <summary>Order type. Allowed values: <c>MARKET</c>, <c>LIMIT</c>.</summary>
        [Required]
        [RegularExpression("^(MARKET|LIMIT)$", ErrorMessage = "OrderType must be 'MARKET' or 'LIMIT'.")]
        public string OrderType { get; set; } = string.Empty;

        /// <summary>Limit price; required when <see cref="OrderType"/> is <c>LIMIT</c>.</summary>
        [Range(0.0001, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal? Price { get; set; }

        /// <summary>Number of units to trade. Must be at least 1.</summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
