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
		public IHttpActionResult Post(TwilioRequest twilioRequest)
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
						Name = "Player" + rnd.Next(1, 100),
						LastActivityTime = DateTimeOffset.Now
					};
					db.Players.Add(player);
					db.SaveChanges();
				}
				Parser.ParseStringInput(player, twilioRequest.Body);
				return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK));
			}
		}
	}
}
