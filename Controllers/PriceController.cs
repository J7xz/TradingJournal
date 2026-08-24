using System.Collections.Generic;
using System.Web.Mvc;

namespace J7Journal.Controllers
{
    public class PriceController : Controller
    {
        public JsonResult GetPrices()
        {
            var prices = new Dictionary<string, decimal>
            {
                { "GOLD", 2050.50m },
                { "NASDAQ", 16200.00m },
                { "SP500", 4850.00m }
            };

            return Json(prices, JsonRequestBehavior.AllowGet);
        }
    }
}