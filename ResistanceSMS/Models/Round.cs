using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceSMS.Models
{
	public class Round
	{
		public Guid RoundId { get; set; }
		public Game Game { get; set; }
		public Player Leader { get; set; }
		public int NumFailures { get; set; }
		public ICollection<Player> MissionPlayers { get; set; }
		public ICollection<Player> VoteMissionApprove { get; set; }
		public ICollection<Player> VoteMissionReject { get; set; }
		public ICollection<Player> VoteMissionPass { get; set; }
		public ICollection<Player> VoteMissionFail { get; set; }
		public bool MissionPassed { get; set; }
	}
}