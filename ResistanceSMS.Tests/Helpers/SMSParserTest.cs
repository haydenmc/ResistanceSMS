﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResistanceSMS.Controllers;
using ResistanceSMS.Models;
using ResistanceSMS.Tests.Utils;
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
			var player = TestUtils.GeneratePlayerWithGame(this.db);

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
			var player = TestUtils.GeneratePlayerWithGame(this.db);

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
			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			//Checks empty params exception and empty first param
			Boolean exceptionChecked = false;
			try
			{
				parser.ParseStringInput(player, "Join");
			}
			catch (Exception e)
			{
				exceptionChecked = true;
			}
			Assert.IsTrue(exceptionChecked, "ParseJoin params cannot be empty");

			exceptionChecked = false;
			try
			{
				parser.ParseJoin(player, null);
			}
			catch (Exception e)
			{
				exceptionChecked = true;
			}
			Assert.IsTrue(exceptionChecked, "ParseJoin params cannot be null");
		}

		[TestMethod]
		public void ReadyTest()
		{
			//should just work lol

			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();
			//new GameController(player.CurrentGame).ActiveGame.Creator = player;

			Assert.IsTrue(parser.ParseStringInput(player, "ready"), "Ready command should return true");
		}

		[TestMethod]
		public void PutTest()
		{
			//should just work lol

			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			Assert.IsTrue(parser.ParseStringInput(player, "put bob, jim, joe, hoe"), "Put command should return true");
		}

		[TestMethod]
		public void VoteTest()
		{
			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			//Asserts if the parser suceeded
			Assert.IsTrue(parser.ParseStringInput(player, "Vote Yes"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote YESSSSS"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote approve"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote ACCEpts"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote Y"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote passed"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote PASSES"));

			Assert.IsTrue(parser.ParseStringInput(player, "Vote NOO"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote no"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote NnnNNnn"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote denied"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote rejected"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote fail"));
			Assert.IsTrue(parser.ParseStringInput(player, "Vote rejects::fsfdsf"));

			//Checks empty params exception and empty first param
			Boolean exceptionChecked = false;
			try
			{
				parser.ParseStringInput(player, "Vote");
			}
			catch(Exception e)
			{
				exceptionChecked = true;
			}
			Assert.IsTrue(exceptionChecked, "ParseVote not catching empty params");

			exceptionChecked = false;
			try
			{
				parser.ParseVote(player, null);
			}
			catch (Exception e)
			{
				exceptionChecked = true;
			}
			Assert.IsTrue(exceptionChecked, "ParseVote not catching null input");

			//Checks invalid params exception
			exceptionChecked = false;
			try
			{
				parser.ParseStringInput(player, "Vote balblabla");
			}
			catch (Exception e)
			{
				exceptionChecked = true;
			}
			Assert.IsTrue(exceptionChecked, "Vote params are invalid");
		}

		[TestMethod]
		public void PassTest()
		{
			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			Assert.IsTrue(parser.ParseStringInput(player, "PaSS:::fsdfsgGDFGrd"), "Pass command should return true");
		
		}

		[TestMethod]
		public void FailTest()
		{
			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			Assert.IsTrue(parser.ParseStringInput(player, "FAIL sdfsdfegsdf"), "Fail command should return true");
		
		}

		[TestMethod]
		public void StatsTest()
		{
			//Init variables
			// Create a test player
			var player = TestUtils.GeneratePlayerWithGame(this.db);

			//Test overall parser input
			// Simulate 'create' command
			var parser = new ResistanceSMS.Helpers.SMSParser();

			Assert.IsTrue(parser.ParseStringInput(player, "STATS"), "Stats command with no parameters should return true");
			Assert.IsTrue(parser.ParseStringInput(player, "stats poop"), "Stats command with parameters should return true");
			Assert.IsTrue(parser.ParseStringInput(player, "mystats"), "MyStarts command should return true");
			Assert.IsTrue(parser.ParseStringInput(player, "mystats:Dfes3fasb"), "mystats command with parameters should return true");
			
			Boolean exceptionChecked = false;
			try
			{
				parser.ParseStats(player, null);
			}
			catch (Exception e)
			{
				exceptionChecked = true;
			}
			Assert.IsTrue(exceptionChecked, "ParseStats not catching null input");

		}

		[TestMethod]
		public void HelpTest()
		{
			//TODO: implement
		}

		//TODO: make it a random generator
		
	}
}
