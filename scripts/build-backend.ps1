# Build Script for Synthetic Social World Backend
# Usage: .\build-backend.ps1 [-Configuration Release]

param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = "D:\SyntheticSocialWorld"
$BackendPath = "$ProjectRoot\src\Backend"

Write-Host "=== Synthetic Social World Backend Build ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray

# Navigate to backend directory
Set-Location $BackendPath

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Yellow
dotnet clean --configuration $Configuration --verbosity quiet

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Package restore failed!" -ForegroundColor Red
    exit 1
}

# Build
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green

# Show output
$OutputPath = "$BackendPath\SyntheticSocialWorld.Api\bin\$Configuration\net10.0"
if (Test-Path $OutputPath) {
    Write-Host "`nOutput location: $OutputPath" -ForegroundColor Gray
    Get-ChildItem $OutputPath -Filter "*.dll" | Select-Object Name, Length | Format-Table
}
