try {
    $suggestions = @();
    # get all files in the srcfiles folder
    $files = Get-ChildItem -Path ".\spopsdocs" -Filter "*.md" -Recurse;

    # loop through each file
    $files | ForEach-Object {
    
        # get file name without extension
        $baseName = $_.BaseName.ToLower();

        # get the file data
        $fileData = Get-Content $_.FullName -Raw;
        # create a regex pattern to match the example code
    
        $pattern = "(?s)(?<=``````powershell)(.*?)(?=``````)"
        $options = [Text.RegularExpressions.RegexOptions]'IgnoreCase, CultureInvariant';
    
        $result = [regex]::Matches($fileData, $pattern, $options);

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
                    "Command" = $value
                    "Rank"    = $i
                }
                $i++;
            }
        }
    }

    # add FileName, LastUpdatedOn and Suggestions to a new json object
    $json = [ordered]@{
        "FileName"       = "SPO.PowerShell.Suggestions.json"
        "LastUpdatedOn"  = (Get-Date).ToString("dd MMMM yyyy")
        "Suggestions"    = $suggestions
    }

    # write the json to a file
    $json | ConvertTo-Json -Depth 10 | Out-File -FilePath ".\SPO.PowerShell.Suggestions.json" -Encoding UTF8 -Force;   
}
catch {
    Write-Error "Unable to create prediction commands file";
    Write-Error $_.Exception.Message;
    exit 1;
}