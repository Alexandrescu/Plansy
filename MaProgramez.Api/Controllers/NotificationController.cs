using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.Models;
using System.Globalization;
using System.Threading;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/Notification")]
    public class NotificationController : ApiController
    {
        #region Public Constructors

        public NotificationController()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Public Methods

        [Authorize]
        [Route("DeleteNotification")]
        [HttpPost]
        public IHttpActionResult DeleteNotification(NotificationParameters notificationParameters)
        {
            return Ok(NotificationBusinessLogic.DeleteNotification(
                notificationParameters.UserId,
                notificationParameters.NotificationId));
        }

        [Authorize]
        [Route("GetNotification")]
        [HttpPost]
        public IHttpActionResult GetNotification(NotificationParameters notificationParameters)
        {
            return Ok(NotificationBusinessLogic.GetNotification(
                notificationParameters.UserId,
                notificationParameters.NotificationId));
        }

        [Authorize]
        [Route("GetNotifications")]
        [HttpPost]
        public IHttpActionResult GetNotifications(NotificationParameters notificationParameters)
        {
            return Ok(NotificationBusinessLogic.GetNotifications(notificationParameters.UserId));
        }

        #endregion Public Methods
    }
}