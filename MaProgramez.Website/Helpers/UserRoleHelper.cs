
using MaProgramez.Repository.DbContexts;

namespace MaProgramez.Website.Helpers
{
    using System.Linq;

    public static class UserRoleHelper
    {
        /// <summary>
        /// Returns true if the user has the specified role
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        public static bool UserHasRole(string userId, string role)
        {
            using (var db = new AppDbContext())
            {
                var exists = (from r in db.Roles
                    where r.Users.Any(u => u.UserId == userId) &&
                    r.Name.ToUpper() == role.ToUpper()
                    select r).FirstOrDefault();

                return exists != null;
            }
        }
    }
}