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
        private List<WordNode> _words;
        
        public List<string> blacklist;
        public static ManualLogSource Logger;

        private readonly int _defaultWeight = 10;
        
        public Autocomplete()
        {
            _words = new List<WordNode>();
            blacklist = new List<string>();
        }
        
        public void Insert(TerminalKeyword terminalKeyword)
        {
            try
            {
                string word = terminalKeyword.name;
                
                bool blackListWord = blacklist.Contains(word);
                if (ListContainsWord(word, _words))
                {
                    if (blackListWord)
                    {
                        _words = _words.Where(x => x.Word != word).ToList();
                    }
                    return;
                }
                if(blackListWord) return;
                
                WordNode node = new WordNode(word, _defaultWeight);

                if (terminalKeyword.compatibleNouns != null)
                {
                    for (int j = 0; j < terminalKeyword.compatibleNouns.Length; j++)
                    {
                        string noun = "";
                        string[] specialWords = new[] { "route", "info" };
                        if (specialWords.Any(word.ToLower().Contains)) 
                        {
                            noun = terminalKeyword.compatibleNouns[j].noun.word;
                        }
                        else
                        {
                            noun = (terminalKeyword.compatibleNouns[j].noun).ToString().Split(' ')[0];
                        }
                        var nounNode = new WordNode(noun, _defaultWeight);

                        if (word.ToLower() == "buy")
                        {
                            for (int k = 1; k < 10; k++)
                            {
                                nounNode.Children.Add(new WordNode(k.ToString(), 10 - k));
                            }
                        }
                        node.Children.Add(nounNode);
                    }
                }

                _words.Add(node);
            }
            catch (Exception exception)
            {
                string word = "None";
                if (terminalKeyword && terminalKeyword.name != "")
                {
                    word = terminalKeyword.name;
                }
                Logger.LogError($"Failed to add terminal keyword '{word}' in to autocomplete dictionary! Exception: {exception}");
            }
        }

        private bool ListContainsWord(string word, List<WordNode> list)
        {
            return list.Any(node => node.Word == word);
        }
        
        public List<string> GetAutocomplete(string input)
        {
            try
            {
                var inputs = input.Split(' ');
                List<WordNode> matchingNodes = new List<WordNode>();

                string matching_start = "";
                for (int i = 0; i < inputs.Length - 1; i++)
                {
                    matching_start += inputs[i] + " ";
                }
                
                foreach (var node in _words)
                {
                    matchingNodes.AddRange(node.FindMatchingWords(inputs));
                }
                
                matchingNodes = matchingNodes
                    .Distinct()
                    .Where(n => n.Weight > 0)
                    .OrderByDescending(n => n.Weight)
                    .ToList();

                return matchingNodes.Select(n => matching_start + n.Word).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed on autocomplete search. Error: {ex}");
                return null;
            }
        }

        public List<WordNode> GetWords()
        {
            return _words;
        }
        
        public void SetWords(List<WordNode> words)
        {
            _words = new List<WordNode>(words);
        }
    }
}