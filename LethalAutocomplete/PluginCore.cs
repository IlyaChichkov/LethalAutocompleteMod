using BepInEx;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using LethalCompanyInputUtils.Api;

using TerminalApi;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace LethalAutocomplete
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.3.0")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", MinimumDependencyVersion: "0.4.2")]
    public partial class Plugin : BaseUnityPlugin
    {
	    private const string modGUID = "redeye.lethalautocomplete", modName = "Lethal Autocomplete", modVersion = "1.0.4";
        private void Awake()
		{
			Logger.LogInfo("Plugin Test Plugin is loaded!");

			TerminalAwake +=  TerminalIsAwake;
			TerminalWaking += TerminalIsWaking;
			TerminalStarting += TerminalIsStarting;
			TerminalStarted += TerminalIsStarted;
			TerminalParsedSentence += TextSubmitted;
			TerminalBeginUsing += OnBeginUsing;
			TerminalBeganUsing += BeganUsing;
			TerminalExited += OnTerminalExit;
            TerminalTextChanged += OnTerminalTextChanged;

			// Will display 'World' when 'hello' is typed into the terminal
			AddCommand("hello", "World\n");

			// Will display 'Sorry but you cannot run kill' when 'run kill' is typed into the terminal
			// Will also display the same thing as above if you just type 'kill' into the terminal 
			// because the default verb will be 'run'
			AddCommand("kill", "Sorry but you cannot run kill\n", "run");

			// All the code below is essentially the same as the line of code above
            TerminalNode triggerNode = CreateTerminalNode($"Frank is not available right now.\n", true);
            TerminalKeyword verbKeyword = CreateTerminalKeyword("get", true);
            TerminalKeyword nounKeyword = CreateTerminalKeyword("frank");

            verbKeyword = verbKeyword.AddCompatibleNoun(nounKeyword, triggerNode);
            nounKeyword.defaultVerb = verbKeyword;

            AddTerminalKeyword(verbKeyword);
            AddTerminalKeyword(nounKeyword);


        }

        private void OnTerminalTextChanged(object sender, TerminalTextChangedEventArgs e)
        {
			string userInput = GetTerminalInput();
			Logger.LogMessage(userInput);
			// Or
			Logger.LogMessage(e.CurrentInputText);

			// If user types in fuck it will changed to frick before they can even submit
			if(userInput == "fuck")
			{
				SetTerminalInput("frick");
			}
			
        }

        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal Exited");
        }

        private void TerminalIsAwake(object sender, TerminalEventArgs e)
		{
			Logger.LogMessage("Terminal is awake");
		}

		private void TerminalIsWaking(object sender, TerminalEventArgs e)
		{
			Logger.LogMessage("Terminal is waking");
		}

		private void TerminalIsStarting(object sender, TerminalEventArgs e)
		{
			Logger.LogMessage("Terminal is starting");
			
			
			Logger.LogInfo($"|Plugin Terminal Autocomplete loaded keywors|");
			TerminalKeyword[] keywords = Terminal.terminalNodes.allKeywords;

			List<string> commands = new List<string>(){""};
			for (int i = 1; i < keywords.Length; i++)
			{
				Logger.LogInfo($"{keywords[i].name}");
				commands.Add(keywords[i].name);
			}
			Logger.LogInfo($"|Plugin Terminal Autocomplete loaded keywors|");
		}

		private void TerminalIsStarted(object sender, TerminalEventArgs e)
		{
			Logger.LogMessage("Terminal is started");
		}

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
        }

		private void OnBeginUsing(object sender, TerminalEventArgs e)
		{
            Logger.LogMessage("Player has just started using the terminal");
        }

        private void BeganUsing(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Player is using terminal");
        }

    }
}