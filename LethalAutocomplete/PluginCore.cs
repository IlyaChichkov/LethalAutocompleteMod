using System;
using System.IO;
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
	    private const string _GUID = "redeye.lethalautocomplete", _Name = "Lethal Autocomplete", _Version = "0.4.4";
	    public static bool IsDebug = false;
	    private AutocompleteManager _autocomplete;

	    public string PluginPath = "";
	    
        private void Awake()
		{
			Logger.LogInfo("Lethal Autocomplete Plugin is loaded!");
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

			try
			{
				PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}
			
			try
			{
				
				_autocomplete = new AutocompleteManager();
				AutocompleteManager.Logger = Logger;
				ConfigFile();
				AutocompleteManager.keybinds = new Keybinds();
				_autocomplete.Awake();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}
		}

        private void OnApplicationQuit()
        {
	        _autocomplete.SaveToJson();
        }

        private void ConfigFile()
        {
	        string defaultSaveFileName = "save.json";
	        string defaultSaveFilePath = Path.Combine(PluginPath, defaultSaveFileName);
	        
	        ConfigEntry<string> c_saveFilePath = Config.Bind("Basic", "Save Data Path", "", "Absolute path to the json file with autocomplete words and commands history. By default save.json generated in plugins/red_eye-LethalAutocomplete folder.");
	        string path = c_saveFilePath.Value;
	        if (!File.Exists(path) && path != "")
	        {
		        path = defaultSaveFilePath;
		        Config.Remove(new ConfigDefinition("Basic", "Save Data Path"));
		        Config.Bind("Basic", "Save Data Path", defaultSaveFilePath, "Absolute path to the json file with autocomplete words and commands history. By default save.json generated in plugins/red_eye-LethalAutocomplete folder.");
		        Logger.LogWarning($"The save file wasn't found in the directory specified by the configuration file! Using default path.");
	        }
	        AutocompleteManager.saveFilePath = path == "" ? defaultSaveFilePath : path;
	        
            ConfigEntry<string> c_autocompleteKey = Config.Bind("Keyboard Bindings", "Autocomplete", "<Keyboard>/tab", "Get autocomplete for current input");
            AutocompleteManager.autocompleteKey = c_autocompleteKey.Value.ToLower().StartsWith("<keyboard>") ? c_autocompleteKey.Value : $"<Keyboard>/{c_autocompleteKey.Value}";
            ConfigEntry<string> c_historyNextKey = Config.Bind("Keyboard Bindings", "History Next", "<Keyboard>/upArrow", "Get current terminal session next command");
            AutocompleteManager.historyNextKey = c_historyNextKey.Value.ToLower().StartsWith("<keyboard>") ? c_historyNextKey.Value : $"<Keyboard>/{c_historyNextKey.Value}";
            ConfigEntry<string> c_historyPrevKey = Config.Bind("Keyboard Bindings", "History Prev", "<Keyboard>/downArrow", "Get current terminal session prev command");
            AutocompleteManager.historyPrevKey = c_historyPrevKey.Value.ToLower().StartsWith("<keyboard>") ? c_historyPrevKey.Value : $"<Keyboard>/{c_historyPrevKey.Value}";
            ConfigEntry<bool> c_historySave = Config.Bind("History", "Save History", true, "Regulates if the history be saved after the re-entry");
            AutocompleteManager.saveHistory = c_historySave.Value;
            ConfigEntry<int> c_historyBufferLength = Config.Bind("History", "Buffer Length", 20, "Max amount of commands to remember during terminal session");
            AutocompleteManager.historyMaxCount = c_historyBufferLength.Value;
            ConfigEntry<bool> c_debugMode = Config.Bind("Other", "Enable Debug", false, "");
            IsDebug = c_debugMode.Value;
        }
        
    }
}