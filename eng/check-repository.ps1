# Purpose: Audits repository format, local Markdown links, ignored materials, and common secret assignments without changing files.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$strictUtf8 = [System.Text.UTF8Encoding]::new($false, $true)
$failures = [System.Collections.Generic.List[string]]::new()
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

    if ($files | Where-Object { $_ -like "reference-materials/*" }) {
        $failures.Add("reference-materials/ must remain ignored.")
    }

    foreach ($relativePath in $files) {
        $absolutePath = Join-Path $repositoryRoot $relativePath

        if ($binaryExtensions.Contains([System.IO.Path]::GetExtension($absolutePath))) {
            continue
        }

        $bytes = [System.IO.File]::ReadAllBytes($absolutePath)

        try {
            $content = $strictUtf8.GetString($bytes)
        }
        catch {
            $failures.Add("${relativePath}: invalid UTF-8.")
            continue
        }

        if ($bytes.Length -ge 3 -and
            $bytes[0] -eq 0xEF -and
            $bytes[1] -eq 0xBB -and
            $bytes[2] -eq 0xBF) {
            $failures.Add("${relativePath}: UTF-8 BOM is prohibited.")
        }

        if ($content.Contains("`r")) {
            $failures.Add("${relativePath}: CR or CRLF line ending detected.")
        }

        if (-not $content.EndsWith("`n")) {
            $failures.Add("${relativePath}: final newline missing.")
        }

        if ($content -match "(?m)[ `t]+$") {
            $failures.Add("${relativePath}: trailing whitespace detected.")
        }

        if ($content.Contains([char]0)) {
            $failures.Add("${relativePath}: NUL byte detected.")
        }

        if ($content -match "(?im)(api[_-]?key|client[_-]?secret|password|token)\s*[:=]\s*['`"][^'`"$<{][^'`"]{7,}['`"]") {
            $failures.Add("${relativePath}: possible committed secret assignment.")
        }

        if ([System.IO.Path]::GetExtension($absolutePath) -ne ".md") {
            continue
        }

        $matches = [regex]::Matches($content, "\[[^\]]*\]\(([^)]+)\)")

        foreach ($match in $matches) {
            $target = $match.Groups[1].Value.Trim().Trim("<", ">")

            if ($target -match "^(https?://|mailto:|#)") {
                continue
            }

            $pathPart = ($target -split "#", 2)[0]

            if ([string]::IsNullOrWhiteSpace($pathPart)) {
                continue
            }

            $decodedPath = [uri]::UnescapeDataString($pathPart)
            $resolvedPath = [System.IO.Path]::GetFullPath(
                (Join-Path (Split-Path -Parent $absolutePath) $decodedPath))

            if (-not (Test-Path -LiteralPath $resolvedPath)) {
                $failures.Add("${relativePath}: broken local link '${target}'.")
            }
        }
    }

    if ($failures.Count -gt 0) {
        throw ($failures -join [Environment]::NewLine)
    }

    Write-Output "Repository audit passed for $($files.Count) non-ignored files."
}
finally {
    Pop-Location
}
