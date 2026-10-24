using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace J7Journal.Models
{
    public class TradeEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public string Direction { get; set; }

        [Required]
        public decimal EntryPrice { get; set; }

        [Required]
        public decimal ExitPrice { get; set; }

        public decimal? ProfitLoss { get; set; }

        public DateTime TradeDate { get; set; }

        public string DailyBias { get; set; }
        public string HTFLiquidity { get; set; }
        public string Confluences { get; set; }
        public string ReasonForEntry { get; set; }
        public string LessonLearned { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public byte[] ScreenshotData { get; set; }
        public string ContentType { get; set; }

        public string SetupType { get; set; }

        // ADD THIS LINE
        public decimal? RiskRewardRatio { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<TradeTag> TradeTags { get; set; }

        // ============ MT5 AUTO-SYNC FIELDS ============
        public long? MT5Ticket { get; set; }
        public decimal? Volume { get; set; }
        public DateTime? MT5OpenTime { get; set; }
        public DateTime? MT5CloseTime { get; set; }
        public bool ImportedFromMT5 { get; set; }
        public bool IsJournalComplete { get; set; }
        // ============ END MT5 AUTO-SYNC FIELDS ============

        public TradeEntry()
        {
            TradeDate = DateTime.Now;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            TradeTags = new List<TradeTag>();
        }
    }
}