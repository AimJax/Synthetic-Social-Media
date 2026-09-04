# Android Build Script
# Usage: .\build-android.ps1 [-BuildType Debug]

param(
    [string]$BuildType = "Debug"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = "D:\SyntheticSocialWorld"
$AndroidPath = "$ProjectRoot\src\Android\SyntheticSocialWorld"

Write-Host "=== Synthetic Social World Android Build ===" -ForegroundColor Cyan

if (-not (Test-Path $AndroidPath)) {
    Write-Host "Android project not found at: $AndroidPath" -ForegroundColor Red
    Write-Host "Run 'init-android.ps1' first to create the project." -ForegroundColor Yellow
    exit 1
}

Set-Location $AndroidPath

# Check for Gradle
$gradle = Get-Command gradle -ErrorAction SilentlyContinue
if (-not $gradle) {
    Write-Host "Gradle not found. Using gradlew..." -ForegroundColor Yellow
    $gradlew = "$AndroidPath\gradlew.bat"
    if (Test-Path $gradlew) {
        & $gradlew assemble$BuildType --no-daemon
    } else {
        Write-Host "gradlew.bat not found. Please create the Android project first." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Using system Gradle..." -ForegroundColor Gray
    gradle assemble$BuildType --no-daemon
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    $apkPath = "$AndroidPath\app\build\outputs\apk\$BuildType"
    if (Test-Path $apkPath) {
        Write-Host "APK location: $apkPath" -ForegroundColor Gray
    }
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
