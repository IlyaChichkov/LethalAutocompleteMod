using System;

namespace LethalAutocomplete
{
    public class MyTerminalApi
    {
        public static string GetTerminalInput(Terminal _terminal)
        {
            string result = "";
            try
            {
                if (_terminal != null)
                {
                    result = _terminal.currentText.Substring(_terminal.currentText.Length - _terminal.textAdded);
                }
            }
            catch (Exception e)
            {
                return result;
            }

            return result;
        }
    }
}