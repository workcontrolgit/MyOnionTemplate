param(
    [string]$Configuration = "Release",
    [string[]]$DestinationTemplatesPaths = @(),
    [string]$TemplateZipName = "TemplateOnionAPI.zip"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Info([string]$message) {
    Write-Host "[template-local] $message"
}

$buildScript = Join-Path $PSScriptRoot "Build-OnionTemplate.ps1"
if (-not (Test-Path $buildScript)) {
    throw "Unable to locate Build-OnionTemplate.ps1 at $buildScript"
}

Write-Info "Generating latest template package (SkipVsix=true)..."
& $buildScript -Configuration $Configuration -SkipVsix

$repoRoot = Split-Path -Parent $PSScriptRoot
$templateZipSource = Join-Path $repoRoot "vsix\VSIXTemplateOnionAPI\ProjectTemplates\CSharp\1033\TemplateOnionAPI.zip"
if (-not (Test-Path $templateZipSource)) {
    throw "Template zip was not created at $templateZipSource"
}

$documents = [Environment]::GetFolderPath("MyDocuments")
$vsTemplateRoots = @(
    # Visual Studio 2022 default templates path
    "Visual Studio 2022\Templates\ProjectTemplates\Visual C#",
    # Visual Studio 2019 default templates path
    "Visual Studio 2019\Templates\ProjectTemplates\Visual C#",
    # Visual Studio 2026 (internal “Visual Studio 18”) templates path
    "Visual Studio 18\Templates\ProjectTemplates\Visual C#",
    # Generic fallback for future Visual Studio versions
    "Visual Studio\Templates\ProjectTemplates\Visual C#"
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

$autoPaths = foreach ($root in $vsTemplateRoots) {
    Join-Path $documents $root
}

$allDestinations = New-Object System.Collections.Generic.List[string]
$allDestinations.AddRange($DestinationTemplatesPaths)
foreach ($auto in $autoPaths) {
    if (-not $allDestinations.Contains($auto)) {
        $allDestinations.Add($auto)
    }
}

foreach ($destination in $allDestinations | Select-Object -Unique) {
    if (-not (Test-Path $destination)) {
        Write-Info "Creating destination directory $destination"
        New-Item -ItemType Directory -Path $destination -Force | Out-Null
    }

    $destinationZip = Join-Path $destination $TemplateZipName
    Copy-Item -Path $templateZipSource -Destination $destinationZip -Force
    Write-Info "Template copied to $destinationZip"
}

Write-Info "Restart Visual Studio or refresh its template cache if the template does not appear immediately."
