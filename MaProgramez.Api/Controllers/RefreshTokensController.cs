using MaProgramez.Repository.DbContexts;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/RefreshTokens")]
    public class RefreshTokensController : ApiController
    {
        #region Private Fields

        private readonly AuthRepository _repo;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokensController"/> class.
        /// </summary>
        public RefreshTokensController()
        {
            _repo = new AuthRepository();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Public Methods

        //[Authorize(Users = "Admin")]
        [AllowAnonymous]
        [Route("")]
        public async Task<IHttpActionResult> Delete(string tokenId)
        {
            bool result = await _repo.RemoveRefreshToken(tokenId);
            if (result)
            {
                return Ok();
            }
            return BadRequest("Token Id does not exist");
        }

        [Authorize(Users = "Admin")]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAllRefreshTokens());
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion Protected Methods
    }
}