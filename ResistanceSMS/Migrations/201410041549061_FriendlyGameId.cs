namespace ResistanceSMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FriendlyGameId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "FriendlyId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "FriendlyId");
        }
    }
}
