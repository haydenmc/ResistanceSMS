using ResistanceSMS.Filters;
using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using Twilio.TwiML;

namespace ResistanceSMS.Controllers
{
	public class SMSController : ApiController
	{
		// POST api/SMS
		[BasicAuthenticator]
		public IHttpActionResult Post(TwilioRequest twilioRequest)
		{
			var response = new Twilio.TwiML.TwilioResponse();
			response.Message("SOUNDS GOOD DUDE");

			// Force XML response... Twilio doesn't specify an Accept header.
			return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ObjectContent<XElement>(response.Element,
				   new System.Net.Http.Formatting.XmlMediaTypeFormatter
				   {
					   UseXmlSerializer = true
				   })
			});
		}
	}
}
