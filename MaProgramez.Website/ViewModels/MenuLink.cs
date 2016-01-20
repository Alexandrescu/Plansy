using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MaProgramez.Website.ViewModels
{
    public class MenuLink
    {
        #region PROPERTIES

        public string Controller { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string IconClass { get; set; }

        #endregion

        #region CONSTRUCTOR

        public MenuLink(string controller, string action, string title, string code, string iconClass)
        {
            this.Controller = controller;
            this.Action = action;
            this.Title = title;
            this.Code = code;
            this.IconClass = iconClass;
        }

        #endregion
    }
}