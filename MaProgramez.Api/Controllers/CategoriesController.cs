using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.Models;
using System.Globalization;
using System.Threading;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/Categories")]
    public class CategoriesController : ApiController
    {
        #region Public Constructors

        public CategoriesController()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Public Methods

        // POST api/Categories/GetCategories
        [AllowAnonymous]
        [Route("GetCategories")]
        [HttpPost]
        public IHttpActionResult GetCategories(CategoryParameters categoryParameters)
        {
            return Ok(RetrieveLists.GetCategoriesFiltered(categoryParameters.ParentCategoryId,
                                                          categoryParameters.CityId,
                                                          categoryParameters.SearchText));
        }

        // POST api/Categories/GetCategory
        [AllowAnonymous]
        [Route("GetCategory")]
        [HttpPost]
        public IHttpActionResult GetCategoryById(CategoryParameters categoryParameters)
        {
            return Ok(RetrieveOthers.GetCategoryById(categoryParameters.CategoryId));
        }

        #endregion Public Methods
    }
}