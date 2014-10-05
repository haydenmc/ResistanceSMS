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

            GameController gc = new GameController(g);
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Count <= 0, "Fail not empty");
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Count <= 0, "Pass not empty");
            gc.CheckPassOrFail(gc.ActiveGame.Players.Last(), true);
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Count <= 0, "Fail not empty, 2");
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Contains(g.Players.Last()), "Pass !contain player, 2");
            gc.CheckPassOrFail(gc.ActiveGame.Players.Last(), false);
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Contains(g.Players.Last()), "Fail not null, 3");
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Count <= 0, "Pass !contain player, 3");
            gc.CheckPassOrFail(gc.ActiveGame.Players.Last(), true);
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Count <= 0, "Fail not null, 4");
            Assert.IsTrue(gc.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Contains(g.Players.Last()), "Pass !contain player, 4");
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
    }
}
