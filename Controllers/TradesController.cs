using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using J7Journal.Data;
using J7Journal.Models;

namespace J7Journal.Controllers
{
    public class TradesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Create()
        {
            var model = new TradeViewModel
            {
                Trade = new TradeEntry(),
                AvailableTags = db.Tags.ToList(),
                SelectedTagIds = new List<int>()
            };
            return PartialView("_TradeForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TradeViewModel model)
        {
            try
            {
                // FIX: Parse decimals with invariant culture
                var culture = CultureInfo.InvariantCulture;
                decimal entryPrice = 0;
                decimal exitPrice = 0;
                decimal? profitLoss = null;

                // Get raw values from Request
                var entryPriceStr = Request.Form["Trade.EntryPrice"]?.Replace(",", ".") ?? "0";
                var exitPriceStr = Request.Form["Trade.ExitPrice"]?.Replace(",", ".") ?? "0";
                var profitLossStr = Request.Form["Trade.ProfitLoss"]?.Replace(",", ".");

                decimal.TryParse(entryPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out entryPrice);
                decimal.TryParse(exitPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out exitPrice);
                if (!string.IsNullOrEmpty(profitLossStr))
                {
                    decimal pl;
                    if (decimal.TryParse(profitLossStr, NumberStyles.Any, CultureInfo.InvariantCulture, out pl))
                        profitLoss = pl;
                }

                // Set values
                model.Trade.EntryPrice = entryPrice;
                model.Trade.ExitPrice = exitPrice;
                model.Trade.ProfitLoss = profitLoss;

                // Get other form values
                model.Trade.Symbol = Request.Form["Trade.Symbol"];
                model.Trade.Direction = Request.Form["Trade.Direction"];

                // Parse date
                var dateStr = Request.Form["Trade.TradeDate"];
                if (!string.IsNullOrEmpty(dateStr))
                {
                    DateTime tradeDate;
                    if (DateTime.TryParse(dateStr, out tradeDate))
                        model.Trade.TradeDate = tradeDate;
                }

                model.Trade.DailyBias = Request.Form["Trade.DailyBias"];
                model.Trade.SetupType = Request.Form["Trade.SetupType"];
                model.Trade.HTFLiquidity = Request.Form["Trade.HTFLiquidity"];
                model.Trade.Confluences = Request.Form["Trade.Confluences"];
                model.Trade.ReasonForEntry = Request.Form["Trade.ReasonForEntry"];
                model.Trade.LessonLearned = Request.Form["Trade.LessonLearned"];

                // Parse Rating
                var ratingStr = Request.Form["Trade.Rating"];
                if (!string.IsNullOrEmpty(ratingStr))
                {
                    int rating;
                    if (int.TryParse(ratingStr, out rating))
                        model.Trade.Rating = rating;
                }

                // Parse Tags
                var tagIds = Request.Form["SelectedTagIds"];
                if (!string.IsNullOrEmpty(tagIds))
                {
                    var ids = tagIds.Split(',');
                    model.SelectedTagIds = ids.Select(int.Parse).ToList();
                }

                // VALIDATE
                if (string.IsNullOrEmpty(model.Trade.Symbol))
                    return Json(new { success = false, message = "Symbol is required." });
                if (string.IsNullOrEmpty(model.Trade.Direction))
                    return Json(new { success = false, message = "Direction is required." });
                if (model.Trade.EntryPrice <= 0)
                    return Json(new { success = false, message = $"Entry Price must be greater than 0. Received: {model.Trade.EntryPrice}" });
                if (model.Trade.ExitPrice <= 0)
                    return Json(new { success = false, message = $"Exit Price must be greater than 0. Received: {model.Trade.ExitPrice}" });

                // SCREENSHOT
                if (model.ScreenshotFile != null && model.ScreenshotFile.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(model.ScreenshotFile.InputStream))
                    {
                        model.Trade.ScreenshotData = binaryReader.ReadBytes(model.ScreenshotFile.ContentLength);
                        model.Trade.ContentType = model.ScreenshotFile.ContentType;
                    }
                }

                // CALCULATE P&L
                if (!model.Trade.ProfitLoss.HasValue)
                {
                    var priceDiff = model.Trade.Direction == "LONG"
                        ? model.Trade.ExitPrice - model.Trade.EntryPrice
                        : model.Trade.EntryPrice - model.Trade.ExitPrice;
                    model.Trade.ProfitLoss = Math.Round(priceDiff, 2);
                }

                // SKIP R:R - Set to null
                model.Trade.RiskRewardRatio = null;

                model.Trade.CreatedAt = DateTime.Now;
                model.Trade.UpdatedAt = DateTime.Now;

                db.TradeEntries.Add(model.Trade);
                db.SaveChanges();

                // TAGS
                if (model.SelectedTagIds != null && model.SelectedTagIds.Any())
                {
                    foreach (var tagId in model.SelectedTagIds)
                    {
                        db.TradeTags.Add(new TradeTag { TradeId = model.Trade.Id, TagId = tagId });
                    }
                    db.SaveChanges();
                }

                return Json(new { success = true, id = model.Trade.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Edit(int id)
        {
            var trade = db.TradeEntries
                .Include(t => t.TradeTags)
                .FirstOrDefault(t => t.Id == id);

            if (trade == null)
                return HttpNotFound();

            var model = new TradeViewModel
            {
                Trade = trade,
                AvailableTags = db.Tags.ToList(),
                SelectedTagIds = db.TradeTags.Where(tt => tt.TradeId == id).Select(tt => tt.TagId).ToList()
            };

            return PartialView("_TradeForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TradeViewModel model)
        {
            try
            {
                var culture = CultureInfo.InvariantCulture;

                var trade = db.TradeEntries.Find(model.Trade.Id);
                if (trade == null)
                    return Json(new { success = false, message = "Trade not found" });

                // Parse decimals with invariant culture
                var entryPriceStr = Request.Form["Trade.EntryPrice"]?.Replace(",", ".") ?? "0";
                var exitPriceStr = Request.Form["Trade.ExitPrice"]?.Replace(",", ".") ?? "0";
                var profitLossStr = Request.Form["Trade.ProfitLoss"]?.Replace(",", ".");

                decimal entryPrice = 0;
                decimal exitPrice = 0;
                decimal? profitLoss = null;

                decimal.TryParse(entryPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out entryPrice);
                decimal.TryParse(exitPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out exitPrice);
                if (!string.IsNullOrEmpty(profitLossStr))
                {
                    decimal pl;
                    if (decimal.TryParse(profitLossStr, NumberStyles.Any, CultureInfo.InvariantCulture, out pl))
                        profitLoss = pl;
                }

                trade.Symbol = Request.Form["Trade.Symbol"];
                trade.Direction = Request.Form["Trade.Direction"];
                trade.EntryPrice = entryPrice;
                trade.ExitPrice = exitPrice;
                trade.ProfitLoss = profitLoss;

                var dateStr = Request.Form["Trade.TradeDate"];
                if (!string.IsNullOrEmpty(dateStr))
                {
                    DateTime tradeDate;
                    if (DateTime.TryParse(dateStr, out tradeDate))
                        trade.TradeDate = tradeDate;
                }

                trade.DailyBias = Request.Form["Trade.DailyBias"];
                trade.HTFLiquidity = Request.Form["Trade.HTFLiquidity"];
                trade.Confluences = Request.Form["Trade.Confluences"];
                trade.ReasonForEntry = Request.Form["Trade.ReasonForEntry"];
                trade.LessonLearned = Request.Form["Trade.LessonLearned"];
                trade.SetupType = Request.Form["Trade.SetupType"];

                var ratingStr = Request.Form["Trade.Rating"];
                if (!string.IsNullOrEmpty(ratingStr))
                {
                    int rating;
                    if (int.TryParse(ratingStr, out rating))
                        trade.Rating = rating;
                }

                trade.UpdatedAt = DateTime.Now;

                if (!trade.ProfitLoss.HasValue)
                {
                    var priceDiff = trade.Direction == "LONG"
                        ? trade.ExitPrice - trade.EntryPrice
                        : trade.EntryPrice - trade.ExitPrice;
                    trade.ProfitLoss = Math.Round(priceDiff, 2);
                }

                // SKIP R:R - Set to null
                trade.RiskRewardRatio = null;

                if (model.ScreenshotFile != null && model.ScreenshotFile.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(model.ScreenshotFile.InputStream))
                    {
                        trade.ScreenshotData = binaryReader.ReadBytes(model.ScreenshotFile.ContentLength);
                        trade.ContentType = model.ScreenshotFile.ContentType;
                    }
                }

                var existingTags = db.TradeTags.Where(tt => tt.TradeId == trade.Id).ToList();
                db.TradeTags.RemoveRange(existingTags);

                var tagIds = Request.Form["SelectedTagIds"];
                if (!string.IsNullOrEmpty(tagIds))
                {
                    var ids = tagIds.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
                    foreach (var tagId in ids)
                    {
                        db.TradeTags.Add(new TradeTag { TradeId = trade.Id, TagId = tagId });
                    }
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var trade = db.TradeEntries.Find(id);
                if (trade == null)
                    return Json(new { success = false, message = "Trade not found" });

                var tradeTags = db.TradeTags.Where(tt => tt.TradeId == id).ToList();
                if (tradeTags.Any())
                    db.TradeTags.RemoveRange(tradeTags);

                db.TradeEntries.Remove(trade);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Details(int id)
        {
            var trade = db.TradeEntries
                .Include(t => t.TradeTags.Select(tt => tt.Tag))
                .FirstOrDefault(t => t.Id == id);

            if (trade == null)
                return HttpNotFound();

            var model = new TradeViewModel
            {
                Trade = trade,
                SelectedTagIds = trade.TradeTags.Select(tt => tt.TagId).ToList()
            };

            return PartialView("_TradeModal", model);
        }

        public ActionResult GetImage(int id)
        {
            try
            {
                var trade = db.TradeEntries.Find(id);
                if (trade == null || trade.ScreenshotData == null || trade.ScreenshotData.Length == 0)
                    return File("~/Content/images/no-image.png", "image/png");

                return File(trade.ScreenshotData, trade.ContentType ?? "image/png");
            }
            catch
            {
                return File("~/Content/images/no-image.png", "image/png");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}