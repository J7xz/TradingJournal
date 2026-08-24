namespace J7Journal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRiskRewardRatio : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradeEntries", "RiskRewardRatio", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TradeEntries", "RiskRewardRatio");
        }
    }
}
