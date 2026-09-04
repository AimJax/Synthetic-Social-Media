# Run Server Script for Synthetic Social World
# Usage: .\run-server.ps1

$ErrorActionPreference = "Stop"
$ProjectRoot = "D:\SyntheticSocialWorld"
$ServerPath = "$ProjectRoot\src\Backend\SyntheticSocialWorld.Api"

Write-Host "=== Starting Synthetic Social World Server ===" -ForegroundColor Cyan
Write-Host "Server will be available at: http://localhost:5000" -ForegroundColor Gray
Write-Host "Swagger UI at: http://localhost:5000/swagger" -ForegroundColor Gray
Write-Host "Press Ctrl+C to stop`n" -ForegroundColor Gray

Set-Location $ServerPath
dotnet run --configuration Debug
