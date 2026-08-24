using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace J7Journal.Services
{
    public class PriceService
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly string _apiKey = System.Configuration.ConfigurationManager.AppSettings["FinnhubApiKey"];

        // Maps your symbols to Finnhub symbols
        private static readonly Dictionary<string, string> _symbolMap = new Dictionary<string, string>
        {
            { "GOLD", "GC=F" },      // Gold Futures
            { "NASDAQ", "IXIC" },    // NASDAQ Composite
            { "SP500", "SPX" }       // S&P 500
        };

        public async Task<Dictionary<string, decimal>> GetCurrentPrices()
        {
            var prices = new Dictionary<string, decimal>();

            foreach (var symbol in _symbolMap)
            {
                try
                {
                    var price = await GetPrice(symbol.Value);
                    prices[symbol.Key] = price;
                    System.Diagnostics.Debug.WriteLine($"{symbol.Key}: ${price}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting {symbol.Key}: {ex.Message}");
                    // Use fallback price if API fails
                    prices[symbol.Key] = GetFallbackPrice(symbol.Key);
                }
            }

            return prices;
        }

        private async Task<decimal> GetPrice(string finnhubSymbol)
        {
            var url = $"https://finnhub.io/api/v1/quote?symbol={finnhubSymbol}&token={_apiKey}";
            var response = await _client.GetStringAsync(url);

            var serializer = new JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(response);

            // Finnhub returns: { "c": 123.45, "h": 124.00, "l": 122.00, "o": 123.00 }
            if (data.ContainsKey("c") && data["c"] != null)
            {
                return Convert.ToDecimal(data["c"]);
            }

            return 0;
        }

        private decimal GetFallbackPrice(string symbol)
        {
            var fallbacks = new Dictionary<string, decimal>
            {
                { "GOLD", 2050.50m },
                { "NASDAQ", 16200.00m },
                { "SP500", 4850.00m }
            };

            return fallbacks.ContainsKey(symbol) ? fallbacks[symbol] : 0;
        }

        // Test method to check if API key works
        public async Task<bool> TestConnection()
        {
            try
            {
                var url = $"https://finnhub.io/api/v1/quote?symbol=GC=F&token={_apiKey}";
                var response = await _client.GetStringAsync(url);
                return !string.IsNullOrEmpty(response);
            }
            catch
            {
                return false;
            }
        }
    }
}