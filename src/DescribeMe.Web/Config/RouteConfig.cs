using System.Web.Mvc;
using System.Web.Routing;

namespace DescribeMe.Web.Config
{
    public static class RouteConfig
    {
        private const string MatchPositiveInteger = @"\d{1,10}";

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "HomeDescribe",
                url: "describe",
                defaults: new { controller = "home", action = "index" }
            );

            routes.MapRoute(
                name: "HomeExamples",
                url: "examples",
                defaults: new { controller = "home", action = "examples" }
            );

            routes.MapRoute(
                name: "HomeImageNotFound",
                url: "imagenotfound",
                defaults: new { controller = "home", action = "imagenotfound" }
            );

            routes.MapRoute(
                name: "HomeReview",
                url: "review",
                defaults: new { controller = "home", action = "review" }
            );

            routes.MapRoute(
                name: "Admin",
                url: "admin/{action}",
                defaults: new { controller = "admin", action = "index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "home", action = "index" }
            );
        }
    }
}