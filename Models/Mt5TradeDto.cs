using System;

namespace J7Journal.Models
{
    public class Mt5TradeDto
    {
        public long Ticket { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }   // "Buy" or "Sell"
        public decimal Volume { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal Profit { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
    }
}