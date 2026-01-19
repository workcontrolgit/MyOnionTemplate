param(
  [Parameter(Mandatory = $true)]
  [string]$Password,
  [string]$CertName = "myonion-dev",
  [string]$EnvPath = ".env",
  [switch]$Force
)

$certDir = Join-Path $env:USERPROFILE ".aspnet\\https"
New-Item -ItemType Directory -Force -Path $certDir | Out-Null
$pfxPath = Join-Path $certDir ($CertName + ".pfx")

if ($Force -and (Test-Path $pfxPath)) {
  Remove-Item -Force $pfxPath
}

dotnet dev-certs https -ep $pfxPath -p $Password | Out-Null

$envLine = "MYONION_HTTPS_CERT_PASSWORD=$Password"
if (Test-Path $EnvPath) {
  $content = Get-Content -Raw $EnvPath
  if ($content -match "(?m)^MYONION_HTTPS_CERT_PASSWORD=") {
    $content = [regex]::Replace($content, "(?m)^MYONION_HTTPS_CERT_PASSWORD=.*$", $envLine)
  } else {
    if ($content.Length -gt 0 -and -not $content.EndsWith("`n")) {
      $content += "`n"
    }
    $content += $envLine + "`n"
  }
  Set-Content -NoNewline $EnvPath $content
} else {
  Set-Content -NoNewline $EnvPath ($envLine + "`n")
}

Write-Host "Wrote cert: $pfxPath"
Write-Host "Updated $EnvPath with MYONION_HTTPS_CERT_PASSWORD"
