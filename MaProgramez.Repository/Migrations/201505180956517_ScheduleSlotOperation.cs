namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScheduleSlotOperation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SlotOperations", "Schedule_Id", "dbo.Schedules");
            DropIndex("dbo.SlotOperations", new[] { "Schedule_Id" });
            CreateTable(
                "dbo.ScheduleSlotOperations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        SlotOperationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Schedules", t => t.ScheduleId, cascadeDelete: false)
                .ForeignKey("dbo.SlotOperations", t => t.SlotOperationId, cascadeDelete: false)
                .Index(t => t.ScheduleId)
                .Index(t => t.SlotOperationId);
            
            DropColumn("dbo.SlotOperations", "Schedule_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SlotOperations", "Schedule_Id", c => c.Int());
            DropForeignKey("dbo.ScheduleSlotOperations", "SlotOperationId", "dbo.SlotOperations");
            DropForeignKey("dbo.ScheduleSlotOperations", "ScheduleId", "dbo.Schedules");
            DropIndex("dbo.ScheduleSlotOperations", new[] { "SlotOperationId" });
            DropIndex("dbo.ScheduleSlotOperations", new[] { "ScheduleId" });
            DropTable("dbo.ScheduleSlotOperations");
            CreateIndex("dbo.SlotOperations", "Schedule_Id");
            AddForeignKey("dbo.SlotOperations", "Schedule_Id", "dbo.Schedules", "Id");
        }
    }
}
