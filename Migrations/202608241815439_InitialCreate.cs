namespace J7Journal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Color = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TradeEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Symbol = c.String(nullable: false),
                        Direction = c.String(nullable: false),
                        EntryPrice = c.Decimal(nullable: false, precision: 18, scale: 4),
                        ExitPrice = c.Decimal(nullable: false, precision: 18, scale: 4),
                        ProfitLoss = c.Decimal(precision: 18, scale: 2),
                        TradeDate = c.DateTime(nullable: false),
                        DailyBias = c.String(),
                        HTFLiquidity = c.String(),
                        Confluences = c.String(),
                        ReasonForEntry = c.String(),
                        LessonLearned = c.String(),
                        Rating = c.Int(),
                        ScreenshotData = c.Binary(),
                        ContentType = c.String(),
                        SetupType = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TradeTags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TradeId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .ForeignKey("dbo.TradeEntries", t => t.TradeId, cascadeDelete: true)
                .Index(t => t.TradeId)
                .Index(t => t.TagId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TradeTags", "TradeId", "dbo.TradeEntries");
            DropForeignKey("dbo.TradeTags", "TagId", "dbo.Tags");
            DropIndex("dbo.TradeTags", new[] { "TagId" });
            DropIndex("dbo.TradeTags", new[] { "TradeId" });
            DropTable("dbo.TradeTags");
            DropTable("dbo.TradeEntries");
            DropTable("dbo.Tags");
        }
    }
}
