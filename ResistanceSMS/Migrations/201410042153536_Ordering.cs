namespace ResistanceSMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ordering : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Players", "TurnOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Rounds", "RoundNumber", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Rounds", "RoundNumber");
            DropColumn("dbo.Players", "TurnOrder");
        }
    }
}
