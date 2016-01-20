using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/Utility")]
    public class UtilityController : ApiController
    {
        #region Public Constructors

        public UtilityController()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Public Methods

        [AllowAnonymous]
        [Route("GetCities")]
        [HttpPost]
        public IHttpActionResult GetCities(UtilityViewModel utility)
        {
            return Ok(Utility.GetCities(utility.CountyId.Value));
        }

        [AllowAnonymous]
        [Route("GetCounties")]
        [HttpPost]
        public IHttpActionResult GetCounties(UtilityViewModel utility)
        {
            List<County> counties = Utility.GetCounties();
            List<City> cities = null;

            if (utility.CountyId.HasValue)
            {
                cities = Utility.GetCities(utility.CountyId.Value);
            }

            return Ok(new LocationViewModel
            {
                Cities = cities,
                Counties = counties
            });
        }

        #endregion Public Methods
    }
}