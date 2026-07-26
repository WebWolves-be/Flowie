$ErrorActionPreference = 'Continue'

try {
  $payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
} catch {
  $payload = $null
}
if ($payload -and $payload.stop_hook_active) { exit 0 }

$changed = git status --porcelain | ForEach-Object { $_.Substring(3) }
if (-not $changed) { exit 0 }

$failures = @()

if ($changed | Where-Object { $_ -like 'backend/*' }) {
  $buildOut = dotnet build Flowie.sln --nologo -v q 2>&1 | Out-String
  if ($LASTEXITCODE -ne 0) {
    $failures += "dotnet build failed:`n$buildOut"
  } else {
    $testOut = dotnet test Flowie.sln --no-build --nologo -v q 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { $failures += "dotnet test failed:`n$testOut" }
  }
}

if ($changed | Where-Object { $_ -like 'frontend/*' }) {
  Push-Location frontend/flowie-app
  $ngOut = npx ng build --configuration development 2>&1 | Out-String
  $ngExit = $LASTEXITCODE
  Pop-Location
  if ($ngExit -ne 0) { $failures += "ng build failed:`n$ngOut" }
}

if ($failures.Count -gt 0) {
  [Console]::Error.WriteLine(($failures -join "`n---`n"))
  exit 2
}
exit 0
