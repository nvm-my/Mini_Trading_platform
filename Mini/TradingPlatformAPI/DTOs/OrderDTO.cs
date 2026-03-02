namespace TradingPlatform.DTOs
{
    public class OrderDTO
    {
        public string InstrumentId { get; set; }
        public string Side { get; set; }
        public string OrderType { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
    }
}