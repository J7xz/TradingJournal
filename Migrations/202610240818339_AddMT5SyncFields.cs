namespace J7Journal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMT5SyncFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradeEntries", "MT5Ticket", c => c.Long());
            AddColumn("dbo.TradeEntries", "Volume", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.TradeEntries", "MT5OpenTime", c => c.DateTime());
            AddColumn("dbo.TradeEntries", "MT5CloseTime", c => c.DateTime());
            AddColumn("dbo.TradeEntries", "ImportedFromMT5", c => c.Boolean(nullable: false));
            AddColumn("dbo.TradeEntries", "IsJournalComplete", c => c.Boolean(nullable: false));
            CreateIndex("dbo.TradeEntries", "MT5Ticket", unique: true, name: "IX_TradeEntries_MT5Ticket");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TradeEntries", "IX_TradeEntries_MT5Ticket");
            DropColumn("dbo.TradeEntries", "IsJournalComplete");
            DropColumn("dbo.TradeEntries", "ImportedFromMT5");
            DropColumn("dbo.TradeEntries", "MT5CloseTime");
            DropColumn("dbo.TradeEntries", "MT5OpenTime");
            DropColumn("dbo.TradeEntries", "Volume");
            DropColumn("dbo.TradeEntries", "MT5Ticket");
        }
    }
}
