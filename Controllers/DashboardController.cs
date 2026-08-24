using System;
using System.Linq;
using System.Web.Mvc;
using J7Journal.Data;

namespace J7Journal.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var trades = db.TradeEntries
                .OrderByDescending(t => t.TradeDate)
                .Take(10)
                .ToList();

            return View(trades);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}