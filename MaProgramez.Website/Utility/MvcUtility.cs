namespace MaProgramez.Website.Utility
{
    using System.IO;
    using System.Web.Mvc;

    public static class MvcUtility
    {
        #region PUBLIC STATIC METHODS

        public static string RenderRazorViewToString(this Controller controller, string viewName, object model)
        {

            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        #endregion
    }
}