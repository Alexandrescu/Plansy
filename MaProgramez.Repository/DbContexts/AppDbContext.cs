using System.Data.Entity;
using MaProgramez.Repository.Entities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MaProgramez.Repository.DbContexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext()
            : base("DbContext")
        {
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Country> Countries{ get; set; }
        public DbSet<County> Counties{ get; set; }
        public DbSet<DefaultCategoryOperation> DefaultCategoryOperations { get; set; }
        public DbSet<DefaultNonWorkingDay> DefaultNonWorkingDays { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<InvoiceHeader> InvoiceHeaders { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<SlotNonWorkingDay> SlotNonWorkingDays { get; set; }
        public DbSet<SlotOperation> SlotOperations { get; set; }
        public DbSet<SlotTimeTable> SlotTimeTables { get; set; }
        public DbSet<ScheduleSlotOperation> ScheduleSlotOperations { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<AutomaticProcessingJob> AutomaticProcessingJobs { get; set; }
        public DbSet<AutomaticProcessingLog> AutomaticProcessingLogs { get; set; }
        public DbSet<CardTransaction> CardTransactions { get; set; }
        public DbSet<CardTransactionHistory> CardTransactionHistories { get; set; }

        #region PUBLIC METHODS

        public static AppDbContext Create()
        {
            return new AppDbContext();
        }

        #endregion
    }
}