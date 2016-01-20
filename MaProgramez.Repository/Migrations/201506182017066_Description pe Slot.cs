namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DescriptionpeSlot : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Slots", "Description", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Slots", "Description");
        }
    }
}
