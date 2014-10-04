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



        public void StateTransition(Game.GameStates toState)
        {
            if (toState == Game.GameStates.SelectMissionPlayers)
            {
                // Every transition to SelectMissionPlayers means creating a new round.
               // CreateNewRound();
                if (ActiveGame.GameState == Game.GameStates.Waiting)
                {
                    // transition from Waiting to SelectMissionPlayers
                    // needs to join game, assign a random leader and assign teams
                   // JoinGame();
                   // AssignLeader();
                  //  AssignTeams();
                }
            }
            else if (ActiveGame.GameState == Game.GameStates.SelectMissionPlayers
              && toState == Game.GameStates.VoteMissionApprove)
            {
                // transition from SelectMissionPlayers to VoteMissionApprove
                // assign the next player to be the leader
              //  AssignLeader();
              //  SelectMissionPlayers();
            }
            else if (ActiveGame.GameState == Game.GameStates.VoteMissionApprove
              && toState == Game.GameStates.VoteMissionPass)
            {
                // transition from VoteMissionApprove to VoteMissionPass
              //  Vote();
                /*using (var db = new ApplicationDbContext())
			    {
                    var round = this.ActiveGame.Rounds.Last();
                    if (round.VoteMissionReject.Count == this.ActiveGame.Players.Count)
                    {
                        // the number of rejection is the same of the number of player in the game
                        // the game ends directly
                        StateTransition(Game.GameStates.GameEnd);
                    } else if (round.VoteMissionReject.Count )
                    {
                        // the 
                    }
                }*/
              //  CheckRejected();
            }
            else if (toState == Game.GameStates.GameEnd)
            {
                if (ActiveGame.GameState == Game.GameStates.VoteMissionPass)
                {
                    // transition from VoteMissionPass to GameEnd
                 //   Vote();
                //    CheckFail();
                }
             //   SendStats();
            }
            ActiveGame.GameState = toState;
        }
	}
}