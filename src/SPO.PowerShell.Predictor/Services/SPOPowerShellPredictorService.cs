using SPO.PowerShell.Predictor.Abstractions.Interfaces;
using SPO.PowerShell.Predictor.Abstractions.Model;
using System.Management.Automation.Subsystem.Prediction;
using System.Reflection;
using System.Text.Json;
using System.Net.Http.Json;

namespace SPO.PowerShell.Predictor.Services
{
    public class SPOPowerShellPredictorService : ISPOPowerShellPredictorService
    {
        private List<Suggestion>? _allPredictiveSuggestions;
        private readonly HttpClient _client;
        private readonly string _commandsFilePath;

        public SPOPowerShellPredictorService()
        {
            _commandsFilePath = SPOPowerShellPredictorConstants.CommandsFilePath; //Add modifications in future if needed
            _client = new HttpClient();
            RequestAllPredictiveCommands();
        }
        protected virtual void RequestAllPredictiveCommands()
        {
            //TODO: Decide if we need to make an http request here to get all the commands
            //TODO: if the http request fails then fallback to local JSON file?
            _ = Task.Run(async () =>
            {
                try
                {
                    _allPredictiveSuggestions = await _client.GetFromJsonAsync<List<Suggestion>>(_commandsFilePath);
                }
                catch (Exception e)
                {
                    _allPredictiveSuggestions = null;
                }

                if (_allPredictiveSuggestions == null)
                {
                    try
                    {
                        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string fileName = Path.Combine($"{executableLocation}{SPOPowerShellPredictorConstants.SuggestionsFileRelativePath}", SPOPowerShellPredictorConstants.SuggestionsFileName);
                        string jsonString = await File.ReadAllTextAsync(fileName);
                        _allPredictiveSuggestions = JsonSerializer.Deserialize<List<Suggestion>>(jsonString)!;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("Unable to load predictions. Press enter to continue.");
                        Console.ResetColor();
                        _allPredictiveSuggestions = null;
                    }
                }

                
            });
        }

        public virtual List<PredictiveSuggestion>? GetSuggestions(PredictionContext context)
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

            //TODO: Decide how the source data should be structured and then add a logic to get filtered suggestions
            var filteredSuggestions = _allPredictiveSuggestions?.
                FindAll(pc => pc.Command.ToLower().StartsWith(input.ToLower())).
                OrderBy(pc => pc.Rank);

            var result = filteredSuggestions?.Select(fs => new PredictiveSuggestion(fs.Command)).ToList();

            return result;
        }
    }
}
