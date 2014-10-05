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
        public int[,] missionPlayerNumber = new int[,] 
            { {2, 2, 2, 3, 3, 3}, 
              {3, 3, 3, 4, 4, 4}, 
              {2, 4, 3, 4, 4, 4}, 
              {3, 3, 4, 5, 5, 5}, 
              {3, 4, 4, 5, 5, 5} };
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

			SMSPlayer(player, "🎉 You have successfully created a game! Tell others to text 'Join " + game.FriendlyId + "'.");
		}

		public void JoinGame(Player player, String friendlyGameId)
		{
			var matchingGame = _Db.Games.Where(g => g.FriendlyId == friendlyGameId.ToUpper()).FirstOrDefault();
			if (matchingGame == null)
			{
				SMSPlayer(player, "We could not find your game! Check your ID and try again.");
				return;
			}
			var dbPlayer = _Db.Players.Where(p => p.PlayerId == player.PlayerId).FirstOrDefault();
			matchingGame.Players.Add(dbPlayer);
			dbPlayer.CurrentGame = matchingGame;
			_Db.SaveChanges();
			SMSPlayerList(matchingGame.Players, "🎆 " + dbPlayer.Name + " has joined the game! There are " + matchingGame.Players.Count + " players in the game.");
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
                    AssignTeams();
                    AssignLeader();
                }
            }
            else if (ActiveGame.GameState == Game.GameStates.SelectMissionPlayers
              && toState == Game.GameStates.VoteMissionApprove)
            {
                // transition from SelectMissionPlayers to VoteMissionApprove
                // assign the next player to be the leader
                AssignLeader();
                SendSelectMissionPlayersMessage();
            }
            else if (ActiveGame.GameState == Game.GameStates.VoteMissionApprove
              && toState == Game.GameStates.VoteMissionPass)
            {
                // transition from VoteMissionApprove to VoteMissionPass
              
              
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
            var playerList = this.ActiveGame.Players.OrderBy(x => rnd.Next()).ToList();
			this.ActiveGame.SpyPlayers = playerList.Take(numSpies).ToList();
			this.ActiveGame.ResistancePlayers = playerList.Skip(numSpies).ToList();
			playerList = this.ActiveGame.Players.OrderBy(x => rnd.Next()).ToList();
			for (int i = 0; i < playerList.Count; i++)
            {
				playerList[i].TurnOrder = i;
            }
            _Db.SaveChanges();

			SMSPlayerList(this.ActiveGame.ResistancePlayers, "😎 You are a Resistance member!");
			SMSPlayerList(this.ActiveGame.SpyPlayers, "👿 You are a Spy!");
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
			var leaderPlayer = lastRound.Leader;
            var message = leaderPlayer.Name +  " is the mission leader for this round.";
            SMSPlayerList(this.ActiveGame.Players, message);
			SMSPlayer(leaderPlayer, "Select players to put on the mission by texting 'Put [playername]'.");
        }

		public void CreateNewRound()
		{
            this.ActiveGame.Rounds.Add(new Round());
		}

		/// <summary>
		/// Called by the parser when a player says they are ready
		/// </summary>
		/// <param name="player">Sender</param>
		/// <param name="ready">Ready state (assume true for now, see NOTE)</param>
		public void PlayerIsReady(Player player, Boolean ready)
		{
			if (!ready) return;
			if (this.ActiveGame.Creator.PlayerId == player.PlayerId)
			{
				// START DA GAME!
				StateTransition(Game.GameStates.SelectMissionPlayers);
			}
			else
			{
				SMSPlayer(player, "You are not the game creator, and cannot start the game!");
			}
			//NOTE: don't have to handle no ready yet since there is no way for
			//		the player to be change from ready to not ready
		}
		
		/// <summary>
		/// Sends a text message to the leader to select the mission players
		/// </summary>
		public void SendSelectMissionPlayersMessage()
		{
            var message="Hey leader! Please select mission players!"; 
            SMSPlayer(this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last().Leader, message);
		}

		/// <summary>
		/// Is called by the parser to set the mission players and increment state
		/// </summary>
		/// <param name="player"></param>
		/// <param name="players"></param>
		public void SelectMissionPlayers(Player player, String[] players)
		{
            var lastRound = this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last();
            var roundNumber = lastRound.RoundNumber;
            var playerNumber = this.ActiveGame.Players.Count;
            int numberOfMissionPlayers = missionPlayerNumber[roundNumber-1, playerNumber-5];
            if (players.Count() != numberOfMissionPlayers) 
            {
                // the number of mission players does not match
                var message = "The number of mission players in this round has to be " 
                    + numberOfMissionPlayers + ".";
                SMSPlayer(player, message);
            }
            else if (!player.Equals(this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last().Leader))
            {
                //  the player who sent the message does not match
                var message = "You are not the leader! SMS is expensive!";
                SMSPlayer(player, message);
            }
            else if (!this.ActiveGame.GameState.Equals(Game.GameStates.SelectMissionPlayers)) 
            {
                //  the game state does not match
                var message = "You did not send this message at the right time man.";
                SMSPlayer(player, message);
            }
            for (int i = 0; i < numberOfMissionPlayers; i++ )
            {
                String candidate = players[i];
                int check = checkCandidate(candidate);
                if (check == -1) 
                {
                    //  the candidate does not exist
                    var message = "The name you typed in doesn't exist.";
                    SMSPlayer(player, message);
                    break;
                }
                else
                {
                    // add him to the mission players list
                    Player playerCand = this.ActiveGame.Players.ElementAt(check);
                    this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last()
                        .MissionPlayers.Add(playerCand);
                }
            }
			//increment
		}
        /// <summary>
        /// check if the candidate is in the player list
        /// used in SelectMissionPlayers
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public int checkCandidate(String candidate)
        {
            for (int i = 0; i < this.ActiveGame.Players.Count(); i++ )
            {
                if (candidate.Equals(this.ActiveGame.Players.ElementAt(i))) {
                    return i;
                }
            }
            return -1;
        }

		/// <summary>
		/// Sends a text message to all players to vote for poeple going on the
		/// mission
		/// </summary>
		public void SendVoteMessage()
		{
            var message = "Hey everyone! Please vote for the mission.";
            SMSPlayerList(this.ActiveGame.Players, message);

		}

		/// <summary>
		/// Is called by the parser to inform the game that a player has voted
		/// for the players going on the mission
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
            if (this.ActiveGame.RoundsOrdered.Last().VoteMissionApprove.Count
                > this.ActiveGame.RoundsOrdered.Last().VoteMissionReject.Count)
            {
                this.ActiveGame.RoundsOrdered.Last().NumRejections++;
            }
            else if (this.ActiveGame.RoundsOrdered.Last().NumRejections == 5)
            {
                //end game
            }

		}

		/// <summary>
		/// Sends a message to the mission goers to vote pass or fail
		/// </summary>
		public void SendPassOrFailMessage()
		{
            var lastRound = this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last();
            var missionPlayers = lastRound.MissionPlayers;
            var message = "Okay mission goers, please vote pass or fail!";
            SMSPlayerList(missionPlayers, message);
		}

		/// <summary>
		/// Called by the parser to inform the game that a player on a mission has
		/// passed or failed the mission.  the state should be incremented after
		/// all mission goers have voted
		/// </summary>
		/// <param name="player"></param>
		/// <param name="vote"></param>
		public void CheckPassOrFail(Player playerRef, Boolean vote)
		{
            var player = _Db.Players.Where(p => p.PlayerId == playerRef.PlayerId).FirstOrDefault();
            if (vote)
            {
                if (this.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Contains(player))
                {
                    this.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Remove(player);
                }
                this.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Add(player);
            }
            else
            {
                if (this.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Contains(player))
                {
                    this.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Remove(player);
                }
                this.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Add(player);
            }
            _Db.SaveChanges();
		}

		/// <summary>
		/// Stats send to the player
		/// </summary>
		public void SendStatsMessage()
		{
            var message = "";
            this.SMSPlayerList(this.ActiveGame.Players, message);
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
			this.SMSPlayer(player, "This is an invalid command: " + command);
		}

		/// <summary>
		/// Called by the parser when a player wants to change his name
		/// </summary>
		/// <param name="player"></param>
		/// <param name="name"></param>
		public void ChangeName(Player player, String name)
		{
			//TODO: check work
			//TODO: check name collision, check name validity (should be handled
			//		by parser anyways)
			String oldName = player.Name;
			player.Name = name;
			String message = player.Name + "has changed their name to " + name;

			this.SMSPlayerList(this.ActiveGame.Players, message);
		}

		/// <summary>
		/// Sends a text message to the specified list of players.
		/// </summary>
		/// <param name="players">Enumerable of players to send to</param>
		/// <param name="message">Message to send</param>
		public void SMSPlayerList(IEnumerable<Player> players, string message)
		{
			if (ConfigurationManager.AppSettings["TwilioFromNumber"].Length <= 0) return;
			if (players == null || players.Count() <= 0) return;
			foreach (var player in players)
			{
				if (player.PhoneNumber == null || player.PhoneNumber.Length < 12) continue;
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
			SMSPlayerList(new List<Player>() { player }, message);
		}
	}
}