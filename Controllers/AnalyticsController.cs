using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using J7Journal.Data;
using J7Journal.Models;

namespace J7Journal.Controllers
{
    public class AnalyticsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string timeframe = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = db.TradeEntries.AsQueryable();
            DateTime filterStartDate = DateTime.Now.AddDays(-30);
            DateTime filterEndDate = DateTime.Now;

            switch (timeframe)
            {
                case "7days":
                    filterStartDate = DateTime.Now.AddDays(-7);
                    break;
                case "30days":
                    filterStartDate = DateTime.Now.AddDays(-30);
                    break;
                case "custom":
                    if (startDate.HasValue) filterStartDate = startDate.Value;
                    if (endDate.HasValue) filterEndDate = endDate.Value;
                    break;
                default:
                    filterStartDate = DateTime.MinValue;
                    break;
            }

            if (filterStartDate != DateTime.MinValue)
                query = query.Where(t => t.TradeDate >= filterStartDate);

            if (filterEndDate != DateTime.MaxValue)
                query = query.Where(t => t.TradeDate <= filterEndDate);

            var trades = query.ToList();
            var model = new AnalyticsViewModel
            {
                TimeframeFilter = timeframe,
                StartDate = filterStartDate,
                EndDate = filterEndDate,
                FilteredTrades = trades
            };

            model.TotalTrades = trades.Count;
            model.WinningTrades = trades.Count(t => t.ProfitLoss > 0);
            model.LosingTrades = trades.Count(t => t.ProfitLoss < 0);
            model.TotalPnL = trades.Sum(t => t.ProfitLoss ?? 0);
            model.WinRate = model.TotalTrades > 0 ? Math.Round((decimal)model.WinningTrades / model.TotalTrades * 100, 1) : 0;
            model.AvgPnL = model.TotalTrades > 0 ? Math.Round(trades.Average(t => t.ProfitLoss ?? 0), 2) : 0;

            // FIX: Only get Best/Worst if trades exist
            model.BestTrade = trades.Any() ? trades.Max(t => t.ProfitLoss ?? 0) : 0;
            model.WorstTrade = trades.Any() ? trades.Min(t => t.ProfitLoss ?? 0) : 0;

            var grossProfit = trades.Where(t => t.ProfitLoss > 0).Sum(t => t.ProfitLoss ?? 0);
            var grossLoss = Math.Abs(trades.Where(t => t.ProfitLoss < 0).Sum(t => t.ProfitLoss ?? 0));
            model.ProfitFactor = grossLoss > 0 ? Math.Round(grossProfit / grossLoss, 2) : grossProfit > 0 ? 999.99m : 0;

            model.AvgRR = trades.Any(t => t.RiskRewardRatio.HasValue)
                ? Math.Round(trades.Where(t => t.RiskRewardRatio.HasValue).Average(t => t.RiskRewardRatio ?? 0), 2)
                : 0;

            model.CumulativePnL = new List<ChartDataPoint>();
            decimal cumulative = 0;
            foreach (var trade in trades.OrderBy(t => t.TradeDate))
            {
                cumulative += trade.ProfitLoss ?? 0;
                model.CumulativePnL.Add(new ChartDataPoint
                {
                    Label = trade.TradeDate.ToString("MM/dd"),
                    Value = cumulative
                });
            }

            model.PnLBySymbol = trades.GroupBy(t => t.Symbol)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(t => t.ProfitLoss ?? 0)
                }).ToList();

            model.WinRateBySetup = trades.GroupBy(t => t.SetupType ?? "Other")
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Count(t => t.ProfitLoss > 0) / (decimal)g.Count() * 100
                }).ToList();

            model.MonthlyPerformance = trades.GroupBy(t => new { t.TradeDate.Year, t.TradeDate.Month })
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Key.Month}/{g.Key.Year}",
                    Value = g.Sum(t => t.ProfitLoss ?? 0)
                }).OrderBy(c => c.Label).ToList();

            model.BiasAccuracy = trades.GroupBy(t => t.DailyBias ?? "Unknown")
                .Select(g => new BiasAccuracy
                {
                    Bias = g.Key,
                    TotalTrades = g.Count(),
                    WinCount = g.Count(t => t.ProfitLoss > 0),
                    WinRate = Math.Round((decimal)g.Count(t => t.ProfitLoss > 0) / g.Count() * 100, 1),
                    AvgPnL = Math.Round(g.Average(t => t.ProfitLoss ?? 0), 2)
                }).ToList();

            model.ConfluencePerformance = trades
                .Where(t => !string.IsNullOrEmpty(t.Confluences))
                .SelectMany(t => t.Confluences.Split(',').Select(c => new { Trade = t, Confluence = c.Trim() }))
                .GroupBy(x => x.Confluence)
                .Select(g => new ConfluenceData
                {
                    Confluence = g.Key,
                    Count = g.Count(),
                    WinRate = Math.Round((decimal)g.Count(x => x.Trade.ProfitLoss > 0) / g.Count() * 100, 1),
                    AvgPnL = Math.Round(g.Average(x => x.Trade.ProfitLoss ?? 0), 2)
                })
                .OrderByDescending(c => c.WinRate)
                .Take(5)
                .ToList();

            model.Insights = GenerateKeyInsights(trades);

            return View(model);
        }

        private List<KeyInsight> GenerateKeyInsights(List<TradeEntry> trades)
        {
            var insights = new List<KeyInsight>();

            var bestSetup = trades
                .GroupBy(t => t.SetupType ?? "Other")
                .Select(g => new { Setup = g.Key, WinRate = (decimal)g.Count(t => t.ProfitLoss > 0) / g.Count() * 100, Count = g.Count() })
                .Where(x => x.Count >= 3)
                .OrderByDescending(x => x.WinRate)
                .FirstOrDefault();

            if (bestSetup != null)
            {
                insights.Add(new KeyInsight
                {
                    Title = "Best Performing Setup",
                    Description = $"{bestSetup.Setup} with {bestSetup.WinRate:F1}% win rate ({bestSetup.Count} trades)",
                    Icon = "fa-trophy",
                    Color = "text-success"
                });
            }

            var worstSetup = trades
                .GroupBy(t => t.SetupType ?? "Other")
                .Select(g => new { Setup = g.Key, WinRate = (decimal)g.Count(t => t.ProfitLoss > 0) / g.Count() * 100, Count = g.Count() })
                .Where(x => x.Count >= 3)
                .OrderBy(x => x.WinRate)
                .FirstOrDefault();

            if (worstSetup != null && worstSetup.WinRate < 50)
            {
                insights.Add(new KeyInsight
                {
                    Title = "Needs Improvement",
                    Description = $"{worstSetup.Setup} has only {worstSetup.WinRate:F1}% win rate",
                    Icon = "fa-exclamation-triangle",
                    Color = "text-warning"
                });
            }

            var bestBias = trades
                .GroupBy(t => t.DailyBias ?? "Unknown")
                .Select(g => new { Bias = g.Key, WinRate = (decimal)g.Count(t => t.ProfitLoss > 0) / g.Count() * 100, Count = g.Count() })
                .Where(x => x.Count >= 3)
                .OrderByDescending(x => x.WinRate)
                .FirstOrDefault();

            if (bestBias != null && bestBias.Bias != "Unknown")
            {
                insights.Add(new KeyInsight
                {
                    Title = "Best Daily Bias",
                    Description = $"{bestBias.Bias} bias: {bestBias.WinRate:F1}% win rate",
                    Icon = "fa-chart-line",
                    Color = "text-info"
                });
            }

            return insights;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}