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
		{
			{2, 2, 2, 3, 3, 3}, 
			{3, 3, 3, 4, 4, 4}, 
			{2, 4, 3, 4, 4, 4}, 
			{3, 3, 4, 5, 5, 5}, 
			{3, 4, 4, 5, 5, 5}
		};

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
				Enumerable.Repeat(chars, 4)
							.Select(s => s[random.Next(s.Length)])
							.ToArray());

			var player = _Db.Players.Where(p => p.PlayerId == creator.PlayerId).FirstOrDefault();
				var game = new Game()
				{
					GameId = Guid.NewGuid(),
				FriendlyId = friendlyId, // TODO: Make sure this doesn't collide
					Creator = player,
				Players = new List<Player>() { player },
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

			SMSPlayer(player, "🎉 You've created a game! Tell others to text 'Join " + game.FriendlyId + "'.");
		}

		public void JoinGame(Player player, String friendlyGameId)
		{
			var matchingGame = _Db.Games.Where(g => g.FriendlyId == friendlyGameId.ToUpper()).FirstOrDefault();
			if (matchingGame == null)
			{
				SMSPlayer(player, "💩 Couldn't find your game! Check your ID and try again.");
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
                if (this.ActiveGame.GameState != Game.GameStates.VoteMissionApprove)
                {
                    CreateNewRound();
                }
				if (ActiveGame.GameState == Game.GameStates.Waiting)
				{
					// transition from Waiting to SelectMissionPlayers
					// needs to join game, assign a random leader and assign teams
					AssignTeams();
				}
				AssignLeader();
			}
			else if (ActiveGame.GameState == Game.GameStates.SelectMissionPlayers
				&& toState == Game.GameStates.VoteMissionApprove)
			{
				// transition from SelectMissionPlayers to VoteMissionApprove
				// assign the next player to be the leader
				SendVoteMessage();
			}
			else if (ActiveGame.GameState == Game.GameStates.VoteMissionApprove
				&& toState == Game.GameStates.VoteMissionPass)
			{
				// transition from VoteMissionApprove to VoteMissionPass
				SendPassOrFailMessage();
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
			_Db.SaveChanges();
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
			var roundHistory = this.ActiveGame.RoundsOrdered.ToList();
			var lastRound = roundHistory.Last();
            if (roundHistory.Count == 1) // this is the first round
            {
                lastRound.Leader = this.ActiveGame.Players.OrderBy(p => p.TurnOrder).First();
            }
            else
            {
                var lastLeader = roundHistory[roundHistory.Count-2].Leader;
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
			var roundNumber = lastRound.RoundNumber;
			var playerNumber = this.ActiveGame.Players.Count;
			int numberOfMissionPlayers = missionPlayerNumber[roundNumber, playerNumber - 5];
			message = "🌟 Select " + numberOfMissionPlayers + " mission players by texting 'Put [name], [name], etc.'";
			SMSPlayer(leaderPlayer, message);
        }

		public void CreateNewRound()
		{
            this.ActiveGame.Rounds.Add(new Round()
            {
                RoundId = Guid.NewGuid(),
                Game = this.ActiveGame,
                MissionPlayers = new List<Player>(),
                NumRejections = 0,
                MissionPassed = false,
                RoundNumber = this.ActiveGame.Rounds.Count,
                VoteMissionApprove = new List<Player>(),
                VoteMissionReject = new List<Player>(),
                VoteMissionPass = new List<Player>(),
                VoteMissionFail = new List<Player>()
            });
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
				SMSPlayer(player, "💩 You are not the game creator, and cannot start the game!");
			}
			//NOTE: don't have to handle no ready yet since there is no way for
			//		the player to be change from ready to not ready
		}
		
		/// <summary>
		/// Is called by the parser to set the mission players and increment state
		/// </summary>
		/// <param name="player"></param>
		/// <param name="players"></param>
		public void SelectMissionPlayers(Player player, String[] players)
		{
            var lastRound = this.ActiveGame.RoundsOrdered.Last();
            var roundNumber = lastRound.RoundNumber;
            var playerNumber = this.ActiveGame.Players.Count;
            int numberOfMissionPlayers = missionPlayerNumber[roundNumber, playerNumber-5];
			if (!this.ActiveGame.GameState.Equals(Game.GameStates.SelectMissionPlayers))
			{
				//  the game state does not match
				var message = "💢 Invalid command at this time.";
				SMSPlayer(player, message);
				return;
			}
			if (!player.PlayerId.Equals(this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last().Leader.PlayerId))
			{
				//  the player who sent the message does not match
				var message = "💢 You are not the leader!";
				SMSPlayer(player, message);
				return;
			}
            if (players.Count() != numberOfMissionPlayers) 
            {
                // the number of mission players does not match
				var message = "💢 The number of mission players in this round has to be " 
                    + numberOfMissionPlayers + ".";
                SMSPlayer(player, message);
				return;
            }
			this.ActiveGame.RoundsOrdered.Last().MissionPlayers.Clear();
            for (int i = 0; i < numberOfMissionPlayers; i++ )
            {
                String candidate = players[i];
                Player playerCand = this.ActiveGame.Players.Where(x => x.Name.ToLower() == candidate.ToLower()).FirstOrDefault();
                if (playerCand == null) 
                {
                    //  the candidate does not exist
					var message = "💢 The name '" + candidate + "' doesn't exist.";
                    SMSPlayer(player, message);
					return;
                }
                // add him to the mission players list
                this.ActiveGame.RoundsOrdered.Last().MissionPlayers.Add(playerCand);
            }
			var playerNames = this.ActiveGame.RoundsOrdered.Last().MissionPlayers.Select(x => x.Name).Aggregate((current, next) => current + ", " + next);
			SMSPlayerList(this.ActiveGame.Players, playerNames + " have been selected.");
			StateTransition(Game.GameStates.VoteMissionApprove);
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
			var message = "🌟 You must vote to approve this mission! 'Vote yes' or 'Vote no'.";
            SMSPlayerList(this.ActiveGame.Players, message);

		}

		/// <summary>
		/// Is called by the parser to inform the game that a player has voted
		/// for the players going on the mission
		/// </summary>
		/// <param name="player"></param>
		/// <param name="vote"></param>
		public void PlayerVote(Player playerRef, Boolean vote)
		{
			var player = _Db.Players.Where(p => p.PlayerId == playerRef.PlayerId).FirstOrDefault();
			var round = this.ActiveGame.RoundsOrdered.Last();
			round.VoteMissionApprove.Remove(player);
			round.VoteMissionReject.Remove(player);
			if (vote)
			{
				round.VoteMissionApprove.Add(player);
			}
			else
			{
				round.VoteMissionReject.Add(player);
			}
			SMSPlayer(player, "👌 Your vote has been recorded.");
			_Db.SaveChanges();
			if (round.VoteMissionReject.Count + round.VoteMissionApprove.Count == this.ActiveGame.Players.Count)
			{
				if (round.VoteMissionApprove.Count >= round.VoteMissionReject.Count)
				{
					// TODO: Reveal players' votes.
					SMSPlayerList(this.ActiveGame.Players, "👍 Mission approved. Standby for mission status...");
					StateTransition(Game.GameStates.VoteMissionPass);
				}
				else
				{
					SMSPlayerList(this.ActiveGame.Players, "👎 Mission rejected. The Resistance is suspicious...");
					round.NumRejections++;
					round.VoteMissionApprove.Clear();
					round.VoteMissionReject.Clear();
					_Db.SaveChanges();
					StateTransition(Game.GameStates.SelectMissionPlayers);
				}
			}
		}

		/// <summary>
		/// Sends a message to the mission goers to vote pass or fail
		/// </summary>
		public void SendPassOrFailMessage()
		{
            var lastRound = this.ActiveGame.Rounds.OrderBy(r => r.RoundNumber).Last();
            var missionPlayers = lastRound.MissionPlayers;
			var message = "💥 You are on the mission. Please text 'Pass' or 'Fail'!";
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
			this.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Remove(player);
			this.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Remove(player);
            if (vote)
            {
                this.ActiveGame.RoundsOrdered.Last().VoteMissionPass.Add(player);
            }
            else
            {
                this.ActiveGame.RoundsOrdered.Last().VoteMissionFail.Add(player);
            }
            _Db.SaveChanges();
			var round = this.ActiveGame.RoundsOrdered.Last();
			if (round.VoteMissionPass.Count + round.VoteMissionFail.Count == round.MissionPlayers.Count)
			{
				if (round.VoteMissionFail.Count > 0)
				{
					SMSPlayerList(this.ActiveGame.Players, "👿 Mission FAILED w/ " + round.VoteMissionPass.Count + " passes, " + round.VoteMissionFail.Count + " fails.");
					this.ActiveGame.SpyScore++;
				}
				else
				{
					SMSPlayerList(this.ActiveGame.Players, "😎 Mission PASSED w/ " + round.VoteMissionPass.Count + " passes, " + round.VoteMissionFail.Count + " fails.");
					this.ActiveGame.ResistanceScore++;
				}
				StateTransition(Game.GameStates.SelectMissionPlayers);
			}
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
			this.SMSPlayer(player, "💢 This is an invalid command: " + command);
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
			this.ActiveGame.Players.Where(p => p.PlayerId == player.PlayerId).First().Name = name;
			player.Name = name;
			String message = "😮 " + oldName + " has changed their name to '" + name + "'";
			this._Db.SaveChanges();
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