using System;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace LethalAutocomplete
{
    internal class AutocompleteManager
    {
        public static Keybinds keybinds;
        
        public static string saveFilePath = "";
        
        public static string autocompleteKey = "<Keyboard>/tab";
        public static string historyNextKey = "<Keyboard>/upArrow";
        public static string historyPrevKey = "<Keyboard>/downArrow";
        
        public static bool saveHistory = true;
        public static int historyMaxCount = 20;

        public bool exited = false;
        
        private Terminal _terminal;
        private string _input;
        private List<string> _terminalCommands;
        private List<string> _commandsHistory;
        private int _historyIndex;
	    
        private string _lastAutocomplete = "";
        private bool _startedAutocomplete = false;
        private List<string> _autocompleteOptions;
        private int _autocompleteOptionIndex = 0;
        
        public static ManualLogSource Logger;

        private Autocomplete _autocomplete;

        public void Awake()
        {
	        if(Plugin.IsDebug) Logger.LogInfo($"Lethal Autocomplete Plugin is loaded! Autocomplete Key: {autocompleteKey}");
	        _terminalCommands = new List<string>();
	        _commandsHistory = new List<string>();
	        _autocomplete = new Autocomplete();
	        Autocomplete.Logger = Logger;
	        
	        LoadFromJson();
	        SetupTerminalCallbacks();
        }

        private void OnTerminalTextChanged(object sender, TerminalTextChangedEventArgs e)
        {
	        try
	        {
		        _input = GetTerminalInput();
		        _input = _input.Replace("\n", "");
		        if(Plugin.IsDebug) Logger.LogMessage($"OnTerminalTextChanged: {_input}");
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
	        if (Plugin.IsDebug) Logger.LogMessage("Terminal Exited");
            if (!saveHistory) _commandsHistory = new List<string>();
            try
            {
	            RemoveKeybindCallbacks();
	            SaveToJson();
            }
            catch (Exception ex)
            {
	            Logger.LogError($"On terminal exit. Error: {ex}");
            }
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
				for (int i = 0; i < _terminal.terminalNodes.allKeywords.Length; i++)
				{
					_autocomplete.Insert(_terminal.terminalNodes.allKeywords[i].name, 10);
				}
				for (int i = 0; i < _terminal.moonsCatalogueList.Length; i++)
				{
					_autocomplete.Insert(_terminal.moonsCatalogueList[i].PlanetName.Split(' ')[1], 10);
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
			if(Plugin.IsDebug) Logger.LogMessage($"TextSubmitted: {_input}");
			try
			{
				if (e.SubmittedText != "")
				{
					// Save value in history
					if (_commandsHistory.Count + 1 > historyMaxCount)
					{
						_commandsHistory.RemoveAt(0);
					}
					_commandsHistory.Add(e.SubmittedText);
					_historyIndex = _commandsHistory.Count;
				}
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
            SetupKeybindCallbacks();
		}
		
		private void OnBeganUsing(object sender, TerminalEventArgs e)
		{
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
	        TerminalBeganUsing += OnBeganUsing;
	        TerminalExited += OnTerminalExit;
	        TerminalTextChanged += OnTerminalTextChanged;
        }
        
        private void SetupKeybindCallbacks()
        {
	        Logger.LogMessage("Setup Keybind Callbacks");
	        keybinds.AutocompleteAction.performed += OnAutocompleteKey;
	        keybinds.HistoryNextAction.performed += OnHistoryNextKey;
	        keybinds.HistoryPrevAction.performed += OnHistoryPrevKey;
        }
        
        private void RemoveKeybindCallbacks()
        {
	        Logger.LogMessage("Remove Keybind Callbacks");
	        keybinds.AutocompleteAction.performed -= OnAutocompleteKey;
	        keybinds.HistoryNextAction.performed -= OnHistoryNextKey;
	        keybinds.HistoryPrevAction.performed -= OnHistoryPrevKey;
        }

        private void OnHistoryNextKey(InputAction.CallbackContext ctx)
        {
	        SetTerminalInput("");
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
	        SetTerminalInput("");
	        try
	        {
		        if (_commandsHistory.Count < 1) return;
		        if (_historyIndex + 1 >= _commandsHistory.Count)
		        {
			        _historyIndex = _commandsHistory.Count;
			        SetTerminalInput("");
			        Logger.LogInfo($"INPUT: {GetTerminalInput()}");
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
		        var options = _autocomplete.GetAutocomplete(_input);
		        if (options != null && options.Count > 0)
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

        public void SaveToJson()
        {
	        var words = _autocomplete.GetWords();
	        var history = new Dictionary<string, List<string>>();
	        history["value"] = _commandsHistory;
	        
	        var combinedData = new
	        {
		        Words = words,
		        History = history
	        };

	        string jsonData = JsonConvert.SerializeObject(combinedData, Formatting.Indented);
	        File.WriteAllText(saveFilePath, jsonData);
        }
        
        class SaveData
        {
	        public Dictionary<string, int> Words { get; set; }
	        public Dictionary<string, List<string>> History { get; set; }
        }
        
        private void LoadFromJson()
        {
	        try
	        {
		        if (!File.Exists(saveFilePath)) return;

		        string jsonData = File.ReadAllText(saveFilePath);
		        if (string.IsNullOrEmpty(jsonData)) return;

		        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);
		        _commandsHistory = new List<string>(saveData.History["value"]);
		        _historyIndex = _commandsHistory.Count;
		        
		        for (int i = 0; i < _commandsHistory.Count; i++)
		        {
			        Logger.LogInfo(_commandsHistory[i]);
		        }
		        Logger.LogInfo($"_historyIndex={_historyIndex}");
		        _autocomplete.LoadSerializedWords(saveData.Words);
		        Logger.LogMessage($"Loaded save from JSON!");
	        }
	        catch (Exception ex)
	        {
		        Logger.LogError($"Failed on loading json save file. Error: {ex}");
	        }
        }
        
    }
}