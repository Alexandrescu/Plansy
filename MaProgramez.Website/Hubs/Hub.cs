using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace MaProgramez.Website.Hubs
{
    public abstract class Hub<T> : Hub where T : Hub
    {
        private static IHubContext hubContext;
        /// <summary>Gets the hub context.</summary>
        /// <value>The hub context.</value>
        public static IHubContext HubContext
        {
            get { return hubContext ?? (hubContext = GlobalHost.ConnectionManager.GetHubContext<T>()); }
        }
    }
}