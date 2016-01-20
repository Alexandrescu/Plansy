namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialcreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CountryId = c.Int(nullable: false),
                        AddressText = c.String(nullable: false, maxLength: 500),
                        PostalCode = c.String(nullable: false, maxLength: 50),
                        CityId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        AddressType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Cities", t => t.CityId, cascadeDelete: true)
                .ForeignKey("dbo.Countries", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId)
                .Index(t => t.CityId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserUniqueId = c.Int(nullable: false, identity: true),
                        LastName = c.String(maxLength: 500),
                        FirstName = c.String(maxLength: 500),
                        ComplaintsNumber = c.Int(nullable: false),
                        IsCompany = c.Boolean(nullable: false),
                        CompanyName = c.String(maxLength: 500),
                        Cui = c.String(maxLength: 50),
                        Jno = c.String(maxLength: 50),
                        CategoryId = c.Int(),
                        DateOfBirth = c.DateTime(),
                        AccountNumber = c.String(maxLength: 100),
                        Bank = c.String(maxLength: 100),
                        IdCardNo = c.String(maxLength: 50),
                        VatRate = c.Int(nullable: false),
                        LogoPath = c.String(maxLength: 1000),
                        AcceptsNotificationOnEmail = c.Boolean(nullable: false),
                        AcceptsNotificationOnSms = c.Boolean(nullable: false),
                        AcceptsPushNotifications = c.Boolean(nullable: false),
                        AgentId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedByUserId = c.Int(nullable: false),
                        AcceptedTermsFlag = c.Boolean(nullable: false),
                        ContractNo = c.Int(),
                        ContractDate = c.DateTime(),
                        ProgrammingPerSlot = c.Boolean(nullable: false),
                        FullDescription = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AgentId)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .Index(t => t.CategoryId)
                .Index(t => t.AgentId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        Description = c.String(maxLength: 1000),
                        ParentCategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.ParentCategoryId)
                .Index(t => t.ParentCategoryId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Siruta = c.Int(nullable: false),
                        Longitude = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Latitude = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Name = c.String(maxLength: 64),
                        Region = c.String(maxLength: 64),
                        CountyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Counties", t => t.CountyId, cascadeDelete: true)
                .Index(t => t.CountyId);
            
            CreateTable(
                "dbo.Counties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(maxLength: 2),
                        Name = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(maxLength: 2),
                        Name = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Banks",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Code = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Secret = c.String(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        ApplicationType = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        RefreshTokenLifeTime = c.Int(nullable: false),
                        AllowedOrigin = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Configs",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 100),
                        Value = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.DefaultCategoryOperations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        OperationId = c.Int(nullable: false),
                        DefaultDurationMinutes = c.Int(nullable: false),
                        DefaultPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: false)
                .ForeignKey("dbo.Operations", t => t.OperationId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.OperationId);
            
            CreateTable(
                "dbo.Operations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 100),
                        CategoryId = c.Int(nullable: false),
                        Slot_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Slots", t => t.Slot_Id)
                .Index(t => t.CategoryId)
                .Index(t => t.Slot_Id);
            
            CreateTable(
                "dbo.DefaultNonWorkingDays",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 300),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Destination = c.String(),
                        Title = c.String(),
                        Content = c.String(),
                        SendingDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Favorites",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        FavoriteUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.FavoriteUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.FavoriteUserId);
            
            CreateTable(
                "dbo.InvoiceHeaders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Series = c.String(maxLength: 20),
                        Number = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        DelegateName = c.String(maxLength: 100),
                        DelegateIdCardDetails = c.String(maxLength: 50),
                        TransportationVehicleDetails = c.String(maxLength: 50),
                        StornoInvoiceId = c.Int(),
                        UserId = c.String(maxLength: 128),
                        VatRate = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InvoiceHeaders", t => t.StornoInvoiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.StornoInvoiceId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.InvoiceLines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InvoiceHeaderId = c.Int(nullable: false),
                        LineDescription = c.String(maxLength: 500),
                        UnitOfMeasurement = c.String(maxLength: 10),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InvoiceHeaders", t => t.InvoiceHeaderId, cascadeDelete: true)
                .Index(t => t.InvoiceHeaderId);
            
            CreateTable(
                "dbo.InvoicePayments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InvoiceHeaderId = c.Int(nullable: false),
                        PaymentMethod = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Details = c.String(maxLength: 200),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InvoiceHeaders", t => t.InvoiceHeaderId, cascadeDelete: true)
                .Index(t => t.InvoiceHeaderId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Type = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Text = c.String(maxLength: 1000),
                        ScheduleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.RefreshTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Subject = c.String(nullable: false, maxLength: 50),
                        ClientId = c.String(nullable: false, maxLength: 50),
                        IssuedUtc = c.DateTime(nullable: false),
                        ExpiresUtc = c.DateTime(nullable: false),
                        ProtectedTicket = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Schedules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        ProviderUserId = c.String(maxLength: 128),
                        ScheduleDateTime = c.DateTime(nullable: false),
                        CreatedDateTime = c.DateTime(nullable: false),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ProviderUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.ProviderUserId);
            
            CreateTable(
                "dbo.SlotNonWorkingDays",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 300),
                        SlotId = c.Int(nullable: false),
                        IsWorkingDay = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Slots", t => t.SlotId, cascadeDelete: true)
                .Index(t => t.SlotId);
            
            CreateTable(
                "dbo.Slots",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        Name = c.String(maxLength: 100),
                        Email = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 15),
                        CategoryId = c.Int(nullable: false),
                        AcceptsNotificationOnEmail = c.Boolean(nullable: false),
                        AcceptsNotificationOnSms = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.SlotOperations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SlotId = c.Int(nullable: false),
                        OperationId = c.Int(nullable: false),
                        DurationMinutes = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Operations", t => t.OperationId, cascadeDelete: true)
                .ForeignKey("dbo.Slots", t => t.SlotId, cascadeDelete: false)
                .Index(t => t.SlotId)
                .Index(t => t.OperationId);
            
            CreateTable(
                "dbo.SlotTimeTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SlotId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        StarTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Slots", t => t.SlotId, cascadeDelete: true)
                .Index(t => t.SlotId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SlotTimeTables", "SlotId", "dbo.Slots");
            DropForeignKey("dbo.SlotOperations", "SlotId", "dbo.Slots");
            DropForeignKey("dbo.SlotOperations", "OperationId", "dbo.Operations");
            DropForeignKey("dbo.SlotNonWorkingDays", "SlotId", "dbo.Slots");
            DropForeignKey("dbo.Slots", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Operations", "Slot_Id", "dbo.Slots");
            DropForeignKey("dbo.Slots", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Schedules", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Schedules", "ProviderUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InvoiceHeaders", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InvoiceHeaders", "StornoInvoiceId", "dbo.InvoiceHeaders");
            DropForeignKey("dbo.InvoicePayments", "InvoiceHeaderId", "dbo.InvoiceHeaders");
            DropForeignKey("dbo.InvoiceLines", "InvoiceHeaderId", "dbo.InvoiceHeaders");
            DropForeignKey("dbo.Favorites", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Favorites", "FavoriteUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DefaultCategoryOperations", "OperationId", "dbo.Operations");
            DropForeignKey("dbo.Operations", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.DefaultCategoryOperations", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Addresses", "CountryId", "dbo.Countries");
            DropForeignKey("dbo.Addresses", "CityId", "dbo.Cities");
            DropForeignKey("dbo.Cities", "CountyId", "dbo.Counties");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Categories", "ParentCategoryId", "dbo.Categories");
            DropForeignKey("dbo.AspNetUsers", "AgentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Addresses", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.SlotTimeTables", new[] { "SlotId" });
            DropIndex("dbo.SlotOperations", new[] { "OperationId" });
            DropIndex("dbo.SlotOperations", new[] { "SlotId" });
            DropIndex("dbo.Slots", new[] { "CategoryId" });
            DropIndex("dbo.Slots", new[] { "UserId" });
            DropIndex("dbo.SlotNonWorkingDays", new[] { "SlotId" });
            DropIndex("dbo.Schedules", new[] { "ProviderUserId" });
            DropIndex("dbo.Schedules", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.InvoicePayments", new[] { "InvoiceHeaderId" });
            DropIndex("dbo.InvoiceLines", new[] { "InvoiceHeaderId" });
            DropIndex("dbo.InvoiceHeaders", new[] { "UserId" });
            DropIndex("dbo.InvoiceHeaders", new[] { "StornoInvoiceId" });
            DropIndex("dbo.Favorites", new[] { "FavoriteUserId" });
            DropIndex("dbo.Favorites", new[] { "UserId" });
            DropIndex("dbo.Operations", new[] { "Slot_Id" });
            DropIndex("dbo.Operations", new[] { "CategoryId" });
            DropIndex("dbo.DefaultCategoryOperations", new[] { "OperationId" });
            DropIndex("dbo.DefaultCategoryOperations", new[] { "CategoryId" });
            DropIndex("dbo.Cities", new[] { "CountyId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Categories", new[] { "ParentCategoryId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "AgentId" });
            DropIndex("dbo.AspNetUsers", new[] { "CategoryId" });
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropIndex("dbo.Addresses", new[] { "CityId" });
            DropIndex("dbo.Addresses", new[] { "CountryId" });
            DropTable("dbo.SlotTimeTables");
            DropTable("dbo.SlotOperations");
            DropTable("dbo.Slots");
            DropTable("dbo.SlotNonWorkingDays");
            DropTable("dbo.Schedules");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.RefreshTokens");
            DropTable("dbo.Notifications");
            DropTable("dbo.InvoicePayments");
            DropTable("dbo.InvoiceLines");
            DropTable("dbo.InvoiceHeaders");
            DropTable("dbo.Favorites");
            DropTable("dbo.Emails");
            DropTable("dbo.DefaultNonWorkingDays");
            DropTable("dbo.Operations");
            DropTable("dbo.DefaultCategoryOperations");
            DropTable("dbo.Configs");
            DropTable("dbo.Clients");
            DropTable("dbo.Banks");
            DropTable("dbo.Countries");
            DropTable("dbo.Counties");
            DropTable("dbo.Cities");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Categories");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Addresses");
        }
    }
}
