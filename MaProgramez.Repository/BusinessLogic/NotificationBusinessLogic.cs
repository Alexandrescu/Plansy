namespace MaProgramez.Repository.BusinessLogic
{
    using MaProgramez.Repository.DbContexts;
    using MaProgramez.Repository.Entities;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    public static class NotificationBusinessLogic
    {
        #region PUBLIC STATIC METHODS

        public static List<Notification> GetNotifications(string userId)
        {
            using (var db = new AppDbContext())
            {
                return db.Notifications.Where(x => x.UserId == userId
                    && x.IsDeleted == false).ToList();
            }
        }

        public static Notification GetNotification(string userId, int notificationId)
        {
            using (var db = new AppDbContext())
            {
                return db.Notifications.FirstOrDefault(x => x.Id == notificationId
                    && x.UserId == userId);
            }
        }

        public static bool DeleteNotification(string userId, int notificationId)
        {
            using (var db = new AppDbContext())
            {
                var notification = db.Notifications.FirstOrDefault(x => x.Id == notificationId
                    && x.UserId == userId);

                if (notification != null)
                {
                    notification.IsDeleted = true;
                    db.Entry(notification).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
