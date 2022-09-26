[CmdletBinding(DefaultParameterSetName = 'Build')]
param(
    [Parameter(ParameterSetName = 'Build')]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)


$srcDir = Join-Path $PSScriptRoot 'src\SPO.PowerShell.Predictor'
dotnet publish -c $Configuration $srcDir

Write-Host "`nThe module 'SPO.PowerShell.Predictor' is published to 'SPO.PowerShell.Predictor.Module\SPO.PowerShell.Predictor'`n" -ForegroundColor Green