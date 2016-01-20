using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MaProgramez.Website.Extensions
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string culture = filterContext.RouteData.Values["culture"] == null ? "ro" : filterContext.RouteData.Values["culture"].ToString();
            string url = string.Format("~/{0}/Account/Login?returnUrl={1}", culture, HttpUtility.UrlEncode(filterContext.HttpContext.Request.RawUrl));

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var redirectResult = filterContext.Result as RedirectResult;
                if (filterContext.Result is RedirectResult)
                {
                    // It was a RedirectResult => we need to calculate the url
                    var result = filterContext.Result as RedirectResult;
                    url = UrlHelper.GenerateContentUrl(result.Url, filterContext.HttpContext);
                }
                else if (filterContext.Result is RedirectToRouteResult)
                {
                    // It was a RedirectToRouteResult => we need to calculate
                    // the target url
                    var result = filterContext.Result as RedirectToRouteResult;
                    url = UrlHelper.GenerateUrl(result.RouteName, null, null, result.RouteValues, RouteTable.Routes, filterContext.RequestContext, false);
                }

                filterContext.Result = new JsonResult
                {
                    Data = new { Redirect = url },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result =
                new RedirectResult
                    (string.Format("~/{0}/Account/Login?returnUrl={1}",
                                    culture,
                                    HttpUtility.UrlEncode(filterContext.HttpContext.Request.RawUrl)));
                //non-ajax request
                //base.HandleUnauthorizedRequest(filterContext);
            }

        }
    }
}