using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;

namespace LethalAutocomplete
{
    public class Autocomplete
    {
        private Dictionary<string, int> _words;
        public static ManualLogSource Logger;

        public Autocomplete()
        {
            _words = new Dictionary<string, int>();
        }
        
        public void Insert(string word, int weight)
        {
            if(!_words.Keys.Contains(word))
                _words.Add(word, weight);
        }

        public List<string> GetAutocomplete(string input)
        {
            try
            {
                return _words
                    .Where(option => option.Key.StartsWith(input, StringComparison.OrdinalIgnoreCase) && option.Value > 0)
                    .OrderByDescending(option => option.Value)
                    .Select(option => option.Key)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed on autocomplete search. Error: {ex}");
                return null;
            }
        }

        public Dictionary<string, int> GetWords()
        {
            return _words;
        }
        
        
        public void LoadSerializedWords(Dictionary<string, int> words)
        {
            _words = new Dictionary<string, int>(words);
        }
    }
}