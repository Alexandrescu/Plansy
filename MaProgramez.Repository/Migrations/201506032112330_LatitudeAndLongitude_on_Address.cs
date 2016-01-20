namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LatitudeAndLongitude_on_Address : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Addresses", "Latitude", c => c.Double());
            AddColumn("dbo.Addresses", "Longitude", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Addresses", "Longitude");
            DropColumn("dbo.Addresses", "Latitude");
        }
    }
}
