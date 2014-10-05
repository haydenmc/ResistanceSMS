using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ResistanceSMS.Models
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext()
			: base("DefaultConnection")
		{

		}

		public DbSet<Game> Games { get; set; }
		public DbSet<Round> Rounds { get; set; }
		public DbSet<Player> Players { get; set; }
	}
}