using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using J7Journal.Data;
using J7Journal.Models;

namespace J7Journal.Controllers
{
    [RoutePrefix("api/trades")]
    public class Mt5ApiController : ApiController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        [Route("import")]
        public HttpResponseMessage Import([FromBody] Mt5TradeDto dto)
        {
            // 1. API key check
            var expectedKey = ConfigurationManager.AppSettings["MT5ApiKey"];
            if (string.IsNullOrEmpty(expectedKey))
                return Json(HttpStatusCode.InternalServerError, new { error = "Server misconfigured: MT5ApiKey missing" });

            var providedKey = "";
            var header = Request.Headers.FirstOrDefault(h => h.Key.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(header.Key))
                providedKey = header.Value.FirstOrDefault() ?? "";

            if (providedKey != expectedKey)
                return Json(HttpStatusCode.Unauthorized, new { error = "Invalid API key" });

            // 2. Validate
            if (dto == null || string.IsNullOrWhiteSpace(dto.Symbol) || dto.Ticket <= 0)
                return Json(HttpStatusCode.BadRequest, new { error = "Invalid payload" });

            // 3. Dedupe
            var existing = db.TradeEntries.FirstOrDefault(t => t.MT5Ticket == dto.Ticket);
            if (existing != null)
                return Json(HttpStatusCode.OK, new { status = "duplicate", id = existing.Id });

            // 4. Map Buy/Sell → LONG/SHORT
            var direction = (dto.Type ?? "").Trim().ToUpper();
            if (direction == "BUY") direction = "LONG";
            else if (direction == "SELL") direction = "SHORT";
            else return Json(HttpStatusCode.BadRequest, new { error = "Type must be Buy or Sell" });

            // 5. Insert
            var trade = new TradeEntry
            {
                // Execution data from MT5
                Symbol = dto.Symbol.Trim(),
                Direction = direction,
                EntryPrice = dto.EntryPrice,
                ExitPrice = dto.ExitPrice,
                ProfitLoss = dto.Profit,
                Volume = dto.Volume,
                MT5Ticket = dto.Ticket,
                MT5OpenTime = DateTime.SpecifyKind(dto.OpenTime, DateTimeKind.Utc),
                MT5CloseTime = DateTime.SpecifyKind(dto.CloseTime, DateTimeKind.Utc),

                // Journal side = empty
                TradeDate = DateTime.SpecifyKind(dto.CloseTime, DateTimeKind.Utc),
                ScreenshotData = null,
                ContentType = null,
                DailyBias = null,
                HTFLiquidity = null,
                Confluences = null,
                ReasonForEntry = null,
                LessonLearned = null,
                SetupType = null,
                Rating = null,
                RiskRewardRatio = null,

                // Meta
                ImportedFromMT5 = true,
                IsJournalComplete = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.TradeEntries.Add(trade);

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                // race condition — another request inserted the same ticket first
                var existingNow = db.TradeEntries.FirstOrDefault(t => t.MT5Ticket == dto.Ticket);
                if (existingNow != null)
                    return Json(HttpStatusCode.OK, new { status = "duplicate", id = existingNow.Id });
                throw;
            }

            return Json(HttpStatusCode.OK, new { status = "ok", id = trade.Id });
        }

        private HttpResponseMessage Json(HttpStatusCode code, object body)
        {
            return Request.CreateResponse(code, body);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}