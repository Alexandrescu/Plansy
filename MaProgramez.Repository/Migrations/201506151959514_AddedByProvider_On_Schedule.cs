namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedByProvider_On_Schedule : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Schedules", "AddedByProvider", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Schedules", "AddedByProvider");
        }
    }
}
