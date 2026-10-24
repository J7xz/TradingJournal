using System.Web.Http;

namespace J7Journal
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable attribute routing ([Route("api/trades/import")] etc.)
            config.MapHttpAttributeRoutes();

            // Fallback conventional route (optional but harmless)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Return JSON by default (not XML)
            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }
    }
}