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

		//Utils regex
		public const String DELIMIT_REGEX		= "[^a-zA-Z0-9]+";

		//Delegates
		public delegate Boolean ParseAction(Player player, String[] input);

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
		public Boolean ParseStringInput(Player player, String input)
		{
			for (int x = 0; x < this.regexArray.Count(); x++)
			{
				if (System.Text.RegularExpressions.Regex.IsMatch(input, this.regexArray[x].Item1))
				{
					String paramString = new Regex(this.regexArray[x].Item1).Match(input).Groups[ANY_SUBSTRING_NAME].ToString();
					String[] paramList = Regex.Split(paramString, DELIMIT_REGEX);
					return this.regexArray[x].Item2(player, paramList);
				}
			}
			return false;
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
	}
}