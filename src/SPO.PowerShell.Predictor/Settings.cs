using SPO.PowerShell.Predictor.Abstractions.Model;

namespace SPO.PowerShell.Predictor;

internal class Settings
{
    public CommandSearchMethod CommandSearchMethod { get; set; }
    
    private static CommandSearchMethod GetCommandSearchMethod()
    {
        var pnpPredictorCommandSearchMethod = Environment.GetEnvironmentVariable(SPOPowerShellPredictorConstants.EnvironmentVariableCommandSearchMethod);
        if (pnpPredictorCommandSearchMethod == null)
        {
            return CommandSearchMethod.Contains;
        }

        switch (pnpPredictorCommandSearchMethod)
        {
            case "Fuzzy":
                return CommandSearchMethod.Fuzzy;
            case "Contains":
                return CommandSearchMethod.Contains;
            case "StartsWith":
                return CommandSearchMethod.StartsWith;
            default:
                return CommandSearchMethod.StartsWith;
        }
    }
    
    public static Settings GetSettings()
    {
        return new Settings()
        {
            CommandSearchMethod = GetCommandSearchMethod()
        };
    }
}