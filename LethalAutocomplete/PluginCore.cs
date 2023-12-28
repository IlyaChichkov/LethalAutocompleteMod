using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Api;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace LethalAutocomplete
{
    [BepInPlugin(_GUID, _Name, _Version)]
    [BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.3.0")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", MinimumDependencyVersion: "0.4.2")]
    public partial class Plugin : BaseUnityPlugin
    {
	    private const string _GUID = "redeye.lethalautocomplete", _Name = "Lethal Autocomplete", _Version = "0.1.0";
	    private Terminal _terminal;
	    private string _input;
	    private TerminalKeyword[] _keywords;
	    private List<string> _terminalCommands;
	    private List<string> _commandsHistory;
	    private int _historyIndex;
	    private int _historyMaxCount = 20;
	    
	    private bool _autocompleteChanged;
	    private string _lastAutocomplete = "";
	    private bool _startedAutocomplete = false;
	    private List<string> _autocompleteOptions;
	    private int _autocompleteOptionIndex = 0;
	    
	    internal static AutocompleteInputs InputActionsInstance = new AutocompleteInputs();
	    
        private void Awake()
		{
			Logger.LogInfo("Lethal Autocomplete Plugin is loaded!");
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

			_terminalCommands = new List<string>();
			_commandsHistory = new List<string>();

			SetupTerminalCallbacks();
			SetupKeybindCallbacks();
		}

        private void OnTerminalTextChanged(object sender, TerminalTextChangedEventArgs e)
        {
	        Logger.LogInfo("OnTerminalTextChanged");
	        _input = GetTerminalInput();
	        Logger.LogMessage($"INPUT: {_input}");
	        Logger.LogMessage($"LAST: {_lastAutocomplete}");
	        if (_input != _lastAutocomplete)
	        {
		        ResetAutocomplete();
	        }
        }

        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal Exited");
            _commandsHistory = new List<string>();
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
			_terminal = TerminalApi.TerminalApi.Terminal;
			_keywords = _terminal.terminalNodes.allKeywords;
			Logger.LogMessage("Terminal is starting");
		}

		private void TerminalIsStarted(object sender, TerminalEventArgs e)
		{
			Logger.LogMessage("Terminal is started");
			
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

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
            if(_commandsHistory.Count + 1 > _historyMaxCount)
            {
	            _commandsHistory.RemoveAt(0);
            }
            _commandsHistory.Add(e.SubmittedText);
            _historyIndex = _commandsHistory.Count;
            _input = "";
            ResetAutocomplete();
        }

		private void OnBeginUsing(object sender, TerminalEventArgs e)
		{
            Logger.LogMessage("Player has just started using the terminal");
        }

        private void BeganUsing(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Player is using terminal");
        }

        private void SetupTerminalCallbacks()
        {
	        Logger.LogMessage("Bind terminal callbacks");
	        TerminalAwake +=  TerminalIsAwake;
	        TerminalWaking += TerminalIsWaking;
	        TerminalStarting += TerminalIsStarting;
	        TerminalStarted += TerminalIsStarted;
	        TerminalParsedSentence += TextSubmitted;
	        TerminalBeginUsing += OnBeginUsing;
	        TerminalBeganUsing += BeganUsing;
	        TerminalExited += OnTerminalExit;
	        TerminalTextChanged += OnTerminalTextChanged;
        }
        
        private void SetupKeybindCallbacks()
        {
	        Logger.LogMessage("Bind input callbacks");
	        InputActionsInstance.LeftShiftKey.performed += OnShiftKeyPressed;
	        InputActionsInstance.UpArrowKey.performed += OnUpArrowKeyPressed;
	        InputActionsInstance.DownArrowKey.performed += OnDownArrowKeyPressed;
        }

        private void OnUpArrowKeyPressed(InputAction.CallbackContext ctx)
        {
	        Logger.LogInfo($"_historyIndex {_historyIndex}");
	        if (_commandsHistory.Count < 1) return;
	        if (_historyIndex < 1) return;
	        _historyIndex--;
	        SetTerminalInput(_commandsHistory[_historyIndex]);
	        
        }

        private void OnDownArrowKeyPressed(InputAction.CallbackContext ctx)
        {
	        Logger.LogInfo($"_historyIndex {_historyIndex}");
	        if (_commandsHistory.Count < 1) return;
	        Logger.LogInfo($"_commandsHistory.Count {_commandsHistory.Count}");
	        if (_historyIndex + 1 >= _commandsHistory.Count)
	        {
		        _historyIndex = _commandsHistory.Count;
		        SetTerminalInput("");
		        return;
	        }
	        _historyIndex++;
	        SetTerminalInput(_commandsHistory[_historyIndex]);
        }

        private void OnShiftKeyPressed(InputAction.CallbackContext ctx)
        {
	        Logger.LogMessage($"--> OnShiftKeyPressed | started Autocomplete: {_startedAutocomplete}");
	        if (_startedAutocomplete)
	        {
		        NextAutocomplete();
	        }
	        else
	        {
		        StartAutocomplete();
	        }
        }

        private void StartAutocomplete()
        {
	        Logger.LogMessage("--> StartAutocomplete");
	        var options = GetAutocompleteOption(_input, _terminalCommands);
	        if (options != null)
	        {
		        _autocompleteOptions = new List<string>(options);
		        
		        _startedAutocomplete = true;
		        _autocompleteChanged = true;
		        _lastAutocomplete = _autocompleteOptions.First();
		        SetTerminalInput(_lastAutocomplete);
	        }
        }

        private void NextAutocomplete()
        {
	        Logger.LogMessage("Autocomplete Options:");
	        for (int i = 0; i < _autocompleteOptions.Count; i++)
	        {
		        Logger.LogMessage(_autocompleteOptions[i]);
	        }
	        Logger.LogMessage($"Autocomplete Index: {_autocompleteOptionIndex + 1}");
	        
	        _autocompleteOptionIndex++;
	        if (_autocompleteOptionIndex >= _autocompleteOptions.Count)
	        {
		        _autocompleteOptionIndex = 0;
		        Logger.LogMessage($"Autocomplete Index Reset");
	        }
	        Logger.LogMessage($"Set Autocomplete {_autocompleteOptions[_autocompleteOptionIndex]}");
	        _autocompleteChanged = true;
	        _lastAutocomplete = _autocompleteOptions[_autocompleteOptionIndex];
	        SetTerminalInput(_lastAutocomplete);
        }
        
        private void ResetAutocomplete()
        {
	        Logger.LogInfo(">> WARNING! ResetAutocomplete!");
	        _startedAutocomplete = false;
	        _autocompleteOptionIndex = 0;
        }
        
        private List<string> GetAutocompleteOption(string input, List<string> options)
        {
	        if (string.IsNullOrEmpty(input) || options == null || options.Count == 0)
		        return null;

	        List<string> filteredOptions = options.Where(option => option.ToLower().StartsWith(input.ToLower())).ToList();

	        if (filteredOptions.Count == 0)
		        return null;
	        
	        filteredOptions.Sort((a, b) => a.Length.CompareTo(b.Length));
	        return filteredOptions;
        }
    }
    
    public class AutocompleteInputs : LcInputActions 
    {
	    [InputAction("<Keyboard>/leftShift", Name = "LeftShift")]
	    public InputAction LeftShiftKey { get; set; }
	    
	    [InputAction("<Keyboard>/upArrow", Name = "UpArrow")]
	    public InputAction UpArrowKey { get; set; }
	    
	    [InputAction("<Keyboard>/downArrow", Name = "DownArrow")]
	    public InputAction DownArrowKey { get; set; }
    }
}