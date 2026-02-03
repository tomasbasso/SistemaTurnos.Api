<#
 scripts/verify-no-secrets.ps1
 Helper to run gitleaks detect and fail if any secrets found. Useful prior to history rewrite or PR.
#>
param(
    [string]$RepoPath = "$PWD",
    [string]$ReportPath = "$PWD\gitleaks-detect.json"
)

if (-not (Get-Command gitleaks -ErrorAction SilentlyContinue)) {
    Write-Host "gitleaks not found in PATH. Install from https://github.com/zricethezav/gitleaks" -ForegroundColor Red
    exit 1
}

Write-Host "Running gitleaks detect on $RepoPath..." -ForegroundColor Cyan
gitleaks detect --source $RepoPath --report-path $ReportPath --redact

$report = Get-Content $ReportPath -Raw | ConvertFrom-Json -ErrorAction SilentlyContinue
if ($report -and $report | Measure-Object | Select-Object -ExpandProperty Count -ErrorAction SilentlyContinue) {
    Write-Host "Gitleaks found possible secrets. Inspect $ReportPath and address them before rewriting history." -ForegroundColor Red
    exit 2
}

Write-Host "No secrets found by gitleaks (or report is empty)." -ForegroundColor Green
exit 0
