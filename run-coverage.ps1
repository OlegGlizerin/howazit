# Run Tests with Coverage and Generate HTML Report
# This script runs tests with coverage, generates an HTML report, and opens it in the browser

Write-Host "Running tests with coverage..." -ForegroundColor Green

# Run tests with coverage
dotnet test HowazitSurveyService.Tests --collect:"XPlat Code Coverage" --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. Exiting." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nTests completed successfully!" -ForegroundColor Green

# Find the latest coverage file
$coverageFiles = Get-ChildItem -Path "HowazitSurveyService.Tests\TestResults" -Filter "coverage.cobertura.xml" -Recurse | Sort-Object LastWriteTime -Descending

if ($coverageFiles.Count -eq 0) {
    Write-Host "No coverage file found. Exiting." -ForegroundColor Red
    exit 1
}

$latestCoverageFile = $coverageFiles[0].FullName
Write-Host "Found coverage file: $latestCoverageFile" -ForegroundColor Cyan

# Check if ReportGenerator is installed
$reportGeneratorInstalled = Get-Command reportgenerator -ErrorAction SilentlyContinue

if (-not $reportGeneratorInstalled) {
    Write-Host "`nReportGenerator not found. Installing..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install ReportGenerator. Exiting." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "ReportGenerator installed successfully!" -ForegroundColor Green
    # Refresh PATH to use the newly installed tool
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
}

# Create output directory
$htmlOutputDir = "HowazitSurveyService.Tests\coverage\html"
if (Test-Path $htmlOutputDir) {
    Remove-Item -Path $htmlOutputDir -Recurse -Force
}
New-Item -Path $htmlOutputDir -ItemType Directory -Force | Out-Null

Write-Host "`nGenerating HTML report..." -ForegroundColor Green

# Generate HTML report
reportgenerator `
    -reports:"$latestCoverageFile" `
    -targetdir:"$htmlOutputDir" `
    -reporttypes:Html `
    -verbosity:Warning

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to generate HTML report. Exiting." -ForegroundColor Red
    exit 1
}

$htmlReportPath = Join-Path $htmlOutputDir "index.html"

Write-Host "`nHTML report generated successfully!" -ForegroundColor Green
Write-Host "Report location: $htmlReportPath" -ForegroundColor Cyan

# Open the report in the default browser
Write-Host "`nOpening report in browser..." -ForegroundColor Green
Start-Process $htmlReportPath

Write-Host "`nDone! Coverage report opened in browser." -ForegroundColor Green

