using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResistanceSMS.Models;
using System.Data.Entity;
using System.IO;
using ResistanceSMS.Controllers;
using System.Collections.Generic;

namespace ResistanceSMS.Tests.Helpers
{
	[TestClass]
	public class SMSControllerTest
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
		public void TestSendToPlayer()
		{
			// Create a test player
			var player = new Player()
			{
				PlayerId = Guid.NewGuid(),
				Name = "PMcGriddle",
				PhoneNumber = "+18479878434",
				Wins = 0,
				Losses = 0,
				JoinTime = DateTimeOffset.Now,
				LastActivityTime = DateTimeOffset.Now
			};
			db.Players.Add(player);
			db.SaveChanges();

			var gc = new GameController(null);
			gc.SMSPlayer(player, "Hey DARREN SMELLS");
		}

		[TestMethod]
		public void TestSendToPlayerList()
		{
			List<Player> players = new List<Player>();
			players.Add(new Player()
			{
				PhoneNumber = "+14088009977"
			});
			players.Add(new Player()
			{
				PhoneNumber = "+17654306609"
			});
			players.Add(new Player()
			{
				PhoneNumber = "+15746011611"
			});
			players.Add(new Player()
			{
				PhoneNumber = "+18479878434"
			});
			var gc = new GameController(null);
			gc.SMSPlayerList(players, "Hey this is a test. Darren smells.");
		}
	}
}
