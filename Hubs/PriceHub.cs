using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.AspNet.SignalR;

namespace J7Journal.Hubs
{
    public class PriceHub : Hub
    {
        private static System.Timers.Timer _timer;
        private static Dictionary<string, dynamic> _lastData = new Dictionary<string, dynamic>();
        private static readonly HttpClient _client = new HttpClient();
        private static readonly string _apiKey = System.Configuration.ConfigurationManager.AppSettings["FinnhubApiKey"];
        private static readonly Random _random = new Random();

        // UPDATED: Map your symbols to Finnhub symbols
        private static readonly Dictionary<string, string> _symbolMap = new Dictionary<string, string>
        {
            { "XAUUSD", "XAUUSD" },    // Gold Spot
            { "US100", "US100" },      // NASDAQ 100
            { "US500", "US500" }       // S&P 500
        };

        static PriceHub()
        {
            // Initialize with fallback data
            _lastData = GetFallbackData();

            // Start timer to update every 5 seconds
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    var data = await GetQuoteData();
                    _lastData = data;
                    GlobalHost.ConnectionManager.GetHubContext<PriceHub>()
                        .Clients.All.priceUpdate(data);
                    System.Diagnostics.Debug.WriteLine("Prices updated from API");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("API Error: " + ex.Message);
                    // Use simulated data when API fails
                    var simData = GetSimulatedData();
                    _lastData = simData;
                    GlobalHost.ConnectionManager.GetHubContext<PriceHub>()
                        .Clients.All.priceUpdate(simData);
                    System.Diagnostics.Debug.WriteLine("Using simulated data");
                }
            };
            _timer.Start();
            System.Diagnostics.Debug.WriteLine("PriceHub timer started");
        }

        private static async Task<Dictionary<string, dynamic>> GetQuoteData()
        {
            var result = new Dictionary<string, dynamic>();

            foreach (var symbol in _symbolMap)
            {
                try
                {
                    var url = $"https://finnhub.io/api/v1/quote?symbol={symbol.Value}&token={_apiKey}";
                    var response = await _client.GetStringAsync(url);

                    var serializer = new JavaScriptSerializer();
                    var data = serializer.Deserialize<Dictionary<string, object>>(response);

                    // Check if we got valid data
                    if (data.ContainsKey("c") && data["c"] != null)
                    {
                        result[symbol.Key] = new
                        {
                            c = Convert.ToDecimal(data["c"]),
                            d = data.ContainsKey("d") ? Convert.ToDecimal(data["d"]) : 0m,
                            dp = data.ContainsKey("dp") ? Convert.ToDecimal(data["dp"]) : 0m,
                            h = data.ContainsKey("h") ? Convert.ToDecimal(data["h"]) : 0m,
                            l = data.ContainsKey("l") ? Convert.ToDecimal(data["l"]) : 0m,
                            o = data.ContainsKey("o") ? Convert.ToDecimal(data["o"]) : 0m,
                            pc = data.ContainsKey("pc") ? Convert.ToDecimal(data["pc"]) : 0m,
                            t = data.ContainsKey("t") ? Convert.ToInt64(data["t"]) : 0
                        };
                    }
                    else
                    {
                        // API returned empty response, use simulated
                        result[symbol.Key] = GetSimulatedPrice(symbol.Key);
                    }
                }
                catch
                {
                    // Use simulated data
                    result[symbol.Key] = GetSimulatedPrice(symbol.Key);
                }
            }

            return result;
        }

        private static Dictionary<string, dynamic> GetFallbackData()
        {
            return new Dictionary<string, dynamic>
            {
                { "XAUUSD", new { c = 4648.92m, d = 5.20m, dp = 0.11m, h = 4660.00m, l = 4635.00m, o = 4642.00m, pc = 4643.72m, t = 0 } },
                { "US100", new { c = 19850.00m, d = 45.00m, dp = 0.23m, h = 19900.00m, l = 19800.00m, o = 19830.00m, pc = 19805.00m, t = 0 } },
                { "US500", new { c = 5650.00m, d = 12.50m, dp = 0.22m, h = 5660.00m, l = 5640.00m, o = 5645.00m, pc = 5637.50m, t = 0 } }
            };
        }

        private static dynamic GetSimulatedPrice(string symbol)
        {
            // Start with base prices
            decimal basePrice = 0;
            switch (symbol)
            {
                case "XAUUSD": basePrice = 4648.92m; break;
                case "US100": basePrice = 19850.00m; break;
                case "US500": basePrice = 5650.00m; break;
                default: basePrice = 1000m; break;
            }

            // Get previous price from last data if available
            if (_lastData.ContainsKey(symbol))
            {
                basePrice = _lastData[symbol].c;
            }

            // Random change
            var changePercent = (decimal)(_random.NextDouble() * 2 - 1);
            var newPrice = basePrice * (1 + changePercent / 100);
            var change = newPrice - basePrice;

            return new
            {
                c = Math.Round(newPrice, 2),
                d = Math.Round(change, 2),
                dp = Math.Round(changePercent, 2),
                h = Math.Round(newPrice + (decimal)(_random.NextDouble() * 10), 2),
                l = Math.Round(newPrice - (decimal)(_random.NextDouble() * 10), 2),
                o = Math.Round(basePrice, 2),
                pc = Math.Round(basePrice, 2),
                t = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        private static Dictionary<string, dynamic> GetSimulatedData()
        {
            var result = new Dictionary<string, dynamic>();
            var symbols = new[] { "XAUUSD", "US100", "US500" };
            foreach (var symbol in symbols)
            {
                result[symbol] = GetSimulatedPrice(symbol);
            }
            return result;
        }

        public override Task OnConnected()
        {
            Clients.Caller.priceUpdate(_lastData);
            return base.OnConnected();
        }

        public void Subscribe(string symbol)
        {
            Groups.Add(Context.ConnectionId, symbol);
        }

        public void Unsubscribe(string symbol)
        {
            Groups.Remove(Context.ConnectionId, symbol);
        }
    }
}