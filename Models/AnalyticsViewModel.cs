using System;
using System.Collections.Generic;

namespace J7Journal.Models
{
    public class AnalyticsViewModel
    {
        public int TotalTrades { get; set; }
        public int WinningTrades { get; set; }
        public int LosingTrades { get; set; }
        public decimal WinRate { get; set; }
        public decimal TotalPnL { get; set; }
        public decimal AvgPnL { get; set; }
        public decimal ProfitFactor { get; set; }
        public decimal AvgRR { get; set; }
        public decimal BestTrade { get; set; }
        public decimal WorstTrade { get; set; }

        public List<ChartDataPoint> CumulativePnL { get; set; }
        public List<ChartDataPoint> PnLBySymbol { get; set; }
        public List<ChartDataPoint> WinRateBySetup { get; set; }
        public List<ChartDataPoint> MonthlyPerformance { get; set; }

        public List<BiasAccuracy> BiasAccuracy { get; set; }
        public List<ConfluenceData> ConfluencePerformance { get; set; }
        public List<KeyInsight> Insights { get; set; }

        public string TimeframeFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TradeEntry> FilteredTrades { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public string Color { get; set; }
    }

    public class BiasAccuracy
    {
        public string Bias { get; set; }
        public int TotalTrades { get; set; }
        public int WinCount { get; set; }
        public decimal WinRate { get; set; }
        public decimal AvgPnL { get; set; }
    }

    public class ConfluenceData
    {
        public string Confluence { get; set; }
        public int Count { get; set; }
        public decimal WinRate { get; set; }
        public decimal AvgPnL { get; set; }
    }

    public class KeyInsight
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}