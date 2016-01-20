using MaProgramez.Website.Extensions;
using System.Web.Mvc;

namespace MaProgramez.Website
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MyErrorHandler());
            
            //filters.Add(new HandleErrorAttribute());
            //filters.Add(new HandleAntiforgeryTokenErrorAttribute() { ExceptionType = typeof(HttpAntiForgeryException) });
        }
    }
}
