namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aliasonuserentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Alias", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Alias");
        }
    }
}
