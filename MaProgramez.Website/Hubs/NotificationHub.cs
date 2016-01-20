using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace MaProgramez.Website.Hubs
{
    public class NotificationHub : Hub<NotificationHub>
    {
        public void SendNotification(string author, string message)
        {
            // Clients.All.sendNotification(author, message);
        }
    }
}