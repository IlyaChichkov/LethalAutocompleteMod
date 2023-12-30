using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;

namespace LethalAutocomplete
{
    [BepInPlugin(_GUID, _Name, _Version)]
    [BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.3.0")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", MinimumDependencyVersion: "0.4.2")]
    public partial class Plugin : BaseUnityPlugin
    {
	    private const string _GUID = "redeye.lethalautocomplete", _Name = "Lethal Autocomplete", _Version = "0.2.0";
	    
	    private Autocomplete _autocomplete;
	    
        private void Awake()
		{
			Logger.LogInfo("Lethal Autocomplete Plugin is loaded!");
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
			
			try
			{
				_autocomplete = new Autocomplete();
				Autocomplete.Logger = Logger;
				ConfigFile();
				Autocomplete.keybinds = new Keybinds();
				_autocomplete.Awake();
			}
			catch (Exception e)
			{
				Logger.LogError(e);
			}
		}
        private void ConfigFile()
        {
            ConfigEntry<string> c_autocompleteKey = Config.Bind("Keyboard Bindings", "Autocomplete", "<Keyboard>/tab", "Get autocomplete for current input");
            Autocomplete.autocompleteKey = c_autocompleteKey.Value.ToLower().StartsWith("<keyboard>") ? c_autocompleteKey.Value : $"<Keyboard>/{c_autocompleteKey.Value}";
            ConfigEntry<string> c_historyNextKey = Config.Bind("Keyboard Bindings", "History Next", "<Keyboard>/upArrow", "Get current terminal session next command");
            Autocomplete.historyNextKey = c_historyNextKey.Value.ToLower().StartsWith("<keyboard>") ? c_historyNextKey.Value : $"<Keyboard>/{c_historyNextKey.Value}";
            ConfigEntry<string> c_historyPrevKey = Config.Bind("Keyboard Bindings", "History Prev", "<Keyboard>/downArrow", "Get current terminal session prev command");
            Autocomplete.historyPrevKey = c_historyPrevKey.Value.ToLower().StartsWith("<keyboard>") ? c_historyPrevKey.Value : $"<Keyboard>/{c_historyPrevKey.Value}";
            ConfigEntry<int> c_historyBufferLength = Config.Bind("History", "Buffer Length", 20, "Max amount of commands to remember during terminal session");
            Autocomplete._historyMaxCount = c_historyBufferLength.Value;

        }
        
    }
}