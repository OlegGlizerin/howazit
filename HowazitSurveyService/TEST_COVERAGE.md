# Test Coverage Commands

## Quick Script (Recommended) ⚡

**Run the script to automatically run tests, generate HTML report, and open it:**

```powershell
.\run-coverage.ps1
```

This script will:
1. ✅ Run all tests with coverage
2. ✅ Find the latest coverage file
3. ✅ Install ReportGenerator if needed
4. ✅ Generate HTML report
5. ✅ Open the report in your browser

## Manual Commands

### Run Coverage

```powershell
dotnet test HowazitSurveyService.Tests --collect:"XPlat Code Coverage"
```

**Coverage report location:**
`HowazitSurveyService.Tests/TestResults/[GUID]/coverage.cobertura.xml`

## View Coverage Report

### Option 1: Use the Script (Easiest)

```powershell
.\run-coverage.ps1
```

### Option 2: Generate HTML Report Manually

1. **Install ReportGenerator (one-time):**
   ```powershell
   dotnet tool install -g dotnet-reportgenerator-globaltool
   ```

2. **Generate HTML report:**
   ```powershell
   reportgenerator `
     -reports:"HowazitSurveyService.Tests/TestResults/*/coverage.cobertura.xml" `
     -targetdir:"HowazitSurveyService.Tests/coverage/html" `
     -reporttypes:Html
   ```

3. **Open the report:**
   ```powershell
   Start-Process "HowazitSurveyService.Tests/coverage/html/index.html"
   ```

### Option 3: View in Visual Studio

1. Open Visual Studio
2. Go to **Test Explorer** → **Code Coverage Results**
3. Click **Analyze Code Coverage** → **Selected Tests**
4. Or open the `.cobertura.xml` file directly in Visual Studio

## Current Coverage

From the generated report:
- **Line Coverage**: 36.67% (249/679 lines)
- **Branch Coverage**: 52.17% (48/92 branches)

## Improve Coverage

To improve coverage:
1. Add more unit tests for untested code paths
2. Test error scenarios and edge cases
3. Test background services and worker processes
4. Test controllers and API endpoints

## View Coverage in CI/CD

The `.cobertura.xml` file can be used in CI/CD pipelines:
- Azure DevOps: Code Coverage tab
- GitHub Actions: Upload coverage reports
- Jenkins: Coverage plugins
