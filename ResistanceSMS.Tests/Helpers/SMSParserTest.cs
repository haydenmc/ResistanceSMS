using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceSMS.Tests.Helpers
{
	[TestClass]
	public class SMSParserTest
	{
		private ApplicationDbContext db;

		[ClassInitialize]
		public static void SetUp(TestContext context)
		{
			AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(context.DeploymentDirectory,String.Empty));
			Database.SetInitializer<ApplicationDbContext>(new DropCreateDatabaseAlways<ApplicationDbContext>());
		}

		[TestInitialize]
		public void LocalSetUp()
		{
			db = new ApplicationDbContext();
			db.Database.Initialize(force: true);
		}

		[TestMethod]
		public void CreateTest()
		{
			// Create a test player
			var player = new Player()
			{
				PlayerId = Guid.NewGuid(),
				Name = "PMcGriddle",
				PhoneNumber = "+12242120088",
				Wins = 0,
				Losses = 0,
				JoinTime = DateTimeOffset.Now,
				LastActivityTime = DateTimeOffset.Now
			};
			db.Players.Add(player);
			db.SaveChanges();

			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();
			parser.ParseStringInput(player,"Create");

			// Check to make sure that the game was created.
			var checkPlayer = db.Players.Where(x => x.PlayerId == player.PlayerId).First();
			var checkGames = db.Games.Select(x => x).ToList();
			Assert.IsNotNull(checkPlayer.CurrentGame, "'Create' command did not create game.");
			Assert.IsTrue(checkPlayer.CurrentGame.FriendlyId.Length > 0, "Create didn't generate a friendly name.");
		}
	}
}
