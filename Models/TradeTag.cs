using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace J7Journal.Models
{
    public class TradeTag
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("TradeEntry")]
        public int TradeId { get; set; }

        [ForeignKey("Tag")]
        public int TagId { get; set; }

      public virtual TradeEntry TradeEntry { get; set; }
        public virtual Tag Tag { get; set; }
    }
}