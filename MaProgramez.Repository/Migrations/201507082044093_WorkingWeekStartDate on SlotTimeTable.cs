namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkingWeekStartDateonSlotTimeTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SlotTimeTables", "WorkingWeekStartDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SlotTimeTables", "WorkingWeekStartDate");
        }
    }
}
