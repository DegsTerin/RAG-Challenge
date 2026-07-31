# Purpose: Normalises repository text files to the required UTF-8, LF, final-newline, and no-trailing-whitespace policy.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
$binaryExtensions = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)

foreach ($extension in @(
    ".gif",
    ".jpeg",
    ".jpg",
    ".pdf",
    ".png",
    ".woff",
    ".woff2"
)) {
    [void]$binaryExtensions.Add($extension)
}

Push-Location $repositoryRoot

try {
    $files = git ls-files --cached --others --exclude-standard

    if ($LASTEXITCODE -ne 0) {
        throw "Git could not enumerate repository files."
    }

    foreach ($relativePath in $files) {
        $absolutePath = Join-Path $repositoryRoot $relativePath

        if ($binaryExtensions.Contains([System.IO.Path]::GetExtension($absolutePath))) {
            continue
        }

        $content = [System.IO.File]::ReadAllText($absolutePath)
        $normalisedLines = ($content -replace "`r`n?", "`n") -split "`n"
        $normalised = ($normalisedLines | ForEach-Object { $_.TrimEnd() }) -join "`n"
        $normalised = $normalised.TrimEnd("`n") + "`n"
        [System.IO.File]::WriteAllText($absolutePath, $normalised, $utf8WithoutBom)
    }
}
finally {
    Pop-Location
}
