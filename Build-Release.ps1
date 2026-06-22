# Build-Release.ps1
# Builds ArchSmarter Charrette for specified Revit versions and stages output
# for Advanced Installer packaging.
#
# Usage:
#   .\Build-Release.ps1                    # Build only (stage files)
#   .\Build-Release.ps1 -BuildMsi          # Build + run Advanced Installer
#   .\Build-Release.ps1 -RevitVersions 25,26  # Build specific versions

param(
    [int[]]$RevitVersions = @(25, 26, 27),
    [switch]$BuildMsi,
    [string]$AipFile = "$PSScriptRoot\Installer\ArchSmarterCharrette.aip",
    [string]$AdvancedInstallerPath = "C:\Program Files (x86)\Caphyon\Advanced Installer 22.8\bin\x86\AdvancedInstaller.com"
)

$ErrorActionPreference = 'Stop'

$SolutionRoot = $PSScriptRoot
$AddinProject = "$SolutionRoot\ArchSmarterCharrette\ArchSmarterCharrette.csproj"
$VideoProject = "$SolutionRoot\ArchSmarterCharrette.VideoTool\ArchSmarterCharrette.VideoTool.csproj"
$StageRoot    = "$SolutionRoot\BuildOutput"

# ── Clean staging area ───────────────────────────────────────────────────────
if (Test-Path $StageRoot) {
    Remove-Item $StageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $StageRoot -Force | Out-Null

# ── Build the VideoTool (version-independent, built once) ────────────────────
Write-Host "`n=== Building VideoTool ===" -ForegroundColor Cyan

dotnet build $VideoProject -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw "VideoTool build failed." }

$VideoToolOutput = "$SolutionRoot\ArchSmarterCharrette.VideoTool\bin\Release"

# Stage VideoTool to a shared folder
$VideoToolStage = "$StageRoot\VideoTool"
New-Item -ItemType Directory -Path $VideoToolStage -Force | Out-Null
Copy-Item "$VideoToolOutput\*" $VideoToolStage -Recurse -Force
Write-Host "  Staged VideoTool -> $VideoToolStage" -ForegroundColor Green

# ── Build the add-in for each Revit version ──────────────────────────────────
foreach ($ver in $RevitVersions) {
    $config = "Release R$ver"
    $revitYear = "20$ver"

    Write-Host "`n=== Building for Revit $revitYear ($config) ===" -ForegroundColor Cyan

    dotnet build $AddinProject -c "$config" --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $config." }

    $buildOutput = "$SolutionRoot\ArchSmarterCharrette\bin\Release\$revitYear"

    # Stage: Addins\{year}\ArchSmarterCharrette\  (DLLs)
    #        Addins\{year}\                        (.addin manifest)
    $addinStage = "$StageRoot\Revit$revitYear\Addins\$revitYear"
    $dllStage   = "$addinStage\ArchSmarterCharrette"

    New-Item -ItemType Directory -Path $dllStage -Force | Out-Null

    Copy-Item "$buildOutput\*.dll" $dllStage -Force
    Copy-Item "$SolutionRoot\ArchSmarterCharrette\ArchSmarter.Charrette.addin" $addinStage -Force

    # Copy VideoTool into the same folder so the add-in can find it
    Copy-Item "$VideoToolStage\*" $dllStage -Recurse -Force

    Write-Host "  Staged DLLs      -> $dllStage" -ForegroundColor Green
    Write-Host "  Staged VideoTool -> $dllStage" -ForegroundColor Green
    Write-Host "  Staged .addin    -> $addinStage" -ForegroundColor Green
}

Write-Host "`n=== Build staging complete ===" -ForegroundColor Cyan
Write-Host "Output: $StageRoot"
Write-Host ""
Write-Host "Staged folder structure:"
Get-ChildItem $StageRoot -Recurse -Name | ForEach-Object { Write-Host "  $_" }

# ── Run Advanced Installer (optional) ────────────────────────────────────────
if ($BuildMsi) {
    Write-Host "`n=== Building MSI with Advanced Installer ===" -ForegroundColor Cyan

    if (-not (Test-Path $AipFile)) {
        throw "Advanced Installer project not found: $AipFile. Create it first (see README)."
    }
    if (-not (Test-Path $AdvancedInstallerPath)) {
        throw "AdvancedInstaller.com not found at: $AdvancedInstallerPath. Update -AdvancedInstallerPath."
    }

    & $AdvancedInstallerPath /build $AipFile
    if ($LASTEXITCODE -ne 0) { throw "Advanced Installer build failed." }

    Write-Host "`nMSI build complete." -ForegroundColor Green
}
