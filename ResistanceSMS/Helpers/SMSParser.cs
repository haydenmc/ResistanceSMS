using ResistanceSMS.Controllers;
using ResistanceSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ResistanceSMS.Helpers
{
	public class SMSParser
	{
		//Utils regex
		public const String ANY_SUBSTRING_NAME	= "ANY";

		public const String DELIMIT_PARAM_REGEX = "((?:[^a-zA-Z0-9]|\\s)+)";
		public const String DELIMIT_COM_REGEX	= "([^a-zA-Z0-9]|\\s)*";
		public const String ANY_REGEX			= DELIMIT_COM_REGEX + "(?<" + ANY_SUBSTRING_NAME + ">.*)\\z";
		
		public const String VOTE_YES_ALTS_REGEX	= "\\A((?i)yes+|accept(s?|(ed)?)|approve(s?|d?)|y+|pass((es)?|(ed)?))\\z";
		public const String VOTE_NO_ALTS_REGEX	= "\\A((?i)no+|den(y?|(ies)?|(ied)?)|reject(s?|(ed)?)|n+|fail(s?|(ed)?))\\z";

		//Command regex
		public const String CREATE_REGEX		= "\\A(?i)create"	+ ANY_REGEX;
		public const String JOIN_REGEX			= "\\A(?i)join"		+ ANY_REGEX;
		public const String READY_REGEX			= "\\A(?i)ready"	+ ANY_REGEX;
		public const String PUT_REGEX			= "\\A(?i)put"		+ ANY_REGEX;
		public const String VOTE_REGEX			= "\\A(?i)vote"		+ ANY_REGEX;
		public const String PASS_REGEX			= "\\A(?i)pass"		+ ANY_REGEX;
		public const String FAIL_REGEX			= "\\A(?i)fail"		+ ANY_REGEX;
		public const String STATS_REGEX			= "\\A(?i)stats"	+ ANY_REGEX;
		public const String HELP_REGEX			= "\\A(?i)help"		+ ANY_REGEX;
		public const String NAME_CHANGE_REGEX	= "\\A(?i)name"		+ ANY_REGEX;


		//Delegates
		public delegate Boolean ParseAction(Player player, String[] input);

		//List of tuples
		List<Tuple<String, ParseAction>> RegexArray;

		public SMSParser()
		{
			RegexArray = new List<Tuple<String, ParseAction>>()
			{
				Tuple.Create<String, ParseAction>(CREATE_REGEX,			this.ParseCreate),
				Tuple.Create<String, ParseAction>(JOIN_REGEX,			this.ParseJoin),
				Tuple.Create<String, ParseAction>(READY_REGEX,			this.ParseReady),
				Tuple.Create<String, ParseAction>(PUT_REGEX,			this.ParsePut),
				Tuple.Create<String, ParseAction>(VOTE_REGEX,			this.ParseVote),
				Tuple.Create<String, ParseAction>(PASS_REGEX,			this.ParsePass),
				Tuple.Create<String, ParseAction>(FAIL_REGEX,			this.ParseFail),
				Tuple.Create<String, ParseAction>(STATS_REGEX,			this.ParseStats),
				Tuple.Create<String, ParseAction>(HELP_REGEX,			this.ParseHelp),
				Tuple.Create<String, ParseAction>(NAME_CHANGE_REGEX,	this.ParseNameChange)
			};
		}

		/// <summary>
		/// Finds the base command and throws it at function to handle it
		/// </summary>
		/// <param name="player">The player who sent of the command</param>
		/// <param name="input">The message sent by the player</param>
		public Boolean ParseStringInput(Player player, String input)
		{
			for (int x = 0; x < this.RegexArray.Count(); x++)
			{
				//Generate command matches
				Match commandMatch = new Regex(this.RegexArray[x].Item1).Match(input);
				System.Diagnostics.Debug.WriteLine("Attempting to match regex: " + this.RegexArray[x].Item1 + " with input: " + input);

				if (commandMatch.Success)
				{
					//Generate regex matches
					String paramString = new Regex(this.RegexArray[x].Item1).Match(input).Groups[ANY_SUBSTRING_NAME].ToString();
					String[] paramList = Regex.Split(paramString, DELIMIT_PARAM_REGEX);
					
					System.Diagnostics.Debug.WriteLine("Param String: " + paramString);

					//For debugging purposes to print out the list of params
					for (int y = 0; y < paramList.Length; y++)
					{
						System.Diagnostics.Debug.WriteLine("Params" + y + ": " + paramList[y]);
					}
 
					//Runs the associated function
					return this.RegexArray[x].Item2(player, paramList);
				}
			}

			//Invalid command
			return this.InvalidCommand(player, input);
		}

		public Boolean ParseCreate(Player player, String[] input)
		{
			//NOTE: param list in create doesn't matter, it's always true
			//		if "create" is typed first
			new GameController(null).CreateGame(player);

			return true;
		}

		public Boolean ParseJoin(Player player, String[] input)
		{
			new GameController(null).JoinGame(player, input[0]);
			return false;
		}

		public Boolean ParseReady(Player player, String[] input)
		{
			return false;
		}

		public Boolean ParsePut(Player player, String[] input)
		{
			return false;
		}

		public Boolean ParseVote(Player player, String[] input)
		{
			//check if there are params
			if(input.Length <=0)
			{
				throw new Exception("Exception at ParseVote, params cannot be empty");
			}

			Match yesMatch = new Regex(VOTE_YES_ALTS_REGEX).Match(input[0]);
			Match noMatch = new Regex(VOTE_NO_ALTS_REGEX).Match(input[0]);

			if(yesMatch.Success)
			{
				return true;
			}
			else if(noMatch.Success)
			{
				return true;
			}

			//no valid params, throw exception
			throw new Exception("Exception at ParseVote, params not valid");
		}

		public Boolean ParsePass(Player player, String[] input)
		{
			return false;
		}

		public Boolean ParseFail(Player player, String[] input)
		{
			return false;
		}

		public Boolean ParseStats(Player player, String[] input)
		{
			return false;
		}

		public Boolean ParseHelp(Player player, String[] input)
		{
			return false;
		}

		public Boolean ParseNameChange(Player player, String[] input)
		{
			return false;
		}

		public Boolean InvalidCommand(Player player, String input)
		{
			System.Diagnostics.Debug.WriteLine("Missed all cases");
			//throw exception?
			return false;
		}
	}
}