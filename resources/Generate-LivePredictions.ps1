try {
    
    $webRequest = Invoke-WebRequest "https://api.github.com/repos/MicrosoftDocs/OfficeDocs-SharePoint-PowerShell/contents/sharepoint/sharepoint-ps/sharepoint-online";
    $files = $webRequest.Content | ConvertFrom-Json;

    $suggestions = @();

    # create a regex pattern to match the example code
    $pattern = "(?s)(?<=``````powershell)(.*?)(?=``````)"
    $options = [Text.RegularExpressions.RegexOptions]'IgnoreCase, CultureInvariant';

    # loop through each file
    $files[0..2] | ForEach-Object {
    
        # get file name without extension md
        $baseName = $_.Name.ToLower().Replace(".md", "");

        # get the file data
        $fileData = Invoke-WebRequest $_.download_url;

        $result = [regex]::Matches($fileData.Content, $pattern, $options);

        $i = 1;
        foreach ($item in $result) {

            $value = $item.Value.Trim();

            # if the item value contains [ then don't add it to the json
            if ($value -match "\[") {
                continue;
            }

            # remove every thing after the first \n
            $value = $value.Split("`n")[0];

            # match multiple spaces and replace with a single space
            $value = $value -replace "\s+", " ";

            # remove `
            $value = $value -replace "`` ", "";

            # if the item value begins with the name of the file then add it to the json
            if ($value.ToLower() -match "^$($baseName).*") {

                $suggestions += @{
                    "Command" = $value.Trim()
                    "Rank"    = $i
                }
                $i++;
            }
        }
    }

    # add FileName, LastUpdatedOn and Suggestions to a new json object
    $json = [ordered]@{
        "FileName"      = "SPO.PowerShell.Suggestions.json"
        "LastUpdatedOn" = (Get-Date).ToString("dd MMMM yyyy")
        "Suggestions"   = $suggestions
    }

    # write the json to a file
    $json | ConvertTo-Json -Depth 10 | Out-File -FilePath ".\resources\SPO.PowerShell.Suggestions.json" -Encoding UTF8 -Force;
}
catch {
    Write-Error "Unable to create prediction commands file";
    Write-Error $_.Exception.Message;
    exit 1;
}