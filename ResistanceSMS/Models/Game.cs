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
        public string FriendlyId { get; set; }
        public virtual ICollection<Player> Players { get; set; }
		public virtual ICollection<Player> ReadyPlayers { get; set; }
		public virtual ICollection<Player> SpyPlayers { get; set; }
		public virtual ICollection<Player> ResistancePlayers { get; set; }
		public virtual Player Creator { get; set; }
		public virtual ICollection<Round> Rounds { get; set; }
		public int ResistanceScore { get; set; }
		public int SpyScore { get; set; }
		public DateTimeOffset LastActivityTime { get; set; }
		public DateTimeOffset CreateTime { get; set; }
		public bool GameStarted { get; set; }
		public GameStates GameState { get; set; }
	}
}