using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Optimization;

namespace MaProgramez.Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region STYLES

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/nanoscroller.css",
                      "~/Content/Site.css"));

            bundles.Add(new StyleBundle("~/Content/landing-page").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/LandingPage.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap-datepicker").Include(
                      "~/Content/datepicker3.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap-select").Include("~/Content/bootstrap-select.css"));

            bundles.Add(new StyleBundle("~/Content/slick").Include(
                      "~/Content/slick.css"));

            bundles.Add(new StyleBundle("~/Content/animate").Include(
                      "~/Content/animate.css"));

            #endregion

            #region SCRIPTS

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/globalize/globalize.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                    .Include("~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.validate.unobtrusive.js",
                    "~/Scripts/jquery.unobtrusive-ajax.js",
                    "~/Scripts/Pages/ValidationExtension.js",
                    "~/Scripts/jquery.validate.globalize.js"));
            
            // LINK: https://gist.github.com/johnnyreilly/4528994
            //Create culture specific bundles which contain the JavaScript files that should be served for each culture
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                bundles.Add(new ScriptBundle("~/js-culture." + culture.Name).Include( //example bundle name would be "~/js-culture.en-GB"
                    DetermineCultureFile(culture, "~/Scripts/globalize/cultures/globalize.culture.{0}.js")             //The Globalize locale-specific JavaScript file
                    //DetermineCultureFile(culture, "~/Scripts/bootstrap-datepicker-locales/bootstrap-datepicker.{0}.js") //The Bootstrap Datepicker locale-specific JavaScript file
                ));
            }

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                        "~/Scripts/Pages/Scripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-select").Include("~/Scripts/bootstrap-select.js"));

            bundles.Add(new ScriptBundle("~/bundles/knob").Include(
                        "~/Scripts/knob.js"));

            bundles.Add(new ScriptBundle("~/bundles/upload-images").Include(
                        "~/Scripts/load-image.js",
                        "~/Scripts/Pages/UploadImages.js"));

            bundles.Add(new ScriptBundle("~/bundles/add-request").Include(
                        "~/Scripts/Pages/AddRequest.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-datepicker").Include(
                        "~/Scripts/bootstrap-datepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                        "~/Scripts/jquery.signalR*"));

            bundles.Add(new ScriptBundle("~/bundles/notification").Include(
                        "~/Scripts/jquery.nanoscroller.min.js",
                        "~/Scripts/Pages/notification.js"));

            bundles.Add(new ScriptBundle("~/bundles/address").Include(
                        "~/Scripts/Pages/Address.js"));

            bundles.Add(new ScriptBundle("~/bundles/personal-details").Include(
                        "~/Scripts/Pages/PersonalDetails.js"));

            bundles.Add(new ScriptBundle("~/bundles/chart").Include(
                        "~/Scripts/chart.js"));

            bundles.Add(new ScriptBundle("~/bundles/supplier-dashboard").Include(
                        "~/Scripts/Pages/SupplierDashboard.js"));

            bundles.Add(new ScriptBundle("~/bundles/agent-dashboard").Include(
                        "~/Scripts/Pages/AgentDashboard.js"));

            bundles.Add(new ScriptBundle("~/bundles/admin-dashboard").Include(
                        "~/Scripts/Pages/AdminDashboard.js"));

            bundles.Add(new ScriptBundle("~/bundles/slick").Include(
                        "~/Scripts/slick.js"));

            bundles.Add(new ScriptBundle("~/bundles/landing-page").Include(
                        "~/Scripts/Pages/LandingPage.js"));

            bundles.Add(new ScriptBundle("~/bundles/clickdesk").Include(
                        "~/Scripts/ClickDeskOnlineSupport.js"));

            bundles.Add(new ScriptBundle("~/bundles/add-user").Include(
                        "~/Scripts/Pages/AddUser.js"));

            bundles.Add(new ScriptBundle("~/bundles/googleAnalytics").Include(
                       "~/Scripts/GoogleAnalytics.js"));

            bundles.Add(new ScriptBundle("~/bundles/finalize").Include(
                        "~/Scripts/Pages/Finalize.js"));

            bundles.Add(new ScriptBundle("~/bundles/waypoints").Include(
                        "~/Scripts/jquery.waypoints.js"));

            #endregion

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            //BundleTable.EnableOptimizations = true;
        }

        /// <summary>
        /// Given the supplied culture, determine the most appropriate Globalize culture script file that should be served up
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="filePattern">a file pattern, eg "~/Scripts/globalize-cultures/globalize.culture.{0}.js"</param>
        /// <param name="defaultCulture">Default culture string to use (eg "en-GB") if one cannot be found for the supplied culture</param>
        /// <returns></returns>
        private static string DetermineCultureFile(CultureInfo culture,
            string filePattern,
            string defaultCulture = "nl-NL"
            )
        {
            //Determine culture - GUI culture for preference, user selected culture as fallback
            var regionalisedFileToUse = string.Format(filePattern, defaultCulture);

            //Try to pick a more appropriate regionalisation if there is one
            if (File.Exists(HttpContext.Current.Server.MapPath(string.Format(filePattern, culture.Name)))) //First try for a globalize.culture.en-GB.js style file
                regionalisedFileToUse = string.Format(filePattern, culture.Name);
            else if (File.Exists(HttpContext.Current.Server.MapPath(string.Format(filePattern, culture.TwoLetterISOLanguageName)))) //That failed; now try for a globalize.culture.en.js style file
                regionalisedFileToUse = string.Format(filePattern, culture.TwoLetterISOLanguageName);

            return regionalisedFileToUse;
        }
    }
}
