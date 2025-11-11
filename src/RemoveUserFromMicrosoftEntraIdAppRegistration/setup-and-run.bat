@echo off
echo ========================================
echo Azure Portal Automation Setup
echo ========================================
echo.

echo Step 1: Building the project...
dotnet build AzurePortalAutomation.csproj
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b %errorlevel%
)
echo.

echo Step 2: Installing Playwright browsers...
echo This may take a few minutes...
pwsh bin\Debug\net8.0\playwright.ps1 install chromium
if %errorlevel% neq 0 (
    echo.
    echo Note: If PowerShell is not available, please run:
    echo   dotnet tool install --global Microsoft.Playwright.CLI
    echo   playwright install chromium
    echo.
    pause
    exit /b %errorlevel%
)
echo.

echo ========================================
echo Setup complete! Running the program...
echo ========================================
echo.
dotnet run --project AzurePortalAutomation.csproj