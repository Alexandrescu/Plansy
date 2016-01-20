using MaProgramez.Repository.DbContexts;

namespace MaProgramez.Website.Helpers
{
    using System;
    using System.Linq;

    public class AgentHelper
    {
        public static int GetTotalUsersByRole(string agentId, string role, DateTime? untilDate)
        {
            using (var db = new AppDbContext())
            {
                var userRole = db.Roles.FirstOrDefault(x => x.Name.ToUpper() == role.ToUpper());
                if (userRole == null)
                {
                    return 0;
                }

                var roleId = userRole.Id;

                    if (untilDate == null)
                    {
                        return db.Users.Count(x => x.AgentId == agentId && x.Roles.Any(r => r.RoleId == roleId));
                    }
                    else
                    {
                        return db.Users.Count(x => x.AgentId == agentId && x.CreatedDate >= untilDate && x.Roles.Any(r => r.RoleId == roleId));
                    }
            }
        }
    }
}