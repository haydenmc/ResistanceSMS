using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceSMS.Tests.Utils
{
	class TestUtils
	{
		public static Player GeneratePlayerWithGame(ApplicationDbContext db)
		{
			var creator = new Player()
			{
				PlayerId = Guid.NewGuid(),
				TurnOrder = 0
			};
			db.Players.Add(creator);
			db.SaveChanges();

			Game g = new Game()
			{
				GameId = Guid.NewGuid(),
				CreateTime = DateTimeOffset.Now,
				GameState = Game.GameStates.Waiting,
				Creator = creator,
				Players = new List<Player>()
                {
                    creator,
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 1
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 2
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 3
                    },
                    new Player() {
                        PlayerId = Guid.NewGuid(),
                        TurnOrder = 4
                    }
                },
				Rounds = new List<Round>()
                {
                    new Round() {
                        RoundId = Guid.NewGuid()
                    }
                }
			};
			creator.CurrentGame = g;
			db.Games.Add(g);
			db.SaveChanges();

			return creator;
		}
	}
}
