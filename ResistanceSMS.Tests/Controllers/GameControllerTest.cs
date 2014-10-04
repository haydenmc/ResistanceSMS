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
        public void TestLeaderSelect()
        {
            Game g = new Game()
            {
                CreateTime = DateTimeOffset.Now,
                GameState = Game.GameStates.Waiting,
                Players = new List<Player>()
                {
                    new Player() {
                        PlayerId = Guid.NewGuid()
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid()
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid()
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid()
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid()
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
