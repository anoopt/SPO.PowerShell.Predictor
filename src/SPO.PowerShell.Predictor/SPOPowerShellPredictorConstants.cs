namespace SPO.PowerShell.Predictor
{
    internal static class SPOPowerShellPredictorConstants
    {
        public const string SuggestionsFileName = "SPO.PowerShell.Suggestions.json";
        public const string SuggestionsFileRelativePath = "\\Data";
        public const string CommandsFilePath = "https://raw.githubusercontent.com/anoopt/SPO.PowerShell.Predictor/main/resources/SPO.PowerShell.Suggestions.json";
        public const string EnvironmentVariableCommandSearchMethod = "SPOPredictorCommandSearchMethod";
        public const string LibraryName = "SPO.PowerShell.Predictor.dll";
        public const string WarningMessageOnLoad = "WARNING: Predictions displayed will be as of {0}. So, you might not see some examples being predicted. Press enter to continue.";
        public const string GenericErrorMessage = "Unable to load predictions. Press enter to continue.";
    }
}
