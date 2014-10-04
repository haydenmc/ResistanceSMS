using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceSMS.Models
{
	public class TwilioRequest
	{
		public string MessageSid { get; set; }
		public string SmsId { get; set; }
		public string AccoundSid { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string Body { get; set; }
		public int NumMedia { get; set; }
		public string FromCity { get; set; }
		public string FromState { get; set; }
		public string FromZip { get; set; }
		public string FromCountry { get; set; }
		public string ToCity { get; set; }
		public string ToState { get; set; }
		public string ToZip { get; set; }
		public string ToCountry { get; set; }
	}
}