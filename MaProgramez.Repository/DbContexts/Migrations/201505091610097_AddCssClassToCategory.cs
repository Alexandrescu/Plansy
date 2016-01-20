namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCssClassToCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "CssClass", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "CssClass");
        }
    }
}
