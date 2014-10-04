using ResistanceSMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ResistanceSMS.Controllers
{
	public class SMSController : ApiController
	{
		// POST api/SMS
		[BasicAuthenticator]
		public string Post()
		{
			return "HEY!";
		}
	}
}
