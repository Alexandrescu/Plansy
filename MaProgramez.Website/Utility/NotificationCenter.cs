namespace MaProgramez.Website.Utility
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Web.Helpers;
    using MaProgramez.Website.Hubs;
    using Microsoft.AspNet.SignalR;

    public static class NotificationCenter
    {
        public static void SendNotificationToUser(string userId, string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.User(userId).sendNotification(message);
        }

        public static void SendNewRequestToSuppliers(string supplierId, string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.User(supplierId).sendNewRequestMessageForSuppliers(message);
        }

        public static void SendActiveRequestsCountToSuppliers(string supplierId, int activeRequestsCount)
        {
            var message = new { ActiveRequetsCount = activeRequestsCount };
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.User(supplierId).sendActiveRequestsCountToSuppliers(message);
        }

        public static void RefreshClientActiveRequest(string clientId, int requestId, string view)
        {
            var message = new
            {
                View = view,
                RequestId = requestId
            };
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.User(clientId).refreshClientActiveRequest(message);
        }

        public static void RefreshRequestTableList(int requestId, string view)
        {
            var message = new
            {
                RequestId = requestId,
                View = view
            };
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.All.refreshRequestTableList(message);
        }

        public static void RemoveRequestFromList(string supplierId, int requestId)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.AllExcept(supplierId).removeRequestFromList(requestId);
        }
    }
}