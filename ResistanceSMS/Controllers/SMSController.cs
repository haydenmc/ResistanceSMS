using ResistanceSMS.Filters;
using ResistanceSMS.Helpers;
using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using Twilio;
using Twilio.TwiML;

namespace ResistanceSMS.Controllers
{
	public class SMSController : ApiController
	{
		private SMSParser Parser = new SMSParser();

		public static Lazy<TwilioRestClient> TwilioClient = new Lazy<TwilioRestClient>(
			() => new TwilioRestClient(ConfigurationManager.AppSettings["TwilioAccountSid"], ConfigurationManager.AppSettings["TwilioAuthToken"])
		);

		// POST api/SMS
		[BasicAuthenticator]
		public HttpResponseMessage Post(TwilioRequest twilioRequest)
		{
			using (var db = new ApplicationDbContext())
			{
				var player = db.Players.Where(x => x.PhoneNumber == twilioRequest.From).FirstOrDefault();
				if (player == null)
				{
					Random rnd = new Random();
					player = new Player()
					{
						PlayerId = Guid.NewGuid(),
						PhoneNumber = twilioRequest.From,
						JoinTime = DateTimeOffset.Now,
						Losses = 0,
						Wins = 0,
						Name = "Operative" + rnd.Next(1, 100),
						LastActivityTime = DateTimeOffset.Now
					};
					db.Players.Add(player);
					db.SaveChanges();
				}
				try
				{
					Parser.ParseStringInput(player, twilioRequest.Body);
				}
				catch (Exception e)
				{
					new GameController(null).SMSPlayer(player, "💣 PARSE ERROR: " + e.Message);
				}
				return Request.CreateResponse<string>(HttpStatusCode.OK,"");
			}
		}

		[BasicAuthenticator]
		public HttpResponseMessage Delete()
		{
			using (var db = new ApplicationDbContext())
			{
				db.Games.RemoveRange(db.Games);
				db.SaveChanges();
				db.Rounds.RemoveRange(db.Rounds);
				db.SaveChanges();
				db.Players.RemoveRange(db.Players);
				db.SaveChanges();
				
				
			}
			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}
