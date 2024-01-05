using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;

namespace LethalAutocomplete
{
    public class WordNode
    {
        public string Word { get; set; }
        public List<WordNode> Children { get; set; }
        public int Weight { get; set; } // Добавленная переменная Weight
    
        public WordNode(string word, int weight)
        {
            Word = word;
            Weight = weight;
            Children = new List<WordNode>();
        }
    
        public List<WordNode> FindMatchingWords(string[] inputs)
        {
            List<WordNode> matchingWords = new List<WordNode>();
    
            if (inputs.Length == 0 || Word.ToLower().StartsWith(inputs[0].ToLower()))
            {
                if (inputs.Length == 1)
                {
                    matchingWords.Add(this);
                }
                else
                {
                    foreach (var child in Children)
                    {
                        matchingWords.AddRange(child.FindMatchingWords(inputs.Skip(1).ToArray()));
                    }
                }
            }
    
            return matchingWords;
        }
    }
    
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