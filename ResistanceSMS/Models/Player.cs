using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceSMS.Models
{
	public class Player
	{
		public Guid PlayerId { get; set; }
		public Game CurrentGame { get; set; }
		public String PhoneNumber { get; set; }
		public String Name { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public DateTimeOffset LastActivityTime { get; set; }
		public DateTimeOffset JoinTime { get; set; }
	}
}