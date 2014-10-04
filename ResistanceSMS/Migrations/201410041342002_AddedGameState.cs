namespace ResistanceSMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedGameState : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "GameState", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "GameState");
        }
    }
}
