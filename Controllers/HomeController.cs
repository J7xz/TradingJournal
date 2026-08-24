using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using J7Journal.Data;

namespace J7Journal.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var trades = db.TradeEntries
                .Include(t => t.TradeTags.Select(tt => tt.Tag))
                .OrderByDescending(t => t.TradeDate)
                .Take(20)
                .ToList();

            var allTrades = db.TradeEntries.ToList();
            var winningTrades = allTrades.Where(t => t.ProfitLoss > 0).ToList();

            // Make sure these are always set with default values
            ViewBag.TotalTrades = allTrades.Count;
            ViewBag.WinRate = allTrades.Any() ?
                Math.Round((decimal)winningTrades.Count / allTrades.Count * 100, 1) : 0m;
            ViewBag.TotalPnL = allTrades.Any() ? allTrades.Sum(t => t.ProfitLoss ?? 0) : 0m;
            ViewBag.AvgPnL = allTrades.Any() ?
                Math.Round(allTrades.Average(t => t.ProfitLoss ?? 0), 2) : 0m;

            return View(trades);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}