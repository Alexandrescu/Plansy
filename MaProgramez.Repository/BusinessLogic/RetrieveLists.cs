using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Web.Security;
using System.Web.UI;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using MaProgramez.Repository.Utility;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MaProgramez.Repository.BusinessLogic
{
    public static class RetrieveLists
    {
        #region CATEGORIES

        /// <summary>
        /// Gets the categories filtered.
        /// </summary>
        /// <param name="parentCategoryId">The parent category identifier.</param>
        /// <param name="cityId">The city identifier.</param>
        /// <param name="countyId">The county identifier.</param>
        /// <param name="searchText">The search text.</param>
        /// <returns></returns>
        public static List<Category> GetCategoriesFiltered(int? parentCategoryId, int? cityId, string searchText = null)
        {
            using (var db = new AppDbContext())
            {
                //filter by parent category
                var users = new List<string>();

                if (cityId.HasValue && cityId > 0)
                {
                    users = db.Addresses.Where(x => x.CityId == cityId).Select(y => y.UserId).ToList();
                }

                var categories =
                    db.Categories.Include("Slots").Where(c => c.ParentCategoryId == parentCategoryId).ToList();

                if (cityId.HasValue && cityId > 0)
                {
                    foreach (var categ in categories.ToList())
                    {
                        if (categ.GetDescendants().Count(c => c.Slots.Any(s => users.Contains(s.ProviderId))) == 0)
                        {
                            categories.Remove(categ);
                        }
                    }
                }

                //filter by search text
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    categories = categories.Where(c => c.Name.ToLower().Contains(searchText.ToLower()) ||
                                                       c.Description.ToLower().Contains(searchText.ToLower()) ||
                                                       c.Slots.Any(s => s.Name.ToLower().Contains(searchText.ToLower()))
                        )
                        .ToList();
                }

                //Filter only categories where we have providers
                if (parentCategoryId.HasValue) //don't do this for main categories
                {
                    var categoriesWithProviders = db.Slots.Select(s => s.CategoryId).Distinct().ToList();

                    foreach (var categ in categories.ToList())
                    {
                        if (categ.GetDescendants().Count(c => categoriesWithProviders.Contains(c.Id)) == 0)
                        {
                            categories.Remove(categ);
                        }
                    }
                }

                return categories.OrderBy(c => c.Name).ToList();
            }
        }

        public static IEnumerable<Category> DescendantCategories(this Category root)
        {
            using (var db = new AppDbContext())
            {
                var nodes = new Stack<Category>(new[] {root});
                while (nodes.Any())
                {
                    Category node = nodes.Pop();
                    yield return node;

                    foreach (var n in db.Categories
                        .Include("Slots")
                        .Where(c => c.ParentCategoryId == node.Id))
                    {
                        nodes.Push(n);
                    }
                }
            }
        }

        public static bool IsLeaf(int categoryId)
        {
            using (var db = new AppDbContext())
            {
                return db.Categories.Find(categoryId).IsLeaf;
                //return !db.Categories.Any(c => c.ParentCategoryId == categoryId);
            }
        }

        public static bool IsPending(int categoryId)
        {
            using (var db = new AppDbContext())
            {
                return db.Categories.Find(categoryId).IsPending;
                //return !db.Slots.Any(c => c.CategoryId == categoryId);
            }
        }

        public static List<Category> GetAllSubCategories()
        {
            using (var db = new AppDbContext())
            {
                var categories = db.Categories.Where(c => c.ParentCategoryId != null).OrderBy(c => c.Name).ToList();

                //foreach (var cat in categories)
                //{
                //    cat.IsLeaf = IsLeaf(cat.Id);
                //    cat.IsPending = IsPending(cat.Id);
                //}

                return categories;
            }
        }

        public static Dictionary<int, string> GetAllSubCategoriesDictionary()
        {
            using (var db = new AppDbContext())
            {
                var categories =
                    db.Categories.Where(c => c.ParentCategoryId != null)
                        .OrderBy(c => c.Name)
                        .ToDictionary(c => c.Id, c => c.Name);
                return categories;
            }
        }

        public static List<Category> GetCategoryByKeyword(string keyword)
        {
            using (var db = new AppDbContext())
            {
                var categories =
                    db.Categories.Where(c => c.Name.Contains(keyword) || c.Description.Contains(keyword))
                        .OrderBy(c => c.Name)
                        .ToList();

                //foreach (var cat in categories)
                //{
                //    cat.IsLeaf = IsLeaf(cat.Id);
                //    cat.IsPending = IsPending(cat.Id);
                //}

                return categories;
            }
        }

        public static List<Category> GetSubCategoriesByCategory(int categoryId)
        {
            using (var db = new AppDbContext())
            {
                var categories =
                    db.Categories.Where(c => c.ParentCategoryId == categoryId).OrderBy(c => c.Name).ToList();

                return categories;
            }
        }

        #endregion

        #region PROVIDERS

        public static List<ApplicationUser> GetClientsByProvider(string providerId, int? slotId)
        {
            using (var db = new AppDbContext())
            {
                return slotId.HasValue
                    ? db.Schedules.Where(s => s.SlotId == slotId)
                        .OrderByDescending(y => y.ScheduleDateTimeStart)
                        .Select(x => x.User).Distinct()
                        .ToList()
                    : db.Schedules.Where(s => s.Slot.ProviderId == providerId)
                        .OrderByDescending(y => y.ScheduleDateTimeStart)
                        .Select(x => x.User).Distinct()
                        .ToList();
            }
        }

        public static List<ApplicationUser> GetProvidersByCategory(int categoryId)
        {
            using (var db = new AppDbContext())
            {
                var roleId = db.Roles.First(r => r.Name == "Provider").Id;

                var providers =
                    (from s in db.Slots where s.CategoryId == categoryId select s.ProviderId).ToList().Distinct();

                return
                    (from u in db.Users
                        where u.Roles.Any(t => t.RoleId == roleId) && providers.Contains(u.Id)
                        select u).OrderBy(p => p.CompanyName).ToList();
            }
        }

        public static List<ApplicationUser> GetProvidersByCategoryAndCity(int categoryId, int cityId)
        {
            using (var db = new AppDbContext())
            {
                var providers =
                    (from s in db.Slots where s.CategoryId == categoryId select s.ProviderId).ToList().Distinct();

                return
                    db.Users.Where(
                        u =>
                            providers.Contains(u.Id) &&
                            (
                                (u.Addresses.Count > 1 &&
                                 u.Addresses.Any(
                                     a => a.AddressType == AddressType.PlaceOfBusinessAddress && a.CityId == cityId))
                                ||
                                (u.Addresses.Count == 1 &&
                                 u.Addresses.Any(a => a.CityId == cityId))
                                )
                        )
                        .OrderBy(p => p.CompanyName)
                        .ToList();
            }
        }

        public static List<ApplicationUser> GetFavoriteProviders(string userId)
        {
            using (var db = new AppDbContext())
            {
                return (from d in db.Favorites
                    .Include("FavoriteUser")
                    where d.UserId == userId
                    select d.FavoriteUser)
                    .OrderBy(p => p.CompanyName).ToList();
            }
        }

        public static List<Favorite> GetFavorites(string userId)
        {
            using (var db = new AppDbContext())
            {
                return (from d in db.Favorites
                    .Include("FavoriteUser")
                    .Include("FavoriteSlot")
                    .Include("FavoriteSlot.Category")
                    where d.UserId == userId
                    select d)
                    .OrderBy(f => f.FavoriteUser.CompanyName).ToList();
            }
        }

        public static List<ApplicationUser> GetProvidersFiltered(int categoryId, int? cityId, int page,
            int pageSize = 10, string searchText = null)
        {
            using (var db = new AppDbContext())
            {
                var providers = db.Slots.Where(s => s.CategoryId == categoryId).Select(s => s.Provider);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    providers = providers.Where(p => p.CompanyName.Contains(searchText) ||
                                                     p.Alias.Contains(searchText) ||
                                                     p.FullDescription.Contains(searchText) ||
                                                     p.FirstName.Contains(searchText) ||
                                                     p.LastName.Contains(searchText)
                        );
                }

                if (cityId.HasValue)
                {
                    providers = providers.Where(p => p.Addresses
                        .Where(
                            a =>
                                a.AddressType ==
                                (p.Addresses.Count > 1 ? AddressType.PlaceOfBusinessAddress : AddressType.InvoiceAddress))
                        .Any(a => a.CityId == cityId));
                }

                var providerList =
                    providers.Include("Addresses.UserCity.CityCounty").Include("Addresses.UserCountry").ToList();

                foreach (var provider in providerList)
                {
                    if (provider.Addresses == null || provider.Addresses.Count <= 1) continue;

                    var invoicingAddress = provider.Addresses.First(a => a.AddressType == AddressType.InvoiceAddress);
                    provider.Addresses.Remove(invoicingAddress);
                }

                return
                    providerList.OrderBy(p => p.CompanyName)
                        .Distinct()
                        .ToList()
                        .Skip(page*pageSize)
                        .Take(pageSize)
                        .ToList();
            }
        }

        public static List<City> GetCitiesWithProviders(int? categoryId)
        {
            using (var db = new AppDbContext())
            {
                var roleId = db.Roles.First(r => r.Name == "Provider").Id;
                var providers =
                    (from u in db.Users where u.Roles.Any(t => t.RoleId == roleId) select u.UserName).ToList();

                var categories = categoryId.HasValue
                    ? (from c in db.Slots
                        where c.CategoryId == categoryId || c.Category.ParentCategoryId == categoryId
                        select c.ProviderId).ToList().Distinct()
                    : (from c in db.Slots select c.ProviderId).ToList().Distinct();

                return
                    (from a in db.Addresses
                        where providers.Contains(a.User.UserName) && categories.Contains(a.UserId)
                        select a.UserCity).OrderBy(c => c.Name).Distinct()
                        .ToList();
            }
        }

        public static List<ApplicationUser> GetUsersInRole(string role)
        {
            using (var db = new AppDbContext())
            {
                var roleId = db.Roles.First(r => r.Name == role).Id;
                return
                    (from u in db.Users where u.Roles.Any(t => t.RoleId == roleId) select u).OrderByDescending(
                        u => u.CreatedDate).ToList();
            }
        }

        #endregion

        #region SLOTS

        public static List<Slot> GetSlotsByProvider(string providerUserId, int? categoryId = null)
        {
            using (var db = new AppDbContext())
            {
                categoryId = categoryId.HasValue && categoryId.Value > 0 ? categoryId : null;
                var slots = db.Slots.Include("Provider").Include("User")
                    .Include("Category").Include("SlotOperations")
                    .Where(s => s.ProviderId == providerUserId);

                if (categoryId.HasValue)
                {
                    slots = slots.Where(s => s.CategoryId == categoryId);
                }

                return slots.OrderBy(s => s.Name).ToList();
            }
        }

        public static List<int> GetSlotsIdsByProvider(string providerUserId, int? categoryId = null)
        {
            using (var db = new AppDbContext())
            {
                var slots = db.Slots
                    .Where(s => s.ProviderId == providerUserId);

                if (categoryId.HasValue)
                {
                    slots = slots.Where(s => s.CategoryId == categoryId);
                }

                return slots.Select(s => s.Id).ToList();
            }
        }

        #endregion

        #region OPERATIONS

        public static List<SlotOperation> GetSlotOperationsByIds(List<int> ids, int slotId)
        {
            using (var db = new AppDbContext())
            {
                return db.SlotOperations.Include("Operation")
                    .Where(s => s.SlotId == slotId && ids.Contains(s.OperationId))
                    .DistinctBy(x => x.OperationId)
                    .OrderBy(so => so.Operation.Description)
                    .ToList();
            }
        }

        public static List<SlotOperation> GetSlotOperationsBySlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.SlotOperations.Include("Operation.Category")
                        .Where(s => s.SlotId == slotId)
                        .OrderBy(so => so.Operation.Description)
                        .ToList();
            }
        }

        public static List<SlotOperation> GetSlotOperationsByProvider(string providerId)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.SlotOperations.Include("Operation.Category")
                        .Where(s => s.Slot.ProviderId == providerId)
                        .OrderBy(so => so.Operation.Description)
                        .ToList();
            }
        }

        public static List<Operation> GetOperationsByCategory(int categoryId)
        {
            using (var db = new AppDbContext())
            {
                return db.Operations.Where(o => o.CategoryId == categoryId)
                    .OrderBy(o => o.Description)
                    .ToList();
            }
        }

        public static List<DefaultCategoryOperation> GetDefaultCategoryOperations(string categoryName)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.DefaultCategoryOperations.Include("Operation")
                        .Include("Category")
                        .Where(o => o.Category.Name == categoryName)
                        .OrderBy(o => o.Operation.Description)
                        .ToList();
            }
        }

        public static List<DefaultCategoryOperation> GetAllDefaultCategoryOperations()
        {
            using (var db = new AppDbContext())
            {
                return
                    db.DefaultCategoryOperations.Include("Operation")
                        .Include("Category")
                        .OrderBy(o => o.Operation.Description)
                        .ToList();
            }
        }

        /// <summary>
        /// Gets the provider slot operations.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="selectedOperations">The selected operations.</param>
        /// <param name="selectedDate">The selected date.</param>
        /// <param name="selectedHour">The selected hour.</param>
        /// <param name="selectedSlotId">The selected slot identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ignoreTimetable">If the provider wants to make schedules anytime.</param>
        /// <returns></returns>
        public static ProviderOperationsResult GetProviderSlotOperations(string providerId, int categoryId,
            List<int> selectedOperations,
            DateTime? selectedDate, int? selectedHour, int? selectedSlotId, string userId, bool ignoreTimetable = false)
        {
            var providerOperationsResult = new ProviderOperationsResult();

            using (var db = new AppDbContext())
            {
                if (!selectedDate.HasValue || selectedDate.Value.Date == DateTime.Today)
                {
                    selectedDate = DateTime.Now.AddHours(1);
                }

                var slotOperations =
                    db.SlotOperations
                        .Include("Operation.Category")
                        .Include("Slot.SlotOperations")
                        .Include("Slot.Provider")
                        .Include("Slot.User")
                        .Where(s => s.Slot.ProviderId == providerId &&
                                    s.Operation.CategoryId == categoryId)
                        .ToList();

                var slots = slotOperations.Select(o => o.Slot).Distinct();

                //if (selectedSlotId.HasValue)
                //{
                //    slotOperations = slotOperations.Where(so => so.SlotId == selectedSlotId).ToList();
                //    //slots = slots.Where(s => s.Id == selectedSlotId);
                //}

                if (selectedOperations != null && selectedOperations.Any())
                {
                    //Get only slots that can do all selected operations
                    var slotsThatPerformAllSelectedOperations = new List<int>();

                    foreach (var slot in slots)
                    {
                        if (selectedOperations.All(so => slot.SlotOperations.Select(o => o.OperationId).Contains(so)))
                        {
                            slotsThatPerformAllSelectedOperations.Add(slot.Id);
                        }

                        if (!string.IsNullOrWhiteSpace(slot.UserId))
                        {
                            var slotAddres = RetrieveOthers.GetUserAddress(slot.UserId);
                            if (slotAddres != null)
                            {
                                slot.FullAddress = slotAddres.ToString();
                            }
                        }
                    }

                    slotOperations =
                        slotOperations.Where(so => slotsThatPerformAllSelectedOperations.Contains(so.SlotId)).ToList();

                    slots = slots.Where(s => slotsThatPerformAllSelectedOperations.Contains(s.Id));
                }

                // FILL IN THE RESPONSE

                //available slots
                providerOperationsResult.AvailableSlots = slots.Distinct().ToList();

                // available operations
                providerOperationsResult.AvailableOperations = slotOperations
                    .OrderBy(so => so.Operation.Description)
                    .Select(so => so.Operation)
                    .Distinct()
                    .ToList();
                //Set price and duration
                foreach (var operation in providerOperationsResult.AvailableOperations)
                {
                    var slotOp =
                        providerOperationsResult.AvailableSlots.First(
                            s => s.SlotOperations.Any(so => so.OperationId == operation.Id))
                            .SlotOperations.First(so => so.OperationId == operation.Id);

                    if (slotOp == null) continue;

                    operation.Price = slotOp.Price;
                    operation.DurationMinutes = slotOp.DurationMinutes;
                }

                //selected operations
                providerOperationsResult.SelectedOperationIds = selectedOperations; //send selection back to screens
                if (selectedOperations != null && selectedOperations.Any())
                {
                    foreach (var operation in providerOperationsResult.AvailableOperations)
                    {
                        operation.Selected = selectedOperations.Contains(operation.Id);
                    }
                }

                var availableHours = new List<DateTime>();

                if (providerOperationsResult.AvailableOperations.Any() &&
                    selectedOperations != null
                    && selectedOperations.Any())
                {
                    while (!availableHours.Any())
                    {
                        foreach (var slotId in slotOperations.Select(so => so.SlotId).Distinct())
                        {
                            var slotAvailableHours =
                                RetrieveOthers.GetAvailableHours(
                                    slotId,
                                    slotOperations.Where(s => s.SlotId == slotId &&
                                                              selectedOperations.Contains(s.OperationId)).ToList(),
                                    selectedDate.Value,
                                    userId, ignoreTimetable);

                            slotAvailableHours.ForEach(h =>
                            {
                                if (!availableHours.Contains(h))
                                {
                                    availableHours.Add(h);
                                }
                            });
                        }

                        if (!availableHours.Any())
                        {
                            selectedDate = selectedDate.Value.AddDays(1).Date;
                        }
                    }
                }

                availableHours.Sort();

                if (availableHours.Any())
                {
                    //first available date
                    providerOperationsResult.FirstAvailableDate = availableHours.First().Date.ToString("dd-MM-yyyy");


                    //Available hours
                    providerOperationsResult.AvailableHours =
                        availableHours.Select(ah => Convert.ToInt32(ah.ToString("HH"))).Distinct().ToList();

                    //Available minutes for selected hour
                    if (selectedHour.HasValue)
                    {
                        providerOperationsResult.AvailableMinutesForSelectedHour =
                            availableHours.Where(ah => Convert.ToInt32(ah.ToString("HH")) == selectedHour)
                                .Select(t => t.Minute)
                                .Distinct()
                                .ToList();

                    }
                    else
                    {
                        var firstHour = availableHours.OrderBy(ah => ah.Hour).First();

                        providerOperationsResult.AvailableMinutesForSelectedHour =
                            availableHours.Where(ah => Convert.ToInt32(ah.ToString("HH")) == firstHour.Hour)
                                .Select(t => t.Minute)
                                .Distinct()
                                .ToList();
                    }
                }
                else
                {
                    providerOperationsResult.FirstAvailableDate = DateTime.Now.ToString("dd-MM-yyyy");
                }

                return providerOperationsResult;
            }
        }

        public static List<SlotOperation> GetOperationsBySchedule(int scheduleId)
        {
            using (var db = new AppDbContext())
            {
                return db.ScheduleSlotOperations
                    .Where(o => o.ScheduleId == scheduleId)
                    .Select(x => x.SlotOperation).Include("Operation")
                    .OrderBy(o => o.Operation.Description)
                    .ToList();
            }
        }

        #endregion

        #region SCHEDULES

        public static List<Schedule> GetAllClientSchedules(string clientId, bool showHistory = true,
            bool showCurrent = true)
        {
            using (var db = new AppDbContext())
            {
                var schedules =
                    db.Schedules.Include("User").Include("Slot").Include("Slot.Provider")
                        .Where(s => s.UserId == clientId &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser);

                var now = DateTime.Now;

                if (!showHistory)
                {
                    schedules = schedules.Where(s => s.ScheduleDateTimeStart >= now);
                }

                if (!showCurrent)
                {
                    schedules = schedules.Where(s => s.ScheduleDateTimeStart < now);
                }

                foreach (var schedule in schedules.ToList())
                {
                    var operations =
                        db.ScheduleSlotOperations.Include(x => x.SlotOperation.Operation)
                            .Where(so => so.ScheduleId == schedule.Id);
                    schedule.OperationsForSchedule = operations.Select(o => o.SlotOperation.Operation).ToList();

                    foreach (var op in schedule.OperationsForSchedule)
                    {
                        var price = operations.First(o => o.SlotOperation.OperationId == op.Id).SlotOperation.Price;
                        op.Price = price;
                    }
                }

                return schedules.OrderByDescending(s => s.ScheduleDateTimeStart).ToList();
            }
        }

        public static List<Schedule> GetClientSchedulesByDate(string clientId, DateTime date)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.Schedules.Include("User")
                        .Where(
                            s =>
                                s.UserId == clientId &&
                                s.State != ScheduleState.CancelledByProvider &&
                                s.State != ScheduleState.CancelledByUser &&
                                s.ScheduleDateTimeStart.Day == date.Day &&
                                s.ScheduleDateTimeStart.Month == date.Month &&
                                s.ScheduleDateTimeStart.Year == date.Year)
                        .OrderByDescending(s => s.ScheduleDateTimeStart)
                        .ToList();
            }
        }

        public static int GetClientSchedulesCountByWeek(string clientId, DateTime date)
        {
            using (var db = new AppDbContext())
            {
                var startDate = date.StartOfWeek(DayOfWeek.Monday);
                var endDate = startDate.AddDays(7);
                return
                    db.Schedules
                        .Count(
                            s =>
                                s.UserId == clientId &&
                                s.State != ScheduleState.CancelledByProvider &&
                                s.State != ScheduleState.CancelledByUser &&
                                s.ScheduleDateTimeStart > startDate &&
                                s.ScheduleDateTimeEnd < endDate);
            }
        }

        public static List<Schedule> GetClientNextFiveSchedules(string clientId)
        {
            using (var db = new AppDbContext())
            {
                var date = DateTime.Now;

                return
                    db.Schedules.Include("Slot.Provider")
                        .Where(
                            s =>
                                s.UserId == clientId &&
                                s.ScheduleDateTimeStart > date &&
                                s.State != ScheduleState.CancelledByProvider &&
                                s.State != ScheduleState.CancelledByUser)
                        .OrderBy(t => t.ScheduleDateTimeStart)
                        .Take(5)
                        .ToList();
            }
        }

        public static List<Schedule> GetClientLastFiveSchedules(string clientId)
        {
            using (var db = new AppDbContext())
            {
                var date = DateTime.Now;

                return
                    db.Schedules.Include("Slot.Provider")
                        .Where(
                            s =>
                                s.UserId == clientId &&
                                s.ScheduleDateTimeStart < date &&
                                s.State != ScheduleState.CancelledByProvider &&
                                s.State != ScheduleState.CancelledByUser)
                        .OrderByDescending(t => t.ScheduleDateTimeStart)
                        .Take(5)
                        .ToList();
            }
        }

        public static List<Schedule> GetProvidersNextFiveSchedules(string providerId, int? slotId)
        {
            using (var db = new AppDbContext())
            {
                var date = DateTime.Now;

                if (slotId.HasValue)
                {
                    return
                        db.Schedules.Include("User")
                            .Where(
                                s =>
                                    s.SlotId == (int) slotId &&
                                    s.ScheduleDateTimeStart > date &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser)
                            .OrderBy(t => t.ScheduleDateTimeStart)
                            .Take(5)
                            .ToList();
                }
                else
                {
                    return
                        db.Schedules.Include("User")
                            .Where(
                                s =>
                                    s.Slot.ProviderId == providerId &&
                                    s.ScheduleDateTimeStart > date &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser)
                            .OrderBy(t => t.ScheduleDateTimeStart)
                            .Take(5)
                            .ToList();
                }
            }
        }

        public static List<Schedule> GetProvidersLastFiveSchedules(string providerId, int? slotId)
        {
            using (var db = new AppDbContext())
            {
                var date = DateTime.Now;

                if (slotId.HasValue)
                {
                    return
                        db.Schedules.Include("User")
                            .Where(
                                s =>
                                    s.SlotId == (int) slotId &&
                                    s.ScheduleDateTimeStart < date &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser)
                            .OrderByDescending(t => t.ScheduleDateTimeStart)
                            .Take(5)
                            .ToList();
                }
                else
                {
                    return
                        db.Schedules.Include("User")
                            .Where(
                                s =>
                                    s.Slot.ProviderId == providerId &&
                                    s.ScheduleDateTimeStart < date &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser)
                            .OrderByDescending(t => t.ScheduleDateTimeStart)
                            .Take(5)
                            .ToList();
                }
            }
        }

        public static List<Schedule> GetSchedulesByProviderSlot(int slotId, bool pending = false)
        {
            using (var db = new AppDbContext())
            {
                if (!pending)
                    return
                        db.Schedules.Include("User").Where(s => s.SlotId == slotId &&
                                                                s.State != ScheduleState.CancelledByProvider &&
                                                                s.State != ScheduleState.CancelledByUser)
                            .OrderByDescending(s => s.ScheduleDateTimeStart)
                            .ToList();

                return
                    db.Schedules.Include("User").Where(s => s.SlotId == slotId && s.State == ScheduleState.Pending)
                        .OrderByDescending(s => s.ScheduleDateTimeStart)
                        .ToList();
            }
        }

        public static List<Schedule> GetSchedulesByProvider(string providerId, bool pending = false)
        {
            using (var db = new AppDbContext())
            {
                if (!pending)
                    return
                        db.Schedules.Include("User").Include("Slot").Where(s => s.Slot.ProviderId == providerId &&
                                                                                s.State !=
                                                                                ScheduleState.CancelledByProvider &&
                                                                                s.State != ScheduleState.CancelledByUser)
                            .OrderByDescending(s => s.ScheduleDateTimeStart)
                            .ToList();

                return
                    db.Schedules.Include("User")
                        .Include("Slot")
                        .Where(s => s.Slot.ProviderId == providerId && s.State == ScheduleState.Pending)
                        .OrderByDescending(s => s.ScheduleDateTimeStart)
                        .ToList();
            }
        }

        public static List<Schedule> GetSchedulesByUser(string userId, bool pending = false, bool current = false)
        {
            using (var db = new AppDbContext())
            {

                var schedules = !pending
                    ? db.Schedules.Include("User").Include("Slot.Provider.Addresses.UserCity.CityCounty")
                        .Where(s => s.UserId == userId &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser)
                        .OrderByDescending(s => s.ScheduleDateTimeStart)
                    : db.Schedules.Include("User")
                        .Include("Slot.Provider")
                        .Where(s => s.UserId == userId && s.State == ScheduleState.Pending)
                        .OrderByDescending(s => s.ScheduleDateTimeStart);

                if (current)
                {
                    schedules =
                        schedules.Where(s => s.ScheduleDateTimeStart >= DateTime.Now)
                            .OrderByDescending(s => s.ScheduleDateTimeStart);
                }

                return schedules.ToList();
            }
        }

        public static List<Schedule> GetSchedulesByDateAndSlot(int slotId, DateTime date)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.Schedules.Include("User")
                        .Where(s => s.SlotId == slotId &&
                                    s.ScheduleDateTimeStart.Day == date.Day &&
                                    s.ScheduleDateTimeStart.Month == date.Month &&
                                    s.ScheduleDateTimeStart.Year == date.Year &&
                                    s.State != ScheduleState.CancelledByProvider &&
                                    s.State != ScheduleState.CancelledByUser)
                        .ToList();
            }
        }

        public static int GetSchedulesCountByProvider(string providerId, DateTime date, bool pending = false)
        {
            using (var db = new AppDbContext())
            {
                return pending
                    ? db.Schedules.Count(s => s.Slot.ProviderId == providerId && s.State == ScheduleState.Pending)
                    : db.Schedules.Count(s => s.Slot.ProviderId == providerId &&
                                              s.ScheduleDateTimeStart.Day == date.Day &&
                                              s.ScheduleDateTimeStart.Month == date.Month &&
                                              s.ScheduleDateTimeStart.Year == date.Year &&
                                              s.State != ScheduleState.CancelledByProvider &&
                                              s.State != ScheduleState.CancelledByUser);
            }
        }

        #endregion

        #region TIMETABLES

        public static List<SlotTimeTable> GetTimetablesBySlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.SlotTimeTables.Where(s => s.SlotId == slotId)
                        .OrderBy(d => d.DayOfWeek)
                        .ThenBy(h => h.StarTime)
                        .ToList();
            }
        }

        #endregion TIMETABLES

        #region NON_WORKING_DAYS

        public static List<DefaultNonWorkingDay> GetDefaultNonWorkingDays()
        {
            using (var db = new AppDbContext())
            {
                return db.DefaultNonWorkingDays.OrderBy(d => d.StartDateTime).ToList();
            }
        }

        public static List<DefaultNonWorkingDay> GetDefaultNonWorkingDaysForSlot(int slotId)
        {
            using (var db = new AppDbContext())
            {
                var list =
                    (from s in db.SlotNonWorkingDays where s.SlotId == slotId && s.IsWorkingDay select s.StartDateTime)
                        .ToList();
                return list.Any()
                    ? db.DefaultNonWorkingDays.Where(s => !list.Contains(s.StartDateTime))
                        .OrderBy(d => d.StartDateTime)
                        .ToList()
                    : GetDefaultNonWorkingDays();
            }
        }

        public static List<SlotNonWorkingDay> GetSlotNonWorkingDays(int slotId)
        {
            using (var db = new AppDbContext())
            {
                return
                    db.SlotNonWorkingDays.Where(s => s.SlotId == slotId && s.IsWorkingDay == false)
                        .OrderBy(d => d.StartDateTime)
                        .ToList();
            }
        }

        #endregion NON_WORKING_DAYS

        /// <summary>
        /// Gets the slot schedules count.
        /// </summary>
        /// <param name="slotIds">The slot ids.</param>
        /// <param name="isPending">if set to <c>true</c> return pending schedules.</param>
        /// <returns></returns>
        public static Dictionary<int, int> GetSlotSchedulesCount(List<int> slotIds, bool isPending = false)
        {
            var slotSchedulesCount = new Dictionary<int, int>();

            using (var db = new AppDbContext())
            {
                foreach (var slotId in slotIds)
                {
                    slotSchedulesCount.Add(slotId,
                        isPending
                            ? db.Schedules.Count(s => s.SlotId == slotId &&
                                                      s.State == ScheduleState.Pending)
                            : db.Schedules.Count(s => s.SlotId == slotId &&
                                                      s.State != ScheduleState.CancelledByProvider &&
                                                      s.State != ScheduleState.CancelledByUser));
                }
            }

            return slotSchedulesCount;
        }

        public static Dictionary<int, int> GetSlotClientsCount(List<int> slotIds, string providerId)
        {
            var slotClientsCount = new Dictionary<int, int>();

            foreach (var slotId in slotIds)
            {
                slotClientsCount.Add(slotId, RetrieveLists.GetClientsByProvider(providerId, slotId).Count());
            }

            return slotClientsCount;
        }

        public static IEnumerable<Address> GetUserAddresses(string userId)
        {
            using (var db = new AppDbContext())
            {
                if (userId.IsNullOrWhiteSpace() || !db.Addresses.Any(a => a.UserId == userId))
                {
                    return new List<Address>();
                }

                return
                    db.Addresses.Include("UserCity.CityCounty")
                        .Include("UserCountry")
                        .Where(a => a.UserId == userId);
            }
        }

        public static List<County> GetCounties()
        {
            using (var db = new AppDbContext())
            {
                return db.Counties.OrderBy(x => x.Name).ToList();
            }
        }

        public static List<InvoiceHeader> GetUnpaidInvoices(string userId)
        {
            using (var db = new AppDbContext())
            {
                return db.InvoiceHeaders.Include("InvoiceLines")
                    .Include("InvoicePayments")
                    .Where(
                        u =>
                            u.UserId == userId &&
                            (!u.InvoicePayments.Any() ||
                             u.InvoiceLines.Sum(y => y.Price) > u.InvoicePayments.Sum(y => y.Amount)))
                    .ToList();
            }
        }

        public static List<ApplicationUser> GetNewProvidersInTown(string userId)
        {
            var city = RetrieveOthers.GetUserAddress(userId);
            if (city == null) return null;

            using (var db = new AppDbContext())
            {
                var roleId = db.Roles.First(r => r.Name == "Provider").Id;
                var providers = (from u in db.Users where u.Roles.Any(t => t.RoleId == roleId) select u.Id).ToList();
                return
                    db.Addresses.Include("User")
                        .Where(x => x.CityId == city.CityId && providers.Contains(x.UserId))
                        .Select(x => x.User)
                        .OrderByDescending(y => y.CreatedDate)
                        .Take(5)
                        .ToList();
            }
        }
    }
}