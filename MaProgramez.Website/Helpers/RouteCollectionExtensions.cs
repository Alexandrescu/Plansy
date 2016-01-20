using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MaProgramez.Website.Helpers
{
    public static class RouteCollectionExtensions
    {
        public static Route MapRouteWithName(this RouteCollection routes,
        string name, string url, object defaults, object constraints)
        {
            var route = routes.MapRoute(name, url, defaults, constraints);
            route.DataTokens = new RouteValueDictionary {{"RouteName", name}};

            return route;
        }
    }
}