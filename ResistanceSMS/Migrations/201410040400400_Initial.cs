namespace ResistanceSMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameId = c.Guid(nullable: false),
                        ResistanceScore = c.Int(nullable: false),
                        SpyScore = c.Int(nullable: false),
                        LastActivityTime = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateTime = c.DateTimeOffset(nullable: false, precision: 7),
                        GameStarted = c.Boolean(nullable: false),
                        Creator_PlayerId = c.Guid(),
                    })
                .PrimaryKey(t => t.GameId)
                .ForeignKey("dbo.Players", t => t.Creator_PlayerId)
                .Index(t => t.Creator_PlayerId);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        PlayerId = c.Guid(nullable: false),
                        PhoneNumber = c.String(),
                        Name = c.String(),
                        Wins = c.Int(nullable: false),
                        Losses = c.Int(nullable: false),
                        LastActivityTime = c.DateTimeOffset(nullable: false, precision: 7),
                        JoinTime = c.DateTimeOffset(nullable: false, precision: 7),
                        CurrentGame_GameId = c.Guid(),
                        Game_GameId = c.Guid(),
                        Game_GameId1 = c.Guid(),
                        Game_GameId2 = c.Guid(),
                        Round_RoundId = c.Guid(),
                        Round_RoundId1 = c.Guid(),
                        Round_RoundId2 = c.Guid(),
                        Round_RoundId3 = c.Guid(),
                        Round_RoundId4 = c.Guid(),
                        Game_GameId3 = c.Guid(),
                    })
                .PrimaryKey(t => t.PlayerId)
                .ForeignKey("dbo.Games", t => t.CurrentGame_GameId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .ForeignKey("dbo.Games", t => t.Game_GameId1)
                .ForeignKey("dbo.Games", t => t.Game_GameId2)
                .ForeignKey("dbo.Rounds", t => t.Round_RoundId)
                .ForeignKey("dbo.Rounds", t => t.Round_RoundId1)
                .ForeignKey("dbo.Rounds", t => t.Round_RoundId2)
                .ForeignKey("dbo.Rounds", t => t.Round_RoundId3)
                .ForeignKey("dbo.Rounds", t => t.Round_RoundId4)
                .ForeignKey("dbo.Games", t => t.Game_GameId3)
                .Index(t => t.CurrentGame_GameId)
                .Index(t => t.Game_GameId)
                .Index(t => t.Game_GameId1)
                .Index(t => t.Game_GameId2)
                .Index(t => t.Round_RoundId)
                .Index(t => t.Round_RoundId1)
                .Index(t => t.Round_RoundId2)
                .Index(t => t.Round_RoundId3)
                .Index(t => t.Round_RoundId4)
                .Index(t => t.Game_GameId3);
            
            CreateTable(
                "dbo.Rounds",
                c => new
                    {
                        RoundId = c.Guid(nullable: false),
                        NumFailures = c.Int(nullable: false),
                        MissionPassed = c.Boolean(nullable: false),
                        Game_GameId = c.Guid(),
                        Leader_PlayerId = c.Guid(),
                    })
                .PrimaryKey(t => t.RoundId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .ForeignKey("dbo.Players", t => t.Leader_PlayerId)
                .Index(t => t.Game_GameId)
                .Index(t => t.Leader_PlayerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Players", "Game_GameId3", "dbo.Games");
            DropForeignKey("dbo.Players", "Round_RoundId4", "dbo.Rounds");
            DropForeignKey("dbo.Players", "Round_RoundId3", "dbo.Rounds");
            DropForeignKey("dbo.Players", "Round_RoundId2", "dbo.Rounds");
            DropForeignKey("dbo.Players", "Round_RoundId1", "dbo.Rounds");
            DropForeignKey("dbo.Players", "Round_RoundId", "dbo.Rounds");
            DropForeignKey("dbo.Rounds", "Leader_PlayerId", "dbo.Players");
            DropForeignKey("dbo.Rounds", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.Players", "Game_GameId2", "dbo.Games");
            DropForeignKey("dbo.Players", "Game_GameId1", "dbo.Games");
            DropForeignKey("dbo.Players", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.Games", "Creator_PlayerId", "dbo.Players");
            DropForeignKey("dbo.Players", "CurrentGame_GameId", "dbo.Games");
            DropIndex("dbo.Rounds", new[] { "Leader_PlayerId" });
            DropIndex("dbo.Rounds", new[] { "Game_GameId" });
            DropIndex("dbo.Players", new[] { "Game_GameId3" });
            DropIndex("dbo.Players", new[] { "Round_RoundId4" });
            DropIndex("dbo.Players", new[] { "Round_RoundId3" });
            DropIndex("dbo.Players", new[] { "Round_RoundId2" });
            DropIndex("dbo.Players", new[] { "Round_RoundId1" });
            DropIndex("dbo.Players", new[] { "Round_RoundId" });
            DropIndex("dbo.Players", new[] { "Game_GameId2" });
            DropIndex("dbo.Players", new[] { "Game_GameId1" });
            DropIndex("dbo.Players", new[] { "Game_GameId" });
            DropIndex("dbo.Players", new[] { "CurrentGame_GameId" });
            DropIndex("dbo.Games", new[] { "Creator_PlayerId" });
            DropTable("dbo.Rounds");
            DropTable("dbo.Players");
            DropTable("dbo.Games");
        }
    }
}
