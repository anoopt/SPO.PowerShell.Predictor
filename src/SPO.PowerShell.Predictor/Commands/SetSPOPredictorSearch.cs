using System.Management.Automation;
using System.Reflection;
using System.Text;
using SPO.PowerShell.Predictor.Abstractions.Model;

namespace SPO.PowerShell.Predictor.Commands;

[Cmdlet(VerbsCommon.Set, "SPOPredictorSearch")]
public class SetPnPPredictorSearch : PSCmdlet
{
    private static readonly string[] ReloadModuleStatements = {
#if DEBUG
        $"Remove-Module {Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,SPOPowerShellPredictorConstants.LibraryName)} -Force",
        $"Import-Module {Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,SPOPowerShellPredictorConstants.LibraryName)} -Force"
#else
            "Remove-Module -Name SPO.PowerShell.Predictor -Force",
            "Import-Module -Name SPO.PowerShell.Predictor -Force"
#endif
            
    };


    [Parameter(Mandatory = true, Position = 0)]
    public CommandSearchMethod Method { get; set; }

    protected override void ProcessRecord()
    {
        var scriptToRun = new StringBuilder();
        var _ = scriptToRun.Append(string.Join(";", ReloadModuleStatements));

        if (Method.GetType() != typeof(CommandSearchMethod))
        {
            return;
        }
        
        Environment
            .SetEnvironmentVariable(
                SPOPowerShellPredictorConstants.EnvironmentVariableCommandSearchMethod,
                Method.ToString()
            );
        InvokeCommand.InvokeScript(scriptToRun.ToString());

    }
}