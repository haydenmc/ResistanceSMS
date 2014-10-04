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
		//Global regex
		public const String ANY_SUBSTRING_NAME = "ANY";
		public const String ANY_REGEX = "(?<" + ANY_SUBSTRING_NAME + ">.*)\\Z";
		
		//Command regex
		public const String CREATE_REGEX		= "\\A(?i)create"	+ ANY_REGEX;
		public const String JOIN_REGEX			= "\\A(?i)join"		+ ANY_REGEX;
		public const String READY_REGEX			= "\\A(?i)ready"	+ ANY_REGEX;
		public const String PUT_REGEX			= "\\A(?i)put"		+ ANY_REGEX;
		public const String VOTE_REGEX			= "\\A(?i)vote"		+ ANY_REGEX;
		public const String STATS_REGEX			= "\\A(?i)stats"	+ ANY_REGEX;
		public const String HELP_REGEX			= "\\A(?i)help"		+ ANY_REGEX;
		public const String NAME_CHANGE_REGEX	= "\\A(?i)name"		+ ANY_REGEX;

		//Delegates
		public delegate void ParseAction(Player player, String input);

		//List of tuples
		List<Tuple<String, ParseAction>> regexArray;

		public SMSParser()
		{
			regexArray = new List<Tuple<String, ParseAction>>()
			{
				Tuple.Create<String, ParseAction>(CREATE_REGEX,			this.ParseCreate),
				Tuple.Create<String, ParseAction>(JOIN_REGEX,			this.ParseJoin),
				Tuple.Create<String, ParseAction>(READY_REGEX,			this.ParseReady),
				Tuple.Create<String, ParseAction>(PUT_REGEX,			this.ParsePut),
				Tuple.Create<String, ParseAction>(VOTE_REGEX,			this.ParseVote),
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
		public void ParseStringInput(Player player, String input)
		{
			for (int x = 0; x < this.regexArray.Count(); x++)
			{
				if (System.Text.RegularExpressions.Regex.IsMatch(input, this.regexArray[x].Item1))
				{
					this.regexArray[x].Item2(player, new Regex(this.regexArray[x].Item1).Match(input).Groups[ANY_SUBSTRING_NAME].ToString());
				}
			}
		}
		
		public void ParseCreate(Player player, String input)
		{
			var gc = new GameController(null);
			gc.CreateGame(player);
		}

		public void ParseJoin(Player player, String input)
		{
            var gc = new GameController(player.CurrentGame);
		}

		public void ParseReady(Player player, String input)
		{

		}

		public void ParsePut(Player player, String input)
		{

		}

		public void ParseVote(Player player, String input)
		{

		}

		public void ParseStats(Player player, String input)
		{

		}

		public void ParseHelp(Player player, String input)
		{

		}

		public void ParseNameChange(Player player, String input)
		{

		}
	}
}