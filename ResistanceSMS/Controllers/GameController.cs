using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ResistanceSMS.Controllers
{
	public class GameController
	{
		public Game ActiveGame { get; set; }
		public GameController(Game game)
		{
			this.ActiveGame = game;
		}

		public void CreateGame(Player creator)
		{
			using (var db = new ApplicationDbContext())
			{
				// Generate a random friendly ID
				var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
				var random = new Random();
				var friendlyId = new string(
					Enumerable.Repeat(chars, 8)
							  .Select(s => s[random.Next(s.Length)])
							  .ToArray());

				var player = db.Players.Where(p => p.PlayerId == creator.PlayerId).FirstOrDefault();
				var game = new Game()
				{
					GameId = Guid.NewGuid(),
					FriendlyId = friendlyId, // TODO: Make sure this doesn't collide
					Creator = player,
					Players = new List<Player>(),
					ReadyPlayers = new List<Player>(),
					ResistancePlayers = new List<Player>(),
					SpyPlayers = new List<Player>(),
					Rounds = new List<Round>(),
					ResistanceScore = 0,
					SpyScore = 0,
					GameState = Game.GameStates.Waiting,
					CreateTime = DateTimeOffset.Now,
					LastActivityTime = DateTimeOffset.Now
				};
				db.Games.Add(game);
				player.CurrentGame = game;
				db.SaveChanges();
			}
		}

		public void JoinGame(Player joiner, String friendlyGameId)
		{
			using (var db = new ApplicationDbContext())
			{
				var matchingGame = db.Games.Where(g => g.FriendlyId == friendlyGameId.ToUpper()).FirstOrDefault();
				if (matchingGame == null)
				{

				}
			}
		}

		public void StateTransition(Game.GameStates toState) {
			if (ActiveGame.GameState == Game.GameStates.Waiting
				&& toState == Game.GameStates.SelectMissionPlayers)
			{
				// organize teams and shit
			}
			// etc.
		}

		/// <summary>
		/// Sends a text message to the specified list of players.
		/// </summary>
		/// <param name="players">Enumerable of players to send to</param>
		/// <param name="message">Message to send</param>
		public void SMSPlayerList(IEnumerable<Player> players, string message) {
			foreach (var player in players)
			{
				SMSController.TwilioClient.Value.SendSmsMessage(ConfigurationManager.AppSettings["TwilioFromNumber"], player.PhoneNumber, message);
			}
		}

		/// <summary>
		/// Sends a message to a particular player
		/// </summary>
		/// <param name="player">The player to send to</param>
		/// <param name="message">The message to send</param>
		public void SMSPlayer(Player player, string message)
		{
			var twilioResult = SMSController.TwilioClient.Value.SendSmsMessage(ConfigurationManager.AppSettings["TwilioFromNumber"], player.PhoneNumber, message);
		}
	}
}