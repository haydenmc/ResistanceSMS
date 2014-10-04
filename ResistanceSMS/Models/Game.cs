using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceSMS.Models
{
	public class Game
	{
		public enum GameStates
		{
			Waiting,
			SelectMissionPlayers,
			VoteMissionApprove,
			VoteMissionPass,
			GameEnd
		}
		public Guid GameId { get; set; }
		public ICollection<Player> Players { get; set; }
		public ICollection<Player> ReadyPlayers { get; set; }
		public ICollection<Player> SpyPlayers { get; set; }
		public ICollection<Player> ResistancePlayers { get; set; }
		public Player Creator { get; set; }
		public ICollection<Round> Rounds { get; set; }
		public int ResistanceScore { get; set; }
		public int SpyScore { get; set; }
		public DateTimeOffset LastActivityTime { get; set; }
		public DateTimeOffset CreateTime { get; set; }
		public bool GameStarted { get; set; }
		public GameStates GameState { get; set; }
	}
}