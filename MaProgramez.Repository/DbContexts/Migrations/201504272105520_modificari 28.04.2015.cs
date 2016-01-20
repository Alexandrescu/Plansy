namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modificari28042015 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Operations", "Slot_Id", "dbo.Slots");
            DropIndex("dbo.Operations", new[] { "Slot_Id" });
            //DropColumn("dbo.SlotOperations", "SlotId");
            //RenameColumn(table: "dbo.SlotOperations", name: "Slot_Id", newName: "SlotId");
            AddColumn("dbo.Schedules", "SlotOperationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Schedules", "SlotOperationId");
            AddForeignKey("dbo.Schedules", "SlotOperationId", "dbo.SlotOperations", "Id", cascadeDelete: false);
            //AddForeignKey("dbo.SlotOperations", "SlotId", "dbo.Slots", "Id", cascadeDelete: true);
            //DropColumn("dbo.Operations", "Slot_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Operations", "Slot_Id", c => c.Int());
            DropForeignKey("dbo.SlotOperations", "SlotId", "dbo.Slots");
            DropForeignKey("dbo.Schedules", "SlotOperationId", "dbo.SlotOperations");
            DropIndex("dbo.Schedules", new[] { "SlotOperationId" });
            DropColumn("dbo.Schedules", "SlotOperationId");
            RenameColumn(table: "dbo.SlotOperations", name: "SlotId", newName: "Slot_Id");
            AddColumn("dbo.SlotOperations", "SlotId", c => c.Int(nullable: false));
            CreateIndex("dbo.Operations", "Slot_Id");
            AddForeignKey("dbo.Operations", "Slot_Id", "dbo.Slots", "Id");
        }
    }
}
