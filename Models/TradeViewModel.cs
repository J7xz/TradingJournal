using System.Collections.Generic;
using System.Web;

namespace J7Journal.Models
{
    public class TradeViewModel
    {
        public TradeEntry Trade { get; set; }
        public List<Tag> AvailableTags { get; set; }
        public List<int> SelectedTagIds { get; set; }
        public HttpPostedFileBase ScreenshotFile { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? PriceChange { get; set; }
    }
}