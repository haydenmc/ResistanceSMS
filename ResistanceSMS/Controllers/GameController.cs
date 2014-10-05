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
			if (game != null)
			{
				this.ActiveGame = this._Db.Games.Where(g => g.GameId == game.GameId).FirstOrDefault();
			}
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



        public void StateTransition(Game.GameStates toState)
        {
            if (toState == Game.GameStates.SelectMissionPlayers)
            {
                // Every transition to SelectMissionPlayers means creating a new round.
				CreateNewRound();
                if (ActiveGame.GameState == Game.GameStates.Waiting)
                {
                    // transition from Waiting to SelectMissionPlayers
                    // needs to join game, assign a random leader and assign teams
                    AssignLeader();
                    AssignTeams();
                }
            }
            else if (ActiveGame.GameState == Game.GameStates.SelectMissionPlayers
              && toState == Game.GameStates.VoteMissionApprove)
            {
                // transition from SelectMissionPlayers to VoteMissionApprove
                // assign the next player to be the leader
                AssignLeader();
                SendSelectMissionPlayersMessage();
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
					this.SendPassOrFailMessage();
                }
				this.SendStatsMessage();
			}
            ActiveGame.GameState = toState;
		}

        //Helper Methods
        public void AssignTeams()
        {
            var numSpies = (int)Math.Round(Math.Sqrt(2 * (this.ActiveGame.Players.Count - 3)));

            Random rnd = new Random();
            this.ActiveGame.Players = this.ActiveGame.Players.OrderBy(x => rnd.Next()).ToList();
            this.ActiveGame.SpyPlayers = this.ActiveGame.Players.Take(numSpies).ToList();
            this.ActiveGame.ReadyPlayers = this.ActiveGame.Players.Skip(numSpies).ToList();
            this.ActiveGame.Players = this.ActiveGame.Players.OrderBy(x => rnd.Next()).ToList();
            for (int i = 0; i < this.ActiveGame.Players.Count; i++)
            {
                this.ActiveGame.Players.ElementAt(i).TurnOrder = i;
            }
            _Db.SaveChanges();

            SMSPlayerList(this.ActiveGame.ResistancePlayers, "You are a Resistance member!");
            SMSPlayerList(this.ActiveGame.SpyPlayers, "You are a spy!");
        }
        
        public void AssignLeader()
        {
            var lastRound = this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last();
            if (this.ActiveGame.Rounds.Last().Leader == null)
            {
                lastRound.Leader = this.ActiveGame.Players.OrderBy(p => p.TurnOrder).First();
            }
            else
            {
                var lastLeader = lastRound.Leader;
                var playerList = this.ActiveGame.Players.OrderBy(p => p.TurnOrder).ToList();
                var leaderIndex = playerList.IndexOf(lastLeader);
                leaderIndex++;
                if (leaderIndex >= playerList.Count)
                {
                    leaderIndex = 0;
                }
                var nextLeader = playerList.ElementAt(leaderIndex);
                lastRound.Leader = nextLeader;
            }
            _Db.SaveChanges();
            var message = this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last().Leader.Name +  "is the leader for this round.";
            SMSPlayerList(this.ActiveGame.Players, message);
        }
        public void SelectMissionPlayers()
        {

        }

		public void CreateNewRound()
		{

		}

		/// <summary>
		/// Called by the parser when a player says they are ready
		/// </summary>
		/// <param name="player"></param>
		/// <param name="ready"></param>
		public void PlayerIsReady(Player player, Boolean ready)
		{
			//NOTE: don't have to handle no ready yet since there is no way for
			//		the player to be change from ready to not ready
		}
		
		/// <summary>
		/// Sends a text message to the leader to select the mission players
		/// </summary>
		public void SendSelectMissionPlayersMessage()
		{

		}

		/// <summary>
		/// Is called by the parser to set the mission players and increment state
		/// </summary>
		/// <param name="player"></param>
		/// <param name="players"></param>
		public void SelectMissionPlayers(Player player, String[] players)
		{

			//increment
		}

		/// <summary>
		/// Sends a text message to all players to vote for poeple going on the
		/// mission
		/// </summary>
		public void SendVoteMessage()
		{

		}

		/// <summary>
		/// Is called by the parser to inform the game that a player has voted
		/// for the people going on the mission
		/// </summary>
		/// <param name="player"></param>
		/// <param name="vote"></param>
		public void PlayerVote(Player player, Boolean vote)
		{

		}

		/// <summary>
		/// Checks if the mission going votes have been rejected, if 5 rejections
		/// have happened then the game is over.  This should be called by 
		/// PlayerVote after all people has voted
		/// </summary>
		public void CheckRejected()
		{

		}

		/// <summary>
		/// Sends a message to the mission goers to vote pass or fail
		/// </summary>
		public void SendPassOrFailMessage()
		{

		}

		/// <summary>
		/// Called by the parser to inform the game that a player on a mission has
		/// passed or failed the mission.  the state should be incremented after
		/// all mission goers have voted
		/// </summary>
		/// <param name="player"></param>
		/// <param name="vote"></param>
		public void CheckPassOrFail(Player player, Boolean vote)
		{

		}

		/// <summary>
		/// Stats send to the player
		/// </summary>
		public void SendStatsMessage()
		{

		}

		/// <summary>
		/// Called by the paser to request stats.  If name is empty, it's
		/// requesting game stats.  If name is not empty, it's asking for
		/// the listed player's stats.
		/// </summary>
		/// <param name="player"></param>
		public void RequestStats(Player player, String name)
		{

		}

		/// <summary>
		/// Called by the parser when an invalid command is called
		/// </summary>
		/// <param name="player"></param>
		/// <param name="command"></param>
		public void InvalidCommand(Player player, String command)
		{

		}

		/// <summary>
		/// Called by the parser when a player wants to change his name
		/// </summary>
		/// <param name="player"></param>
		/// <param name="name"></param>
		public void ChangeName(Player player, String name)
		{

		}

		/// <summary>
		/// Sends a text message to the specified list of players.
		/// </summary>
		/// <param name="players">Enumerable of players to send to</param>
		/// <param name="message">Message to send</param>
		public void SMSPlayerList(IEnumerable<Player> players, string message)
		{
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