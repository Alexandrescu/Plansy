using System.Linq;
using MaProgramez.Repository.DbContexts;
using Microsoft.AspNet.SignalR;

namespace MaProgramez.Website.Membership
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(x => x.UserName == request.User.Identity.Name);
                return user != null ? user.Id : string.Empty;
            }
        }
    }
}