#Requires -Version 5.1
<#
.SYNOPSIS
    Installs git-cp for the current user on Windows.

.DESCRIPTION
    Downloads the latest git-cp-win-x64.exe from GitHub Releases,
    places it in %LOCALAPPDATA%\Programs\git-cp\, and adds that
    directory to the current user's PATH (no admin required).

    Once installed, both of the following work from any terminal:
        git-cp
        git cp

.EXAMPLE
    # One-liner install (PowerShell):
    irm https://raw.githubusercontent.com/lepepe/git-cp/main/install.ps1 | iex
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo      = 'lepepe/git-cp'
$assetName = 'git-cp-win-x64.exe'
$installDir = Join-Path $env:LOCALAPPDATA 'Programs\git-cp'
$binPath    = Join-Path $installDir 'git-cp.exe'

Write-Host "Fetching latest release info..." -ForegroundColor Cyan

$release = Invoke-RestMethod "https://api.github.com/repos/$repo/releases/latest"
$asset   = $release.assets | Where-Object { $_.name -eq $assetName } | Select-Object -First 1

if (-not $asset) {
    Write-Error "Asset '$assetName' not found in the latest release."
    exit 1
}

$version = $release.tag_name
Write-Host "Downloading git-cp $version..." -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $binPath

Write-Host "Installed to: $binPath" -ForegroundColor Green

# Add install directory to user PATH if not already present
$currentPath = [Environment]::GetEnvironmentVariable('PATH', 'User')
if ($currentPath -notlike "*$installDir*") {
    [Environment]::SetEnvironmentVariable('PATH', "$currentPath;$installDir", 'User')
    Write-Host "Added $installDir to your PATH." -ForegroundColor Green
    Write-Host "Restart your terminal for the PATH change to take effect." -ForegroundColor Yellow
} else {
    Write-Host "$installDir is already in your PATH." -ForegroundColor Gray
}

Write-Host ""
Write-Host "Done! Run 'git cp' (or 'git-cp') from inside any git repository." -ForegroundColor Green
