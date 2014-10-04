using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace ResistanceSMS.Filters
{
	public class BasicAuthenticator : Attribute, IAuthenticationFilter
	{
		public bool AllowMultiple { get { return false; } }

		public BasicAuthenticator()
		{
		}

		public Task AuthenticateAsync(HttpAuthenticationContext context,
									  CancellationToken cancellationToken)
		{
			var req = context.Request;
			if (req.Headers.Authorization != null &&
					req.Headers.Authorization.Scheme.Equals(
							  "basic", StringComparison.OrdinalIgnoreCase))
			{
				Encoding encoding = Encoding.GetEncoding("iso-8859-1");
				string credentials = encoding.GetString(
									  Convert.FromBase64String(
										  req.Headers.Authorization
													   .Parameter));
				string[] parts = credentials.Split(':');
				string userId = parts[0].Trim();
				string password = parts[1].Trim();

				if (userId.Equals("twilio") && password.Equals("DarrenSmellsBad")) // Just a dumb check
				{
					var claims = new List<Claim>()
						{
							new Claim(ClaimTypes.Name, "twilioauth")
						};
					var id = new ClaimsIdentity(claims, "Basic");
					var principal = new ClaimsPrincipal(new[] { id });
					context.Principal = principal;
				}
			}
			else
			{
				context.ErrorResult = new UnauthorizedResult(
						 new AuthenticationHeaderValue[0],
											  context.Request);
			}

			return Task.FromResult(0);
		}

		public async Task ChallengeAsync(HttpAuthenticationChallengeContext context,
							   CancellationToken cancellationToken)
		{
			var result = await context.Result.ExecuteAsync(cancellationToken);
			if (result.StatusCode == HttpStatusCode.Unauthorized)
			{
				result.Headers.WwwAuthenticate.Add(
						new AuthenticationHeaderValue(
							"Basic"));
			}
			context.Result = new ResponseMessageResult(result);
		}
	}
}