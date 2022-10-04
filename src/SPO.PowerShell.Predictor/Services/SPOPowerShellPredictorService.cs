using SPO.PowerShell.Predictor.Abstractions.Interfaces;
using SPO.PowerShell.Predictor.Abstractions.Model;
using System.Management.Automation.Subsystem.Prediction;
using System.Reflection;
using System.Text.Json;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SPO.PowerShell.Predictor.Utilities;

namespace SPO.PowerShell.Predictor.Services
{
    internal sealed class SPOPowerShellPredictorService : ISPOPowerShellPredictorService
    {
        private SuggestionsFile? _suggestionsFile;
        private List<Suggestion>? _allPredictiveSuggestions;
        private readonly HttpClient _client;
        private readonly string? _commandsFilePath;
        private readonly CommandSearchMethod _commandSearchMethod;

        public SPOPowerShellPredictorService(Settings settings)
        {
            _commandsFilePath = SPOPowerShellPredictorConstants.CommandsFilePath; //Add modifications in future if needed
            _client = new HttpClient();
            _commandSearchMethod = settings.CommandSearchMethod;
            RequestAllPredictiveCommands();
        }

        private void SetPredictiveSuggestions()
        {
            var lastUpdatedOn = _suggestionsFile?.LastUpdatedOn;
            _allPredictiveSuggestions = _suggestionsFile?.Suggestions;
            UpdateCommandNameInSuggestions();
            RemoveInvalidSuggestions();

            var today = DateTime.Now.ToString("dd MMMM yyyy");

            if (lastUpdatedOn != today)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(string.Format(SPOPowerShellPredictorConstants.WarningMessageOnLoad, lastUpdatedOn));
                Console.ResetColor();
            }
        }
        
        private void UpdateCommandNameInSuggestions()
        {
            //if _allPredictiveSuggestions is null, then return
            if (_allPredictiveSuggestions == null)
            {
                return;
            }
            
            //if the first suggestion has the CommandName property, then return
            if (!string.IsNullOrEmpty(_allPredictiveSuggestions[0].CommandName))
            {
                return;
            }
            
            //For each suggestion in the list, set the CommandName property to the first word in the Command property using Regex
            foreach (var suggestion in _allPredictiveSuggestions)
            {
                if (suggestion.Command != null)
                    suggestion.CommandName = Regex.Match(suggestion.Command, @"^\S+").Value;
            }
        }
        
        private void RemoveInvalidSuggestions()
        {
            //if _allPredictiveSuggestions is null, then return
            if (_allPredictiveSuggestions == null)
            {
                return;
            }
            
            //filter out suggestions where CommandName and Command are not null or empty
            _allPredictiveSuggestions = _allPredictiveSuggestions.Where(suggestion => !string.IsNullOrEmpty(suggestion.CommandName) && !string.IsNullOrEmpty(suggestion.Command))?.ToList();
        }

        private void RequestAllPredictiveCommands()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    _suggestionsFile = await _client.GetFromJsonAsync<SuggestionsFile>(_commandsFilePath);
                    SetPredictiveSuggestions();
                }
                catch (Exception)
                {
                    _allPredictiveSuggestions = null;
                }

                if (_allPredictiveSuggestions == null)
                {
                    try
                    {
                        var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var fileName = Path.Combine($"{executableLocation}{SPOPowerShellPredictorConstants.SuggestionsFileRelativePath}", SPOPowerShellPredictorConstants.SuggestionsFileName);
                        var jsonString = await File.ReadAllTextAsync(fileName);
                        _suggestionsFile = JsonSerializer.Deserialize<SuggestionsFile>(jsonString);
                        SetPredictiveSuggestions();
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(SPOPowerShellPredictorConstants.GenericErrorMessage);
                        Console.ResetColor();
                        _allPredictiveSuggestions = null;
                    }
                }

                
            });
        }
        
        private IEnumerable<Suggestion>? GetFilteredSuggestions(string input)
        {
            IEnumerable<Suggestion>? filteredSuggestions = null;
            #region Search

            switch (_commandSearchMethod)
            {
                
                case CommandSearchMethod.StartsWith:
                    filteredSuggestions = _allPredictiveSuggestions
                        ?.Where(pc => pc.CommandName != null && pc.CommandName.ToLower().StartsWith(input.ToLower()))
                        .OrderBy(pc => pc.Rank);
                    break;
                
                case CommandSearchMethod.Contains:
                    filteredSuggestions = _allPredictiveSuggestions
                        ?.Where(pc => pc.CommandName != null && pc.CommandName.ToLower().Contains(input.ToLower()))
                        .OrderBy(pc => pc.Rank);
                    break;
                
                case CommandSearchMethod.Fuzzy:
                {
                    var inputWithoutSpaces = Regex.Replace(input, @"\s+", "");

                    var matches = new List<Suggestion>();

                    foreach (var suggestion in CollectionsMarshal.AsSpan(_allPredictiveSuggestions))
                    {
                        FuzzyMatcher.Match(suggestion.CommandName, inputWithoutSpaces, out var score);
                        suggestion.Rank = score;
                        matches.Add(suggestion);
                    }

                    filteredSuggestions = matches.OrderByDescending(m => m.Rank);
                    break;
                }
            }

            #endregion
            
            return filteredSuggestions;
        }

        public List<PredictiveSuggestion>? GetSuggestions(PredictionContext context)
        {
            var input = context.InputAst.Extent.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (_allPredictiveSuggestions == null)
            {
                return null;
            }

            var filteredSuggestions = GetFilteredSuggestions(input);
            
            /*var filteredSuggestions = _allPredictiveSuggestions?.
                Where(pc => pc.Command != null && pc.Command.ToLower().StartsWith(input.ToLower())).
                OrderBy(pc => pc.Rank);*/

            var result = filteredSuggestions?.Select(fs => new PredictiveSuggestion(fs.Command)).ToList();

            return result;
        }
    }
}
