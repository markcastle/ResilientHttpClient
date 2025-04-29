@echo off
REM Script to run code coverage for ResilientHttpClient

REM Step 1: Ensure Coverlet is installed in the test project
echo Installing Coverlet collector (if needed)...
dotnet add ResilientHttpClient.Tests/ResilientHttpClient.Tests.csproj package coverlet.collector

REM Step 2: Run tests and collect coverage
echo Running tests and collecting coverage...
dotnet test --collect:"XPlat Code Coverage"

REM Step 3: Ensure ReportGenerator is installed globally
echo Installing ReportGenerator (if needed)...
dotnet tool install -g dotnet-reportgenerator-globaltool

REM Step 4: Find the coverage report file
for /R %%f in (*coverage.cobertura.xml) do set COVERAGE=%%f

REM Step 5: Generate HTML report
if not defined COVERAGE (
    echo Coverage file not found!
    exit /b 1
)
echo Generating HTML coverage report...
reportgenerator -reports:%COVERAGE% -targetdir:coveragereport

REM Step 6: Open the report in the default browser (optional)
if exist coveragereport\index.html (
    start coveragereport\index.html
    echo Coverage report opened in your browser.
) else (
    echo Coverage report not found!
)

echo Done.
pause
