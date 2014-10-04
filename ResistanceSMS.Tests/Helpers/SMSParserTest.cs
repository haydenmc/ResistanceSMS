﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public void BasicParserTest()
		{
			//Init variables
			// Create a test player
			var player = this.GeneratePlayer();

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			//Testing all with CREATE as it doesn't modify params
			//Test some inputs
			Assert.IsTrue(parser.ParseStringInput(player, "CREATESDFSDGDF"));
			Assert.IsTrue(parser.ParseStringInput(player, "cReATe:::::hi"));
			Assert.IsTrue(parser.ParseStringInput(player, "create@test:test"));
			Assert.IsTrue(parser.ParseStringInput(player, "create何"));
			Assert.IsTrue(parser.ParseStringInput(player, "create!@#$#$%#$%%^&^&^*&(*)(_)+"));
			Assert.IsTrue(parser.ParseStringInput(player, "create"));
			Assert.IsTrue(parser.ParseStringInput(player, "create!!!!@test:test"));

			Assert.IsFalse(parser.ParseStringInput(player, "CREATDFSDGDF"));
			Assert.IsFalse(parser.ParseStringInput(player, "cr"));
			Assert.IsFalse(parser.ParseStringInput(player, "COMMOND"));
			Assert.IsFalse(parser.ParseStringInput(player, "何"));		
		}

		[TestMethod]
		public void CreateTest()
		{
			//Init variables
			// Create a test player
			var player = this.GeneratePlayer();

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			//Asserts if the parser suceeded
			Assert.IsTrue(parser.ParseStringInput(player,"Create"));

			// Check to make sure that the game was created.
			var checkPlayer = db.Players.Where(x => x.PlayerId == player.PlayerId).First();
			var checkGames = db.Games.Select(x => x).ToList();
			Assert.IsNotNull(checkPlayer.CurrentGame, "'Create' command did not create game.");

			//Test Only ParseCreate
			Assert.IsTrue(parser.ParseCreate(player, new String[2]{"asdafe", "1230"}));
		}

		[TestMethod]
		public void JoinTest()
		{

		}

		[TestMethod]
		public void ReadyTest()
		{

		}

		[TestMethod]
		public void PutTest()
		{

		}

		[TestMethod]
		public void StatsTest()
		{

		}

		[TestMethod]
		public void HelpTest()
		{

		}

		//TODO: make it a random generator
		public Player GeneratePlayer()
		{
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

			return player;
		}
	}
}
