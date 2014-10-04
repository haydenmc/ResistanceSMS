using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResistanceSMS.Models;
using System.Data.Entity;
using System.IO;
using ResistanceSMS.Controllers;

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
				PhoneNumber = "+14088009977",
				Wins = 0,
				Losses = 0,
				JoinTime = DateTimeOffset.Now,
				LastActivityTime = DateTimeOffset.Now
			};
			db.Players.Add(player);
			db.SaveChanges();

			var gc = new GameController(null);
			gc.SMSPlayer(player, "Hey, this is a test!");
		}
	}
}
