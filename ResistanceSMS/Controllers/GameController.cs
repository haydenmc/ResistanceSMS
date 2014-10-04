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
        private ApplicationDbContext _Db = new ApplicationDbContext();
        private Guid _ActiveGameId;
        public Game ActiveGame
        {
            get
            {
                return _Db.Games.Where(g => g.GameId == this._ActiveGameId).FirstOrDefault();
            }
            set
            {
                this._ActiveGameId = value.GameId;
            }
        }
		public GameController(Game game)
		{
            this.ActiveGame = this._Db.Games.Where(g => g.GameId == game.GameId).FirstOrDefault();
		}

		public void CreateGame(Player creator)
		{
			// Generate a random friendly ID
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			var friendlyId = new string(
				Enumerable.Repeat(chars, 8)
							.Select(s => s[random.Next(s.Length)])
							.ToArray());

			var player = _Db.Players.Where(p => p.PlayerId == creator.PlayerId).FirstOrDefault();
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
			_Db.Games.Add(game);
			player.CurrentGame = game;
			_Db.SaveChanges();
		}

		public void JoinGame(Player joiner, String friendlyGameId)
		{
			var matchingGame = _Db.Games.Where(g => g.FriendlyId == friendlyGameId.ToUpper()).FirstOrDefault();
			if (matchingGame == null)
			{

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

        //Helper Methods
        public void JoinGame()
        {
            
        }

        public void AssignTeams()
        {
        
        }
        
        public void AssignLeader()
        {
            Random rnd = new Random();
            if (this.ActiveGame.Rounds.Last().Leader == null)
            {
                this.ActiveGame.Rounds.Last().Leader = this.ActiveGame.Players.First();
            }
            else
            {
                var lastLeader = this.ActiveGame.Rounds.Last().Leader;
                var leaderIndex = this.ActiveGame.Players.ToList().IndexOf(lastLeader);
                leaderIndex++;
                if (leaderIndex >= this.ActiveGame.Players.Count)
                {
                    leaderIndex = 0;
                }
                var nextLeader = this.ActiveGame.Players.ToList()[leaderIndex];
            }
            _Db.SaveChanges();
        }
	}
}