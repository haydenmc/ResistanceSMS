namespace ResistanceSMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameRejections : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rounds", "NumRejections", c => c.Int(nullable: false));
            DropColumn("dbo.Rounds", "NumFailures");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Rounds", "NumFailures", c => c.Int(nullable: false));
            DropColumn("dbo.Rounds", "NumRejections");
        }
    }
}
