using System.Web.Mvc;
using System.Web.Routing;

namespace MaProgramez.Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{culture}/Public/{*pathInfo}");

            routes.MapRoute(
                name: "Service",
                url: "Service/{action}",
                defaults: new { culture = "", controller = "Service", action = "Index" }
            );

            //routes.MapRoute(
            //    name: "PublicPageRo",
            //    url: "ro/piese-auto/{*parameters}",
            //    defaults: new { culture = "ro", controller = "Public", action = "Index", parameters = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "PublicPageEn",
            //    url: "en/car-parts/{*parameters}",
            //    defaults: new { culture = "en", controller = "Public", action = "Index", parameters = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "PublicPageHu",
            //    url: "hu/autoalkatresz/{*parameters}",
            //    defaults: new { culture = "hu", controller = "Public", action = "Index", parameters = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{culture}/{controller}/{action}/{id}",
                defaults: new { culture = "", controller = "Home", action = "Index", id = UrlParameter.Optional } //culture = CultureHelper.GetDefaultCulture(),
            );

            //    routes.MapRoute(
            //name: "404-PageNotFound",
            //        // This will handle any non-existing urls
            //url: "{*url}",
            //        // "Shared" is the name of your error controller, and "Error" is the action/page
            //        // that handles all your custom errors
            //defaults: new { controller = "Shared", action = "Error" }
            //);
        }
    }
}