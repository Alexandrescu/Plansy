using System.Collections.Generic;
using MaProgramez.Resources;

namespace MaProgramez.Website.ViewModels
{
    public class NotificationViewModel
    {
        public string User { get; set; }
        public Dictionary<int, string> NotificationTypes { get; set; }
        public int Result { get; set; }
        public string Text { get; set; }

        public NotificationViewModel()
        {

        }

        public NotificationViewModel(string user)
        {
            this.User = user;
            NotificationTypes = new Dictionary<int, string> { { 1, Resource.NotificationType_Advertisement }, { 2, Resource.NotificationType_SystemAlert } };
        }
    }
}