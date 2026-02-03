# PowerShell helper to run gitleaks locally and show summary
param(
    [string]$ReportPath = "gitleaks-local-report.json"
)

if (-not (Get-Command gitleaks -ErrorAction SilentlyContinue)) {
    Write-Error "gitleaks not found in PATH. Install from https://github.com/zricethezav/gitleaks"
    exit 1
}

Write-Host "Running gitleaks..."
& gitleaks detect --source . --report-path $ReportPath --redact

if (Test-Path $ReportPath) {
    $json = Get-Content $ReportPath -Raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($null -eq $json) {
        Write-Host "No valid JSON report found at $ReportPath"
        exit 0
    }

    if ($json | Get-Member -Name 'Findings' -ErrorAction SilentlyContinue) {
        $count = ($json.Findings | Measure-Object).Count
        Write-Host "Gitleaks findings: $count"
        if ($count -gt 0) {
            Write-Host "Open $ReportPath to inspect findings."
            exit 2
        }
    }
    else {
        Write-Host "No findings property in report; check file: $ReportPath"
    }
}
else {
    Write-Host "Report not created."
}
