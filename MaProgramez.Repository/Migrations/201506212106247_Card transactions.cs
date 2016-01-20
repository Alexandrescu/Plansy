namespace MaProgramez.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cardtransactions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardTransactionHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CardTransactionId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        RequestXmlContent = c.String(storeType: "xml"),
                        ResponseXmlContent = c.String(storeType: "xml"),
                        ErrorCode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CardTransactions", t => t.CardTransactionId, cascadeDelete: true)
                .Index(t => t.CardTransactionId);
            
            CreateTable(
                "dbo.CardTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(maxLength: 3),
                        Details = c.String(maxLength: 1000),
                        InvoiceId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        State = c.String(maxLength: 500),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InvoiceHeaders", t => t.InvoiceId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.InvoiceId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CardTransactions", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CardTransactionHistories", "CardTransactionId", "dbo.CardTransactions");
            DropForeignKey("dbo.CardTransactions", "InvoiceId", "dbo.InvoiceHeaders");
            DropIndex("dbo.CardTransactions", new[] { "UserId" });
            DropIndex("dbo.CardTransactions", new[] { "InvoiceId" });
            DropIndex("dbo.CardTransactionHistories", new[] { "CardTransactionId" });
            DropTable("dbo.CardTransactions");
            DropTable("dbo.CardTransactionHistories");
        }
    }
}
