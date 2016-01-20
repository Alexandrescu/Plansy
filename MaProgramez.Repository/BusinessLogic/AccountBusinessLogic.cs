using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Models;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Repository.BusinessLogic
{
    public static class AccountBusinessLogic
    {
        public static ApplicationUser SaveUserDetails(UserModel userModel)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userModel.UserId);
                if (user == null)
                {
                    return null;
                }

                user.PhoneNumberConfirmed = user.PhoneNumber == userModel.Phone;
                user.FirstName = userModel.FirstName;
                user.LastName = userModel.LastName;
                user.PhoneNumber = userModel.Phone;

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return user;
            }
        }
    }
}
