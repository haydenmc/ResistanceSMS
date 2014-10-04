using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
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
				var player = db.Players.Where(p => p.PlayerId == creator.PlayerId).FirstOrDefault();
				var game = new Game()
				{
					GameId = Guid.NewGuid(),
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

		public void StateTransition(Game.GameStates toState) {
			if (ActiveGame.GameState == Game.GameStates.Waiting
				&& toState == Game.GameStates.SelectMissionPlayers)
			{
				// organize teams and shit
			}
			// etc.
		}
	}
}