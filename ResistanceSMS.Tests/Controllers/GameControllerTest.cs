using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResistanceSMS.Models;
using System.Collections.Generic;
using ResistanceSMS.Controllers;
using System.Linq;
using System.Data.Entity;
using System.IO;

namespace ResistanceSMS.Tests.Controllers
{
    [TestClass]
    public class GameControllerTest
    {
        private ApplicationDbContext db;

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(context.DeploymentDirectory, String.Empty));
            Database.SetInitializer<ApplicationDbContext>(new DropCreateDatabaseAlways<ApplicationDbContext>());
        }

        [TestInitialize]
        public void LocalSetUp()
        {
            db = new ApplicationDbContext();
            db.Database.Initialize(force: true);
        }

        [TestMethod]
        public void TestPassOrFail()
        {
            Game g = new Game()
            {
                CreateTime = DateTimeOffset.Now,
                GameState = Game.GameStates.Waiting,
                GameId = Guid.NewGuid(),
                Players = new List<Player>()
                {
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 0
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 1
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 2
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 3
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 4
                    }
                },
                Rounds = new List<Round>()
                {
                    new Round() {
                        RoundId = Guid.NewGuid(),
                        VoteMissionPass = new List<Player>(),
                        VoteMissionFail = new List<Player>()
                    }
                }
            };
            this.db.Games.Add(g);

            this.db.SaveChanges();
            //initialize things
            GameController gc = new GameController(g);
            var p = gc.ActiveGame.Players.Last();
            var voteF = gc.ActiveGame.RoundsOrdered.Last().VoteMissionFail;
            var voteP = gc.ActiveGame.RoundsOrdered.Last().VoteMissionPass;
            //test lists empty
            Assert.IsTrue(voteF.Count <= 0, "Fail not empty");
            Assert.IsTrue(voteP.Count <= 0, "Pass not empty");
            //test add player to vote pass
            gc.CheckPassOrFail(p, true);
            Assert.IsTrue(voteF.Count <= 0, "Fail not empty, 2");
            Assert.IsTrue(voteP.Contains(p), "Pass !contain player, 2");
            //test add player to vote fail/remove from false
            gc.CheckPassOrFail(p, false);
            Assert.IsTrue(voteF.Contains(p), "Fail not null, 3");
            Assert.IsTrue(voteP.Count <= 0, "Pass !contain player, 3");
            //test add player to vote pass/remove from true
            gc.CheckPassOrFail(p, true);
            Assert.IsTrue(voteF.Count <= 0, "Fail not null, 4");
            Assert.IsTrue(voteP.Contains(p), "Pass !contain player, 4");
        }

        [TestMethod]
        public void TestTeamAssignment()
        {
            Game g = new Game()
            {
                CreateTime = DateTimeOffset.Now,
                GameState = Game.GameStates.Waiting,
                Players = new List<Player>()
                {
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 0
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 1
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 2
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 3
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 4
                    }
                },
                Rounds = new List<Round>()
                {
                    new Round() {
                        RoundId = Guid.NewGuid()
                    }
                }
            };
            this.db.Games.Add(g);

            this.db.SaveChanges();

            GameController gc = new GameController(g);
            gc.AssignTeams();
            Assert.IsTrue(gc.ActiveGame.SpyPlayers.Count == 2);
            Assert.IsTrue(gc.ActiveGame.ResistancePlayers.Count == 3);
            Assert.IsTrue(gc.ActiveGame.Players.Count == 5);
            var playerList = gc.ActiveGame.Players.OrderBy(p => p.TurnOrder).ToList();
            for (int i = 0; i < playerList.Count; i++)
            {
                var player = playerList[i];
                Assert.IsTrue(player.TurnOrder == i, "Players are not in correct turn order.");
            }
        }
        
        [TestMethod]
        public void TestLeaderSelect()
        {
            Game g = new Game()
            {
                CreateTime = DateTimeOffset.Now,
                GameState = Game.GameStates.Waiting,
                Players = new List<Player>()
                {
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 0
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 1
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 2
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 3
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 4
                    }
                },
                Rounds = new List<Round>()
                {
                    new Round() {
                        RoundId = Guid.NewGuid()
                    }
                }
            };
            this.db.Games.Add(g);

            this.db.SaveChanges();

            GameController gc = new GameController(g);
            gc.AssignLeader();
            Assert.IsTrue(gc.ActiveGame.Rounds.Last().Leader.PlayerId == g.Players.First().PlayerId, "First player wasn't selected as leader!");
            gc.AssignLeader();
            Assert.IsTrue(gc.ActiveGame.Rounds.Last().Leader.PlayerId == g.Players.ElementAt(1).PlayerId, "Second player wasn't selected as next leader!");
        }
        [TestMethod]
        public void SelectMissionPlayersTest() {
            Player creator = new Player()
            {
                PlayerId = Guid.NewGuid(),
                Name = "Lucas",
                TurnOrder = 0
            };
            Game g = new Game()
            {
                Creator = creator,
                CreateTime = DateTimeOffset.Now,
                GameState = Game.GameStates.Waiting,
                Players = new List<Player>()
                {
                    creator,
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        Name = "Darren",
                        TurnOrder = 1
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        Name = "Hayden",
                        TurnOrder = 2
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        Name = "Corbin",
                        TurnOrder = 3
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        Name = "Matt",
                        TurnOrder = 4
                    }
                },
                Rounds = new List<Round>(),
                GameId = Guid.NewGuid()
            };
            this.db.Games.Add(g);
            this.db.SaveChanges();
            GameController gc = new GameController(g);
            gc.StateTransition(Game.GameStates.SelectMissionPlayers);
            Player leader = gc.ActiveGame.RoundsOrdered.Last().Leader;
            String[] missionPlayers = {"Hayden", "Lucas"};
            gc.SelectMissionPlayers(leader, missionPlayers);
            Assert.IsTrue(gc.ActiveGame.Rounds.Last().MissionPlayers.Count() == 2, "There is something wrong with selcting process!");
        }
    }
}
