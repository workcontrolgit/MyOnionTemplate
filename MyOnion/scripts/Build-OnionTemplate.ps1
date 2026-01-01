param(
    [string]$Configuration = "Release",
    [string]$DesktopDropPath = "$([Environment]::GetFolderPath('Desktop'))\\VSIXTemplateOnionAPI.vsix"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Info([string]$message) {
    Write-Host "[template] $message"
}

function Get-MSBuildPath {
    $default = "msbuild"
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) {
        return $default
    }

    $path = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($path)) {
        return $default
    }

    return $path
}

function Copy-ProjectSource {
    param(
        [string]$Source,
        [string]$Destination
    )

    if (Test-Path $Destination) {
        Remove-Item -Recurse -Force $Destination
    }

    New-Item -ItemType Directory -Path $Destination | Out-Null

    $excludeDirs = @("bin", "obj", ".vs", "TestResults", "logs", "Logs", ".git", "artifacts")
    $excludeList = $excludeDirs | ForEach-Object { "/XD", $_ }
    $arguments = @("`"$Source`"", "`"$Destination`"", "/E", "/NFL", "/NDL", "/NJH", "/NJS") + $excludeList

    $process = Start-Process -FilePath "robocopy" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    if ($process.ExitCode -ge 8) {
        throw "robocopy failed for $Source -> $Destination with exit code $($process.ExitCode)"
    }
}

function Rename-ProjectFile {
    param(
        [string]$ProjectDirectory,
        [string]$TemplateProjectName
    )

    $existing = Get-ChildItem -Path $ProjectDirectory -Filter "*.csproj" | Select-Object -First 1
    if (-not $existing) {
        throw "Could not find project file inside $ProjectDirectory"
    }

    $targetName = "$TemplateProjectName.csproj"
    if ($existing.Name -ne $targetName) {
        Rename-Item -Path $existing.FullName -NewName $targetName
    }
}

function Update-TextTokens {
    param(
        [string]$ProjectDirectory,
        [hashtable]$ReplaceRules
    )

    $textExtensions = @(".cs", ".csproj", ".cshtml", ".json", ".md", ".props", ".targets", ".config", ".xml", ".yml", ".yaml", ".editorconfig", ".http", ".txt", ".razor", ".ps1", ".psm1", ".sql", ".gitignore")
    $binaryExtensions = @(".ico", ".png", ".jpg", ".jpeg", ".gif", ".pfx", ".snk", ".dll", ".pdb", ".exe", ".ttf", ".woff", ".woff2", ".zip", ".7z", ".gz")

    $files = Get-ChildItem -Path $ProjectDirectory -Recurse -File | Where-Object {
        $ext = $_.Extension.ToLower()
        $textExtensions -contains $ext -or ($ext -eq "" -and $_.Length -lt 512000)
    }

    foreach ($file in $files) {
        $content = Get-Content -Raw -Path $file.FullName
        $updated = $content

        foreach ($key in $ReplaceRules.Keys) {
            $pattern = [Regex]::Escape($key)
            $updated = [Regex]::Replace($updated, $pattern, $ReplaceRules[$key])
        }

        if ($updated -ne $content) {
            Set-Content -Path $file.FullName -Value $updated -Encoding UTF8
        }
    }
}

function New-TemplateXml {
    param(
        [string]$ProjectDirectory,
        [string]$TemplateProjectName,
        [string]$Description
    )

    $ns = "http://schemas.microsoft.com/developer/vstemplate/2005"
    $doc = New-Object System.Xml.XmlDocument
    $doc.AppendChild($doc.CreateXmlDeclaration("1.0", "utf-8", $null)) | Out-Null

    $root = $doc.CreateElement("VSTemplate", $ns)
    $root.SetAttribute("Version", "3.0.0")
    $root.SetAttribute("Type", "Project")
    $doc.AppendChild($root) | Out-Null

    $templateData = $doc.CreateElement("TemplateData", $ns)
    $root.AppendChild($templateData) | Out-Null

    foreach ($pair in @(@("Name", $TemplateProjectName), @("Description", $Description), @("ProjectType", "CSharp"), @("SortOrder", "1000"), @("CreateNewFolder", "true"), @("DefaultName", $TemplateProjectName), @("ProvideDefaultName", "true"), @("LocationField", "Enabled"), @("EnableLocationBrowseButton", "true"), @("CreateInPlace", "true"))) {
        $element = $doc.CreateElement($pair[0], $ns)
        $element.InnerText = $pair[1]
        $templateData.AppendChild($element) | Out-Null
    }

    $templateContent = $doc.CreateElement("TemplateContent", $ns)
    $root.AppendChild($templateContent) | Out-Null

    $projectElement = $doc.CreateElement("Project", $ns)
    $projectElement.SetAttribute("TargetFileName", "$TemplateProjectName.csproj")
    $projectElement.SetAttribute("File", "$TemplateProjectName.csproj")
    $projectElement.SetAttribute("ReplaceParameters", "true")
    $templateContent.AppendChild($projectElement) | Out-Null

    Add-DirectoryNodes -XmlDoc $doc -ParentNode $projectElement -Directory $ProjectDirectory

    $outputPath = Join-Path $ProjectDirectory "MyTemplate.vstemplate"
    $doc.Save($outputPath)
}

function Add-DirectoryNodes {
    param(
        [System.Xml.XmlDocument]$XmlDoc,
        [System.Xml.XmlElement]$ParentNode,
        [string]$Directory
    )

    $ns = "http://schemas.microsoft.com/developer/vstemplate/2005"
    $skipDirs = @("bin", "obj", ".vs", "TestResults", "logs", "Logs", ".git", "artifacts")
    $skipFiles = @("MyTemplate.vstemplate")
    $binaryExtensions = @(".ico", ".png", ".jpg", ".jpeg", ".gif", ".pfx", ".snk", ".dll", ".pdb", ".exe", ".ttf", ".woff", ".woff2", ".zip", ".7z", ".gz")

    $subDirectories = Get-ChildItem -Path $Directory -Directory | Where-Object { $skipDirs -notcontains $_.Name } | Sort-Object Name
    foreach ($dir in $subDirectories) {
        $folder = $XmlDoc.CreateElement("Folder", $ns)
        $folder.SetAttribute("Name", $dir.Name)
        $folder.SetAttribute("TargetFolderName", $dir.Name)
        $ParentNode.AppendChild($folder) | Out-Null
        Add-DirectoryNodes -XmlDoc $XmlDoc -ParentNode $folder -Directory $dir.FullName
    }

    $files = Get-ChildItem -Path $Directory -File | Where-Object { $skipFiles -notcontains $_.Name -and $_.Extension -ne ".csproj" } | Sort-Object Name
    foreach ($file in $files) {
        $item = $XmlDoc.CreateElement("ProjectItem", $ns)
        $ext = $file.Extension.ToLower()
        $replace = if ($binaryExtensions -contains $ext) { "false" } else { "true" }
        $item.SetAttribute("ReplaceParameters", $replace)
        $item.SetAttribute("TargetFileName", $file.Name)
        $item.InnerText = $file.Name
        $ParentNode.AppendChild($item) | Out-Null
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$templateRoot = Join-Path $repoRoot "artifacts\\TemplateOnionAPI"
$vsixProjectDir = Join-Path $repoRoot "vsix\\VSIXTemplateOnionAPI"
$projectTemplatesDir = Join-Path $vsixProjectDir "ProjectTemplates"
$zipPath = Join-Path $projectTemplatesDir "TemplateOnionAPI.zip"

New-Item -ItemType Directory -Path (Split-Path $templateRoot) -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Path $projectTemplatesDir -ErrorAction SilentlyContinue | Out-Null

if (Test-Path $templateRoot) {
    Remove-Item -Recurse -Force $templateRoot
}
New-Item -ItemType Directory -Path $templateRoot | Out-Null

$projects = @(
    @{
        Name = "Apiresources.Application"
        Source = Join-Path $repoRoot "src\\MyOnion.Application"
        Description = "Application layer: CQRS, DTOs, validation, and MediatR behaviours."
        Replace = @{
            "MyOnion.Application" = "`$safeprojectname$"
            "MyOnion.Domain" = "`$ext_projectname$.Domain"
        }
    },
    @{
        Name = "Apiresources.Domain"
        Source = Join-Path $repoRoot "src\\MyOnion.Domain"
        Description = "Domain entities, enums, and value objects."
        Replace = @{
            "MyOnion.Domain" = "`$safeprojectname$.Domain"
        }
    },
    @{
        Name = "Apiresources.Infrastructure.Persistence"
        Source = Join-Path $repoRoot "src\\MyOnion.Infrastructure.Persistence"
        Description = "EF Core DbContext, repositories, and migrations."
        Replace = @{
            "MyOnion.Infrastructure.Persistence" = "`$safeprojectname$"
            "MyOnion.Domain" = "`$ext_projectname$.Domain"
            "MyOnion.Application" = "`$ext_projectname$.Application"
        }
    },
    @{
        Name = "Apiresources.Infrastructure.Shared"
        Source = Join-Path $repoRoot "src\\MyOnion.Infrastructure.Shared"
        Description = "Shared services (email, datetime, seeders)."
        Replace = @{
            "MyOnion.Infrastructure.Shared" = "`$safeprojectname$"
            "MyOnion.Application" = "`$ext_projectname$.Application"
            "MyOnion.Domain" = "`$ext_projectname$.Domain"
        }
    },
    @{
        Name = "Apiresources.WebApi"
        Source = Join-Path $repoRoot "src\\MyOnion.WebApi"
        Description = "ASP.NET Core Web API host, middleware, and configuration."
        Replace = @{
            "MyOnion.WebApi" = "`$safeprojectname$"
            "MyOnion.Application" = "`$ext_projectname$.Application"
            "MyOnion.Domain" = "`$ext_projectname$.Domain"
            "MyOnion.Infrastructure.Persistence" = "`$ext_projectname$.Infrastructure.Persistence"
            "MyOnion.Infrastructure.Shared" = "`$ext_projectname$.Infrastructure.Shared"
        }
    }
)

foreach ($project in $projects) {
    $destination = Join-Path $templateRoot $project.Name
    Write-Info "Preparing $($project.Name)"
    Copy-ProjectSource -Source $project.Source -Destination $destination
    Rename-ProjectFile -ProjectDirectory $destination -TemplateProjectName $project.Name
    Update-TextTokens -ProjectDirectory $destination -ReplaceRules $project.Replace
    New-TemplateXml -ProjectDirectory $destination -TemplateProjectName $project.Name -Description $project.Description
}

$rootTemplate = @'
<VSTemplate Version="3.0.0" Type="ProjectGroup" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">
  <TemplateData>
    <Name>Onion API (.NET 10)</Name>
    <Description>Creates a 5-project onion architecture API solution targeting .NET 10 with CQRS, MediatR, FluentValidation, EF Core, and Swagger.</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>1000</SortOrder>
    <CreateNewFolder>true</CreateNewFolder>
    <DefaultName>MyOnionApi</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
    <LocationField>Enabled</LocationField>
    <EnableLocationBrowseButton>true</EnableLocationBrowseButton>
  </TemplateData>
  <TemplateContent>
    <ProjectCollection>
      <SolutionFolder Name="Presentation">
        <ProjectTemplateLink ProjectName="$projectname$.WebApi" CopyParameters="true">
          Apiresources.WebApi\MyTemplate.vstemplate
        </ProjectTemplateLink>
      </SolutionFolder>
      <SolutionFolder Name="Core">
        <ProjectTemplateLink ProjectName="$projectname$.Application" CopyParameters="true">
          Apiresources.Application\MyTemplate.vstemplate
        </ProjectTemplateLink>
        <ProjectTemplateLink ProjectName="$projectname$.Domain" CopyParameters="true">
          Apiresources.Domain\MyTemplate.vstemplate
        </ProjectTemplateLink>
      </SolutionFolder>
      <SolutionFolder Name="Infrastructure">
        <ProjectTemplateLink ProjectName="$projectname$.Infrastructure.Persistence" CopyParameters="true">
          Apiresources.Infrastructure.Persistence\MyTemplate.vstemplate
        </ProjectTemplateLink>
        <ProjectTemplateLink ProjectName="$projectname$.Infrastructure.Shared" CopyParameters="true">
          Apiresources.Infrastructure.Shared\MyTemplate.vstemplate
        </ProjectTemplateLink>
      </SolutionFolder>
    </ProjectCollection>
  </TemplateContent>
</VSTemplate>
'@

$rootTemplate | Set-Content -Path (Join-Path $templateRoot "OnionAPI.vstemplate") -Encoding UTF8

if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}
Compress-Archive -Path (Join-Path $templateRoot "*") -DestinationPath $zipPath -Force
Write-Info "Template zip created at $zipPath"

$msbuildExe = Get-MSBuildPath
Push-Location $vsixProjectDir
try {
    & $msbuildExe ".\VSIXTemplateOnionAPI.csproj" /t:Restore,"Rebuild" /p:Configuration=$Configuration | Write-Host
} finally {
    Pop-Location
}

$builtVsix = Join-Path $vsixProjectDir "bin\\$Configuration\\VSIXTemplateOnionAPI.vsix"
if (-not (Test-Path $builtVsix)) {
    throw "VSIX build did not produce $builtVsix"
}

Copy-Item -Path $builtVsix -Destination $DesktopDropPath -Force
Write-Info "VSIX copied to $DesktopDropPath"
