using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceSMS.Models
{
	public class Round
	{
		public Guid RoundId { get; set; }
		public virtual Game Game { get; set; }
		public int RoundNumber { get; set; }
		public virtual Player Leader { get; set; }
		public int NumFailures { get; set; }
		public virtual ICollection<Player> MissionPlayers { get; set; }
		public virtual ICollection<Player> VoteMissionApprove { get; set; }
		public virtual ICollection<Player> VoteMissionReject { get; set; }
		public virtual ICollection<Player> VoteMissionPass { get; set; }
		public virtual ICollection<Player> VoteMissionFail { get; set; }
		public bool MissionPassed { get; set; }
	}
}