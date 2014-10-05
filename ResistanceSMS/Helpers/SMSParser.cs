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
		public const String MY_STATS_REGEX		= "\\A(?i)mystats"	+ ANY_REGEX;
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
				Tuple.Create<String, ParseAction>(MY_STATS_REGEX,		this.ParsePlayerStats),
				Tuple.Create<String, ParseAction>(HELP_REGEX,			this.ParseHelp),
				Tuple.Create<String, ParseAction>(NAME_CHANGE_REGEX,	this.ParseNameChange)
			};
		}

		/// <summary>
		/// Takes input and parses it based on the command and its parameters, then
		/// sends it to the associated function for processing
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

		/// <summary>
		/// Function used for creating the game, no parameters are needed
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseCreate(Player player, String[] input)
		{
			//NOTE: param list in create doesn't matter, it's always true
			//		if "create" is typed first
			new GameController(null).CreateGame(player);

			return true;
		}

		/// <summary>
		/// Function used for joining an existing game, the first parameter should
		/// be a number representating the game to be joined
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseJoin(Player player, String[] input)
		{
			//check if there are params
			if (input.Length <= 0)
			{
				throw new Exception("Exception at ParseJoin, params cannot be empty");
			}

			//Sends the game id over to the GameController
			new GameController(null).JoinGame(player, input[0]);
			return true;
		}

		/// <summary>
		/// Function used to tell the server if you are ready or not, paramets are not
		/// required
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseReady(Player player, String[] input)
		{
			//NOTE: always ready, we dont have a way for the player to say not ready
			new GameController(player.CurrentGame).PlayerIsReady(player, true);
			return true;
		}

		/// <summary>
		/// Function used to tell the server who to put on a mission, each parameter is
		/// a name of the player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParsePut(Player player, String[] input)
		{
			new GameController(player.CurrentGame).SelectMissionPlayers(player, input);
			return true;
		}

		/// <summary>
		/// Function used to tell the server whether you voted yes or no (or variants of
		/// them), first parameter is yes or no or variants of them
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseVote(Player player, String[] input)
		{
			//check if there are params
			if(input == null || input[0].Equals(""))
			{
				throw new Exception("Exception at ParseVote, params cannot be empty");
			}

			Match yesMatch = new Regex(VOTE_YES_ALTS_REGEX).Match(input[0]);
			Match noMatch = new Regex(VOTE_NO_ALTS_REGEX).Match(input[0]);

			if(yesMatch.Success)
			{
				new GameController(player.CurrentGame).PlayerVote(player, true);
				return true;
			}
			else if(noMatch.Success)
			{
				new GameController(player.CurrentGame).PlayerVote(player, false);
				return true;
			}

			//no valid params, throw exception
			throw new Exception("Exception at ParseVote, params not valid");
		}

		/// <summary>
		/// Function used to tell the server if you passed a mission, no parameters 
		/// are needed
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParsePass(Player player, String[] input)
		{
			new GameController(player.CurrentGame).CheckPassOrFail(player, true);
			return true;
		}

		/// <summary>
		/// Function used to tell the server if you failed a mission, no parameters
		/// are needed
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseFail(Player player, String[] input)
		{
			new GameController(player.CurrentGame).CheckPassOrFail(player, false);
			return true;
		}

		/// <summary>
		/// Function used to tell the server to return stats to the player, no 
		/// parameter means it's asking for game stats, a parameter means it's
		/// requesting stats of a player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseStats(Player player, String[] input)
		{
			//check for null
			if(input == null)
			{
				throw new Exception("Exception at ParseStats, params cannot be empty");
			}

			//if input[0] is empty then its asking for game stats
			if(input[0].Equals(""))
			{
				//TODO: call game stats
				new GameController(player.CurrentGame).RequestStats(player, "");
				return true;
			}
			else
			{
				return this.ParseStats(player, input);
			}
		}

		/// <summary>
		/// Function used to tell the server to return player stats to the player, if
		/// parameter is empty, return player's own stats, else the listed player's
		/// stats
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParsePlayerStats(Player player, String[] input)
		{
			//check for null
			if (input == null)
			{
				throw new Exception("Exception at ParseMyStats, params cannot be empty");
			}

			//if input[0] is empty then its asking for my stats
			if (input[0].Equals(""))
			{
				new GameController(player.CurrentGame).RequestStats(player, player.Name);
				return true;
			}
			else
			{
				new GameController(player.CurrentGame).RequestStats(player, input[0]);
				return true;
			}
		}

		/// <summary>
		/// Function used to tell the server the user wants the help page, parameters may
		/// be used to determine subject
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseHelp(Player player, String[] input)
		{
			//TODO: add parameter and call GameController function
			return true;
		}

		/// <summary>
		/// Function used to tell the server the user wants a name game, first parameter
		/// should be the desired name
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean ParseNameChange(Player player, String[] input)
		{
			//check if there are params
			if (input.Length <= 0)
			{
				throw new Exception("Exception at ParseNameChange, params cannot be empty");
			}

			new GameController(player.CurrentGame).ChangeName(player, input[0]);
			return true;
		}

		/// <summary>
		/// This function is called if the command set by the player does not match
		/// any listed command
		/// </summary>
		/// <param name="player"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public Boolean InvalidCommand(Player player, String input)
		{
			System.Diagnostics.Debug.WriteLine("Missed all cases");
			new GameController(player.CurrentGame).InvalidCommand(player, input);
			return false;
		}
	}
}