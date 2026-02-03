<#
scripts/cleanup-history.ps1

Safe interactive helper to rewrite git history removing secrets using git-filter-repo or BFG.
USAGE: .\cleanup-history.ps1 -RepoUrl 'git@github.com:org/repo.git' -SecretsFile .\secrets-to-remove.txt -DryRun

WARNING: This is a destructive operation that rewrites history. Rotate credentials BEFORE running.
#>
param(
    [string]$RepoUrl = "",
    [string]$MirrorDir = "$PWD\repo-mirror",
    [string]$SecretsFile = "$PWD\secrets-to-remove.txt",
    [switch]$UseBFG,
    [switch]$DryRun
)

function Require-Confirm {
    param([string]$Message)
    Write-Host "${Message}`nType 'YES' to continue:`n" -ForegroundColor Yellow
    $answer = Read-Host
    if ($answer -ne 'YES') {
        Write-Host "Aborting." -ForegroundColor Red
        exit 1
    }
}

# Basic checks
if (-not (Test-Path $SecretsFile)) {
    Write-Host "Secrets file '$SecretsFile' not found. Create it with each secret or regex on its own line." -ForegroundColor Red
    exit 1
}

if ($RepoUrl -eq "") {
    Write-Host "RepoUrl not provided. Use -RepoUrl 'git@github.com:org/repo.git'" -ForegroundColor Red
    exit 1
}

Write-Host "This script will rewrite the history of '$RepoUrl' to remove secrets listed in '$SecretsFile'." -ForegroundColor Cyan
Write-Host "Make sure you've ROTATED and REVOKED the secrets before proceeding.
" -ForegroundColor Yellow

if ($DryRun) {
    Write-Host "Running in DryRun mode: no push will be made, and commands will be printed instead of executed." -ForegroundColor Green
}

Require-Confirm "Proceed with the mirror clone and history rewrite? (This is destructive)"

# Clone a mirror
if (Test-Path $MirrorDir) {
    Write-Host "Mirror dir exists. Removing it first." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $MirrorDir
}

Write-Host "Cloning mirror..." -ForegroundColor Cyan
if ($DryRun) { Write-Host "git clone --mirror $RepoUrl $MirrorDir" } else { git clone --mirror $RepoUrl $MirrorDir }

Push-Location $MirrorDir

# Prepare for replace-text format if using git-filter-repo
$replaceFile = "replace-text.txt"
Write-Host "Generating replace-text file from $SecretsFile" -ForegroundColor Cyan
Get-Content $SecretsFile | ForEach-Object { if (-not [string]::IsNullOrWhiteSpace($_)) { "$_==[REDACTED]" } } | Set-Content $replaceFile

if ($UseBFG) {
    Write-Host "Using BFG to replace secrets. Ensure bfg.jar is available and JAVA is installed." -ForegroundColor Cyan
    if ($DryRun) {
        Write-Host "java -jar bfg.jar --replace-text $replaceFile" -ForegroundColor Green
    } else {
        java -jar bfg.jar --replace-text $replaceFile
        git reflog expire --expire=now --all
        git gc --prune=now --aggressive
    }
} else {
    Write-Host "Using git-filter-repo to replace secrets." -ForegroundColor Cyan
    # Note: git-filter-repo requires Python and to be installed on PATH.
    if ($DryRun) {
        Write-Host "git filter-repo --replace-text $replaceFile" -ForegroundColor Green
    } else {
        git filter-repo --replace-text $replaceFile
    }
}

Write-Host "History rewrite complete locally. Next steps: verify the repository, run gitleaks detect on it, then push --force if everything is clean." -ForegroundColor Yellow

if (-not $DryRun) {
    Write-Host "Do you want to push the cleaned repo to origin with --force?" -ForegroundColor Yellow
    Require-Confirm "This will force-push and affect all collaborators. Type YES to push."
    Write-Host "Pushing cleaned repo..." -ForegroundColor Cyan
    git push --force
}

Pop-Location

Write-Host "Done. REMEMBER: All collaborators must reclone or reset local branches. See docs/ROTATE_AND_CLEANUP.md for checklist." -ForegroundColor Green
