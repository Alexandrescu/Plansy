namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class scheduleUpdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Schedules", "ProviderUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Schedules", new[] { "ProviderUserId" });
            AddColumn("dbo.Schedules", "SlotId", c => c.Int(nullable: false));
            CreateIndex("dbo.Schedules", "SlotId");
            AddForeignKey("dbo.Schedules", "SlotId", "dbo.Slots", "Id", cascadeDelete: true);
            DropColumn("dbo.Schedules", "ProviderUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Schedules", "ProviderUserId", c => c.String(maxLength: 128));
            DropForeignKey("dbo.Schedules", "SlotId", "dbo.Slots");
            DropIndex("dbo.Schedules", new[] { "SlotId" });
            DropColumn("dbo.Schedules", "SlotId");
            CreateIndex("dbo.Schedules", "ProviderUserId");
            AddForeignKey("dbo.Schedules", "ProviderUserId", "dbo.AspNetUsers", "Id");
        }
    }
}
