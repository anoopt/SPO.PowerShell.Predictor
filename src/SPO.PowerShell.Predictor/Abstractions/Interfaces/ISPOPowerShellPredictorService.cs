using System.Management.Automation.Subsystem.Prediction;

namespace SPO.PowerShell.Predictor.Abstractions.Interfaces
{
    public interface ISPOPowerShellPredictorService
    {
        public List<PredictiveSuggestion>? GetSuggestions(PredictionContext context);
    }
}
