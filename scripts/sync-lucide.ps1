<#
.SYNOPSIS
    Refresh the vendored Lucide icon catalog at the version pinned in LUCIDE_VERSION.txt.

.DESCRIPTION
    Downloads the Lucide GitHub source archive for the pinned tag and copies the
    `icons/` directory into `catalog/lucide/icons/`. Idempotent: safe to re-run.

.EXAMPLE
    ./scripts/sync-lucide.ps1
    ./scripts/sync-lucide.ps1 -Version 0.480.0
#>
[CmdletBinding()]
param(
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$versionFile = Join-Path $repoRoot 'LUCIDE_VERSION.txt'
$catalogDir = Join-Path $repoRoot 'catalog\lucide'
$iconsDir = Join-Path $catalogDir 'icons'

if (-not $Version) {
    if (-not (Test-Path $versionFile)) {
        throw "LUCIDE_VERSION.txt not found at $versionFile"
    }
    $Version = (Get-Content $versionFile -Raw).Trim()
}

Write-Host "Syncing Lucide $Version -> $iconsDir" -ForegroundColor Cyan

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) "lucide-$Version-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $tmp -Force | Out-Null

try {
    $archiveUrl = "https://github.com/lucide-icons/lucide/archive/refs/tags/$Version.zip"
    $zipPath = Join-Path $tmp 'lucide.zip'

    Write-Host "  Downloading $archiveUrl"
    Invoke-WebRequest -Uri $archiveUrl -OutFile $zipPath -UseBasicParsing

    Write-Host "  Extracting"
    Expand-Archive -Path $zipPath -DestinationPath $tmp -Force

    $extracted = Get-ChildItem -Path $tmp -Directory | Where-Object { $_.Name -like 'lucide-*' } | Select-Object -First 1
    if (-not $extracted) { throw "Could not find extracted lucide-* directory in $tmp" }
    $srcIcons = Join-Path $extracted.FullName 'icons'
    if (-not (Test-Path $srcIcons)) { throw "Extracted archive has no 'icons' directory: $srcIcons" }

    if (Test-Path $iconsDir) { Remove-Item $iconsDir -Recurse -Force }
    New-Item -ItemType Directory -Path $iconsDir -Force | Out-Null

    Write-Host "  Copying icons/"
    Copy-Item -Path (Join-Path $srcIcons '*') -Destination $iconsDir -Recurse -Force

    $svgCount = (Get-ChildItem -Path $iconsDir -Filter '*.svg' | Measure-Object).Count
    Write-Host "  Done: $svgCount icons vendored." -ForegroundColor Green

    Set-Content -Path $versionFile -Value "$Version`n" -Encoding utf8 -NoNewline
}
finally {
    if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue }
}
