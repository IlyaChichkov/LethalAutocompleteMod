using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace LethalAutocomplete
{
    internal class Autocomplete
    {
        public static Keybinds keybinds;
        
        public static string autocompleteKey = "<Keyboard>/tab";
        public static string historyNextKey = "<Keyboard>/upArrow";
        public static string historyPrevKey = "<Keyboard>/downArrow";

        public bool exited = false;
        
        private Terminal _terminal;
        private string _input;
        private TerminalKeyword[] _keywords;
        private List<string> _terminalCommands;
        private List<string> _commandsHistory;
        private int _historyIndex;
        public static int _historyMaxCount = 20;
	    
        private string _lastAutocomplete = "";
        private bool _startedAutocomplete = false;
        private List<string> _autocompleteOptions;
        private int _autocompleteOptionIndex = 0;
        
        public static ManualLogSource Logger;

        public void Awake()
        {
	        if(Plugin.IsDebug) Logger.LogInfo($"Lethal Autocomplete Plugin is loaded! Autocomplete Key: {autocompleteKey}");
	        _terminalCommands = new List<string>();
	        _commandsHistory = new List<string>();

	        SetupTerminalCallbacks();
	        SetupKeybindCallbacks();
        }

        private void OnTerminalTextChanged(object sender, TerminalTextChangedEventArgs e)
        {
	        try
	        {
		        _input = MyTerminalApi.GetTerminalInput(_terminal, Logger);
		        if (_input != _lastAutocomplete)
		        {
			        ResetAutocomplete();
		        }
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"On terminal text changed event. Error: {ex}");
	        }
        }

        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
	        if(Plugin.IsDebug) Logger.LogMessage("Terminal Exited");
            _commandsHistory = new List<string>();
        }

		private void TerminalIsStarting(object sender, TerminalEventArgs e)
		{
			try
			{
				if(Plugin.IsDebug) Logger.LogMessage("Terminal is starting");
				_terminal = TerminalApi.TerminalApi.Terminal;
				if (autocompleteKey.Contains("tab"))
				{
					RemoveTerminalExitBinding();
				}
				_keywords = _terminal.terminalNodes.allKeywords;
			}
			catch (Exception ex)
			{
				Logger.LogError($"On terminal starting. Error: {ex}");
			}
		}

		private void RemoveTerminalExitBinding()
		{
			try
			{
				var openMenu = _terminal.playerActions.Movement.OpenMenu;
				if(Plugin.IsDebug) Logger.LogMessage($"{openMenu.name} {openMenu.GetBindingDisplayString()}");
				openMenu.Disable();
				openMenu.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/tab", overridePath = "" } );
				openMenu.Enable();
				if(Plugin.IsDebug) Logger.LogMessage($"{openMenu.name} {openMenu.GetBindingDisplayString()}");
			}
			catch (Exception exception)
			{
				Logger.LogWarning("Remove terminal tab exit binded key");
				Logger.LogError(exception);
				throw;
			}
		}

		private void TerminalIsStarted(object sender, TerminalEventArgs e)
		{
			try
			{
				_terminalCommands.Clear();
				for (int i = 0; i < _keywords.Length; i++)
				{
					_terminalCommands.Add(_keywords[i].name);
				}
				for (int i = 0; i < _terminal.moonsCatalogueList.Length; i++)
				{
					_terminalCommands.Add(_terminal.moonsCatalogueList[i].PlanetName.Split(' ')[1]);
				}
			}
			catch (Exception exception)
			{
				Logger.LogWarning("Terminal is started");
				Logger.LogError(exception);
				throw;
			}
		}

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
	        try
	        {
		        if(_commandsHistory.Count + 1 > _historyMaxCount)
		        {
			        _commandsHistory.RemoveAt(0);
		        }
		        _commandsHistory.Add(e.SubmittedText);
		        _historyIndex = _commandsHistory.Count;
		        _input = "";
		        ResetAutocomplete();
	        }
	        catch (Exception ex)
	        {
		        Logger.LogWarning($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
		        Logger.LogError(ex);
	        }
        }

		private void OnBeginUsing(object sender, TerminalEventArgs e)
		{
            Logger.LogMessage("Player has just started using the terminal");
			
            if (autocompleteKey.Contains("tab"))
            {
	            HUDManager.Instance.ChangeControlTip(0, "Quit terminal : [Esc]", true);
            }
		}

        private void SetupTerminalCallbacks()
        {
	        TerminalStarting += TerminalIsStarting;
	        TerminalStarted += TerminalIsStarted;
	        TerminalParsedSentence += TextSubmitted;
	        TerminalBeginUsing += OnBeginUsing;
	        TerminalExited += OnTerminalExit;
	        TerminalTextChanged += OnTerminalTextChanged;
        }
        
        private void SetupKeybindCallbacks()
        {
	        keybinds.AutocompleteAction.performed += OnAutocompleteKey;
	        keybinds.HistoryNextAction.performed += OnHistoryNextKey;
	        keybinds.HistoryPrevAction.performed += OnHistoryPrevKey;
        }

        private void OnHistoryNextKey(InputAction.CallbackContext ctx)
        {
	        try
	        {
		        if (_commandsHistory.Count < 1) return;
		        if (_historyIndex < 1) return;
		        _historyIndex--;
		        SetTerminalInput(_commandsHistory[_historyIndex]);
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on history next key performed. Error: {ex}");
		        Logger.LogInfo($"_commandsHistory.Count={_commandsHistory.Count}");
		        Logger.LogInfo($"historyIndex={_historyIndex}");
	        }
        }

        private void OnHistoryPrevKey(InputAction.CallbackContext ctx)
        {
	        try
	        {
		        if (_commandsHistory.Count < 1) return;
		        if (_historyIndex + 1 >= _commandsHistory.Count)
		        {
			        _historyIndex = _commandsHistory.Count;
			        SetTerminalInput("");
			        return;
		        }
		        _historyIndex++;
		        SetTerminalInput(_commandsHistory[_historyIndex]);
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on history prev key performed. Error: {ex}");
		        Logger.LogInfo($"_commandsHistory.Count={_commandsHistory.Count}");
		        Logger.LogInfo($"historyIndex={_historyIndex}");
	        }
        }

        private void OnAutocompleteKey(InputAction.CallbackContext ctx)
        {
	        try
	        {
		        if (_startedAutocomplete)
		        {
			        NextAutocomplete();
		        }
		        else
		        {
			        StartAutocomplete();
		        }
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on autocomplete key performed. Error: {ex}");
		        Logger.LogInfo($"_startedAutocomplete={_startedAutocomplete}");
	        }
        }

        private void StartAutocomplete()
        {
	        try
	        {
		        var options = GetAutocompleteOption(_input, _terminalCommands);
		        if (options != null)
		        {
			        _autocompleteOptions = new List<string>(options);

			        _startedAutocomplete = true;
			        _lastAutocomplete = _autocompleteOptions.First();
			        Logger.LogMessage($"Set Autocomplete {_lastAutocomplete}");
			        SetTerminalInput(_lastAutocomplete);
		        }
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on autocomplete new options search. Error: {ex}");
	        }
        }

        private void NextAutocomplete()
        {
	        try
	        {
		        if (Plugin.IsDebug)
		        {
			        Logger.LogInfo("Autocomplete Options:");
			        for (int i = 0; i < _autocompleteOptions.Count; i++)
			        {
				        Logger.LogInfo(_autocompleteOptions[i]);
			        }
			        Logger.LogInfo($"Autocomplete Index: {_autocompleteOptionIndex + 1}");
		        }
		        
		        _autocompleteOptionIndex++;
		        if (_autocompleteOptionIndex >= _autocompleteOptions.Count)
		        {
			        _autocompleteOptionIndex = 0;
		        }
		        _lastAutocomplete = _autocompleteOptions[_autocompleteOptionIndex];
		        SetTerminalInput(_lastAutocomplete);
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on autocomplete next options search. Error: {ex}");
	        }
        }
        
        private void ResetAutocomplete()
        {
	        try
	        {
		        if (Plugin.IsDebug) Logger.LogInfo("Reset autocomplete state");
		        _startedAutocomplete = false;
		        _autocompleteOptionIndex = 0;
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on autocomplete reset. Error: {ex}");
	        }
        }
        
        private List<string> GetAutocompleteOption(string input, List<string> options)
        {
	        try
	        {
		        if (string.IsNullOrEmpty(input) || options == null || options.Count == 0)
			        return null;

		        List<string> filteredOptions = options.Where(option => option.ToLower().StartsWith(input.ToLower())).ToList();

		        if (filteredOptions.Count == 0)
			        return null;
		        
		        filteredOptions.Sort((a, b) => a.Length.CompareTo(b.Length));
		        return filteredOptions;
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on autocomplete search. Error: {ex}");
		        return null;
	        }
        }
    }
}