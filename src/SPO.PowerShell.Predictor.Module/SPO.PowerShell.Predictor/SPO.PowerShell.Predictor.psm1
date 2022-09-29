
$PSDefaultParameterValues.Clear()
Set-StrictMode -Version Latest

if ($true -and ($PSEdition -eq 'Desktop')) {
    throw "Windows PowerShell is not supported. Please install PowerShell Core 7.2 or higher version."
}

if ($true -and ($PSEdition -eq 'Core')) {
    if ($PSVersionTable.PSVersion -lt [Version]'7.2.0') {
        throw "Current SPO.PowerShell.Predictor version doesn't support PowerShell Core versions lower than 7.2.0. Please upgrade to PowerShell Core 7.2.0 or higher. "
    }
}

$psReadlineModule = Get-Module -Name PSReadLine
$minimumRequiredVersion = [version]"2.2.2"
$shouldImportPredictor = $true

if ($psReadlineModule -ne $null -and $psReadlineModule.Version -lt $minimumRequiredVersion) {
    $shouldImportPredictor = $false
    throw "This module requires PSReadLine version $minimumRequiredVersion. An earlier version of PSReadLine is imported in the current PowerShell session. Please open a new session before importing this module. "
}
elseif ($psReadlineModule -eq $null) {
    try {
        Import-Module PSReadLine -MiniumVersion $minimumRequiredVersion -Scope Global
    }
    catch {
        $shouldImportPredictor = $false
        throw "This module requires PSReadLine version $minimumRequiredVersion. Please install PSReadLine $minimumRequiredVersion or higher. "
    }
}

$spoPowerShellModule = Get-Module -Name Microsoft.Online.SharePoint.PowerShell -ListAvailable | Select Name,Version

if($spoPowerShellModule -eq $null) {
    $shouldImportPredictor = $false
    throw "Please make sure you either have SharePoint Online Management shell installed or have installed Microsoft.Online.SharePoint.PowerShell module. See - https://learn.microsoft.com/en-us/powershell/sharepoint/sharepoint-online/connect-sharepoint-online?view=sharepoint-ps"
} else {
    Import-Module -Name Microsoft.Online.SharePoint.PowerShell
}

# Import the predictor module
if ($shouldImportPredictor) {
    try {
        Import-Module (Join-Path -Path $PSScriptRoot -ChildPath SPO.PowerShell.Predictor.dll);
    }
    catch {
        Write-Error "An error occurred while importing the module. Details:"
        Write-Error $_
    }
}