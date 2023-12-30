using System;
using BepInEx.Logging;

namespace LethalAutocomplete
{
    public class MyTerminalApi
    {
        public static string GetTerminalInput(Terminal _terminal, ManualLogSource Logger)
        {
            string result = "";
            try
            {
                if (_terminal != null)
                {
                    result = _terminal.currentText.Substring(_terminal.currentText.Length - _terminal.textAdded);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed on getting terminal input. Error: {ex}");
                return result;
            }

            return result;
        }
    }
}