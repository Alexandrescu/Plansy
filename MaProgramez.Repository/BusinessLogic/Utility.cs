using System.Data.Entity;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MaProgramez.Repository.BusinessLogic
{
    public static class Utility
    {
        #region PUBLIC STATIC METHODS

        public static List<County> GetCounties()
        {
            using (var db = new AppDbContext())
            {
                var prestatori = db.Users.Include("Addresses.UserCity.CityCounty")//.Include("Addresses.UserCountry")
                                   .Where(u => u.Roles.Any(r => r.RoleId == "7a215679-fe92-4377-9077-2b8aa1f378ac")).ToList();

                var counties = prestatori.SelectMany(p => p.Addresses.Select(a => a.UserCity.CityCounty))
                                        .Distinct()
                                        .OrderBy(c => c.Name)
                                        .ToList();
                
                return counties;
            }
        }

        public static List<City> GetCities(int countyId)
        {
            using (var db = new AppDbContext())
            {
                var prestatori = db.Users.Include("Addresses.UserCity")
                                   .Where(u => u.Roles.Any(r => r.RoleId == "7a215679-fe92-4377-9077-2b8aa1f378ac")).ToList();

                var cities = prestatori.SelectMany(p => p.Addresses.Select(a => a.UserCity))
                    .Where(x => x.CountyId == countyId)
                    .Distinct()
                    .OrderBy(c => c.Name)
                    .ToList();

                return cities;
            }
        }

        #endregion PUBLIC STATIC METHODS
    }
}