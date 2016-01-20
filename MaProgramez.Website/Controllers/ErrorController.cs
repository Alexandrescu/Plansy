using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MaProgramez.Website.Controllers
{
    public partial class ErrorController : BaseController
    {
        [AllowAnonymous]
        public virtual ViewResult Index()
        {
            return View("Error");
        }
        
        [AllowAnonymous]
        public virtual ViewResult Error()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual ViewResult NotFound()
        {
            Response.StatusCode = 404;  //you may want to set this to 200
            return View("NotFound");
        }

        public virtual ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}