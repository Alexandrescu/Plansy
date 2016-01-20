using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MaProgramez.Repository.BusinessLogic
{
    public static class RetrieveOthers
    {
        #region Public Methods

        public static List<int> AcceptAllPendingSchedulesOnSlot(int slotId)
        {
            var schedules = RetrieveLists.GetSchedulesByProviderSlot(slotId, true);

            using (var db = new AppDbContext())
            {
                foreach (var s in schedules)
                {
                    s.State = ScheduleState.Valid;
                    db.Schedules.Attach(s);
                    db.Entry(s).State = EntityState.Modified;
                }

                db.SaveChanges();
            }

            return schedules.Select(x => x.Id).ToList();
        }

        public static void AcceptPendingSchedule(int scheduleId)
        {
            using (var db = new AppDbContext())
            {
                var s = db.Schedules.Find(scheduleId);

                s.State = ScheduleState.Valid;
                db.Schedules.Attach(s);
                db.Entry(s).State = EntityState.Modified;

                db.SaveChanges();
            }
        }

        public static void ActivatePhoneNumber(string userId)
        {
            using (var db = new AppDbContext())
            {
                var u = db.Users.Find(userId);

                u.PhoneNumberConfirmed = true;
                db.Users.Attach(u);
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void ActivateAccount(string userId)
        {
            using (var db = new AppDbContext())
            {
                var u = db.Users.Find(userId);

                u.EmailConfirmed = true;
                db.Users.Attach(u);
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static Notification AddNotification(int scheduleId, string userId, NotificationType type, string text)
        {
            using (var db = new AppDbContext())
            {
                var n = new Notification
                {
                    CreatedDate = DateTime.Now,
                    ScheduleId = scheduleId,
                    UserId = userId,
                    Type = type,
                    Text = text
                };
                db.Notifications.Add(n);
                db.SaveChanges();

                return n;
            }
        }

        public static void AddToFavorites(string userId, string favoriteProviderId, int? favoriteSlotId)
        {
            using (var db = new AppDbContext())
            {
                Favorite f = null;
                if (favoriteSlotId.HasValue)
                {
                    if (!db.Favorites.Any(x => x.UserId == userId && x.FavoriteUserId == favoriteProviderId && x.FavoriteSlotId == favoriteSlotId.Value))
                    {
                        var slot = GetSlotById((int)favoriteSlotId);
                        f = new Favorite()
                        {
                            UserId = userId,
                            FavoriteUserId = slot.ProviderId,
                            FavoriteSlotId = slot.Id
                        };
                    }
                }
                else
                {
                    if (!db.Favorites.Any(x => x.UserId == userId && x.FavoriteUserId == favoriteProviderId && x.FavoriteSlotId == null))
                    {
                        f = new Favorite()
                        {
                            UserId = userId,
                            FavoriteUserId = favoriteProviderId
                        };
                    }
                }

                if (f != null)
                {
                    db.Favorites.Add(f);
                    db.SaveChanges();
                }
            }
        }

        public static List<DateTime> GetAvailableHours(int slotId, List<SlotOperation> selectedSlotOperations, DateTime selectedDate, string userId, bool ignoreTimetable = false)
        {
            using (var db = new AppDbContext())
            {
                selectedDate = selectedDate.Date;
                var dayOfTheWeek = (Day)selectedDate.DayOfWeek;

                var slotTimeTables =
                    db.SlotTimeTables.Where(
                        s => s.SlotId == slotId && (int)s.DayOfWeek == (int)dayOfTheWeek)
                            .Distinct()
                            .ToList();

                var minDuration =
                   selectedSlotOperations
                        .OrderBy(x => x.DurationMinutes)
                        .First()
                        .DurationMinutes;

                var selectedOperationsDuration = selectedSlotOperations.Sum(x => x.DurationMinutes);

                var schedules =
                    db.Schedules.Where(
                        s => DbFunctions.TruncateTime(s.ScheduleDateTimeStart) == selectedDate &&
                            (s.SlotId == slotId || s.UserId == userId)
                            )
                            .ToList();

                var nonWorkingDays = db.SlotNonWorkingDays.Where(snwd => snwd.SlotId == slotId).ToList();
                var currentYear = DateTime.Today.Year;
                var nationalNonWorkingDays = db.DefaultNonWorkingDays.Where(nwd => nwd.StartDateTime.Year == currentYear).ToList();

                var availableHours = new List<DateTime>();
                foreach (var t in slotTimeTables)
                {
                    if (!ignoreTimetable)
                    {
                        // ignore non working days
                        if (nonWorkingDays.Any(x => x.SlotId == t.SlotId &&
                                                    x.StartDateTime.Date == selectedDate.Date &&
                                                    x.IsWorkingDay == false))
                        {
                            continue;
                        }

                        // ignore national non working days
                        if (nationalNonWorkingDays.Any(x => x.StartDateTime.Date == selectedDate.Date &&
                                                            !nonWorkingDays.Any(nwd => nwd.SlotId == t.SlotId &&
                                                                                       nwd.StartDateTime.Date ==
                                                                                       selectedDate.Date &&
                                                                                       nwd.IsWorkingDay)))
                        {
                            continue;
                        }

                        //ignore non working week if that's the case
                        if (t.WorkingWeekStartDate.HasValue &&
                            !TimeHelpers.IsWorkingWeeek(t.WorkingWeekStartDate.Value.Date, selectedDate.Date))
                        {
                            continue;
                        }


                        // skip today if outside working hours
                        if (selectedDate.Date == DateTime.Today.Date &&
                            t.StarTime.TimeOfDay < DateTime.Now.TimeOfDay)
                        {
                            if (DateTime.Now.AddMinutes(15).TimeOfDay > t.EndTime.TimeOfDay)
                            {
                                continue;
                            }
                        }

                        availableHours.AddRange(TimeHelpers.GetAvailableMinutesForInterval(selectedDate,
                            t.StarTime.TimeOfDay,
                            t.EndTime.TimeOfDay,
                            schedules,
                            minDuration,
                            selectedOperationsDuration));
                    }
                    else
                    {
                        availableHours.AddRange(TimeHelpers.GetAvailableMinutesForInterval(selectedDate,
                            new TimeSpan(0,0,0), 
                            new TimeSpan(24,0,0), 
                            schedules,
                            minDuration,
                            selectedOperationsDuration));
                    }
                }

                return availableHours.Distinct().ToList();
            }
        }

        public static Category GetCategoryById(int categoryId)
        {
            using (var db = new AppDbContext())
            {
                return db.Categories.FirstOrDefault(x => x.Id == categoryId);
            }
        }

        public static int GetCityIdByName(string cityName)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var split = cityName.Split(new[] { ',' }); // string format should be City, County
                    var city = split.Any() ? split[0] : cityName;

                    return db.Cities.First(c => c.Name == city).Id;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static string GetClientsCityName(string userId)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    return db.Addresses.Include("UserCity.CityCounty").First(a => a.UserId == userId).UserCity.Name;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static int GetCountyIdByName(string cityName)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var split = cityName.Split(new[] { ',' }); // string format should be City, County
                    var countyName = split.Count() == 2 ? split[1] : string.Empty;

                    return db.Counties.First(c => c.Name == countyName).Id;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static Slot GetFirstAvailableSlot(List<Slot> avilableSlots, DateTime startDateTime, List<int> selectedOperationIds)
        {
            using (var db = new AppDbContext())
            {
                foreach (var slot in avilableSlots)
                {
                    var slotOperations = RetrieveLists.GetSlotOperationsByIds(selectedOperationIds, slot.Id);
                    var endDateTime = startDateTime.AddMinutes(slotOperations.Sum(x => x.DurationMinutes));

                    var slotSchedules = db.Schedules.Where(s => s.SlotId == slot.Id &&
                                                                (
                                                                    (s.ScheduleDateTimeStart <= startDateTime &&
                                                                     s.ScheduleDateTimeEnd > startDateTime) ||
                                                                    (s.ScheduleDateTimeStart < endDateTime &&
                                                                     s.ScheduleDateTimeEnd >= startDateTime)
                                                                    ));
                    if (!slotSchedules.Any())
                    {
                        return slot;
                    }
                }
            }

            return null; // ASTA N-AR TREBUI SA SE INTAMPLE NICIODATA
        }

        public static Address GetUserAddress(string userId)
        {
            using (var db = new AppDbContext())
            {
                if (!db.Addresses.Any(a => a.UserId == userId))
                {
                    return null;
                }

                if (db.Addresses.Any(a => a.UserId == userId && a.AddressType == AddressType.PlaceOfBusinessAddress))
                {
                    return
                        db.Addresses.Include("UserCity.CityCounty")
                            .Include("UserCountry")
                            .First(a => a.UserId == userId && a.AddressType == AddressType.PlaceOfBusinessAddress);
                }
                return
                        db.Addresses.Include("UserCity.CityCounty")
                            .Include("UserCountry")
                            .First(a => a.UserId == userId);
            }
        }

        public static List<Address> GetUserAddresses(string userId)
        {
            using (var db = new AppDbContext())
            {
                if (!db.Addresses.Any(a => a.UserId == userId))
                {
                    return null;
                }

                return
                    db.Addresses.Include("UserCity.CityCounty")
                        .Include("UserCountry")
                        .ToList();
            }
        }

        public static ApplicationUser GetProviderById(string providerId)
        {
            using (var db = new AppDbContext())
            {
                var provider = db.Users
                    .Include(x => x.Addresses)
                    .Include(x => x.Addresses.Select(a => a.UserCity))
                    .Include(x => x.Addresses.Select(a => a.UserCity.CityCounty))
                    .FirstOrDefault(u => u.Id == providerId);
                if (provider != null && provider.Addresses.Count > 1)
                {
                    var invoicingAddress = provider.Addresses.First(a => a.AddressType == AddressType.InvoiceAddress);
                    provider.Addresses.Remove(invoicingAddress);
                }

                return provider;
            }
        }

        public static ApplicationUser GetProviderBySlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.Include("Provider").First(s => s.Id == slotId).Provider;
            }
        }

        public static List<int> GetProviderCategories(string providerId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.Where(s => s.ProviderId == providerId).Select(s => s.CategoryId).Distinct().ToList();
            }
        }

        public static string GetProviderIdBySlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.Find(slotId).ProviderId;
            }
        }

        public static Rating GetRating(string userId, int slotId, string providerId)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    return slotId > 0
                        ? db.Ratings.First(f => f.SlotId == slotId && f.UserId == userId)
                        : db.Ratings.First(f => f.ProviderId == providerId && f.UserId == userId);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static Schedule GetScheduleById(int id)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.Schedules.Include("User")
                        .Include("Slot.Provider")
                        .Include("Slot.Category")
                        .FirstOrDefault(i => i.Id == id);
            }
        }

        public static Slot GetSlotById(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.Include("User").Include("Provider").First(s => s.Id == slotId);
            }
        }

        public static Slot GetSlotBySlotUserId(string userId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.First(s => s.UserId == userId);
            }
        }

        public static int GetSlotIdByUserId(string userId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.First(p => p.UserId == userId).Id;
            }
        }

        public static string GetSlotName(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return db.Slots.Find(slotId).Name;
            }
        }

        public static ApplicationUser GetUserById(string userId)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users
                    .Include("Addresses")
                    .Include("Agent")
                    .Include("Addresses.UserCity")
                    .Include("Addresses.UserCity.CityCounty")
                    .FirstOrDefault(u => u.Id == userId);

                return user;
            }
        }

        public static ApplicationUser GetUserByPhoneNumber(string phoneNumber)
        {
            using (var db = new AppDbContext())
            {
                if (db.Users.Any(a => a.PhoneNumber == phoneNumber))
                    return db.Users.First(a => a.PhoneNumber == phoneNumber);
            }
            return null;
        }

        public static string GetUserName(string userId)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(userId);

                if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                    return user.FirstName + " " + user.LastName;
                return user.CompanyDisplayName ?? user.Email;
            }
        }

        public static bool HasProgrammingPerSlot(string providerId)
        {
            using (var db = new AppDbContext())
            {
                return db.Users.Find(providerId).ProgrammingPerSlot;
            }
        }

        public static bool IsCityIdValid(int cityId)
        {
            using (var db = new AppDbContext())
            {
                var city = db.Cities.Find(cityId);

                return city != null;
            }
        }

        public static bool IsFavourite(string userId, int slotId, string providerId)
        {
            using (var db = new AppDbContext())
            {
                return slotId > 0
                    ? db.Favorites.Any(f => f.FavoriteSlotId == slotId && f.UserId == userId)
                    : db.Favorites.Any(f => f.FavoriteUserId == providerId && f.UserId == userId);
            }
        }

        public static int IsSchedulePossible(Schedule schedule)
        {
            var schedules = RetrieveLists.GetClientSchedulesByDate(schedule.UserId, schedule.ScheduleDateTimeStart);

            foreach (var s in schedules)
            {
                if (s.ScheduleDateTimeStart <= schedule.ScheduleDateTimeStart &&
                    s.ScheduleDateTimeEnd >= schedule.ScheduleDateTimeStart)
                    return 1; // overlapping schedule

                //if (s.SlotId == schedule.SlotId)
                //    return 2; // more than one schedule at a provider in the same day
            }

            return 0; // Schedule possible
        }

        public static int NoSchedulesByDate(DateTime day)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.Schedules.Count(
                        d =>
                            d.CreatedDateTime.Day == day.Day && d.CreatedDateTime.Month == day.Month &&
                            d.CreatedDateTime.Year == day.Year);
            }
        }

        public static int NoSchedulesFromNowOnByUserId(string userId)
        {
            using (var db = new AppDbContext())
            {
                var date = DateTime.Now;
                return
                    db.Schedules.Count(d =>
                        d.UserId == userId
                        && (d.State == ScheduleState.Pending || d.State == ScheduleState.Valid)
                        && d.ScheduleDateTimeStart > date);
            }
        }

        public static void RemoveFavorite(string userId, string providerId, int? slotId)
        {
            using (var db = new AppDbContext())
            {
                Favorite favorite;
                if (slotId.HasValue)
                {
                    favorite = db.Favorites.FirstOrDefault(x => x.UserId == userId && x.FavoriteUserId == providerId && x.FavoriteSlotId == slotId.Value);
                }
                else
                {
                    favorite = db.Favorites.FirstOrDefault(x => x.UserId == userId && x.FavoriteUserId == providerId && x.FavoriteSlotId == null);
                }

                if (favorite != null)
                {
                    db.Favorites.Remove(favorite);
                    db.SaveChanges();
                }
            }
        }

        public static InvoiceHeader GetInvoiceHeaderById(int id)
        {
            using (var db = new AppDbContext())
            {
                var header = db.InvoiceHeaders.Find(id);

                return header;
            }
        }

        #endregion Public Methods
    }
}