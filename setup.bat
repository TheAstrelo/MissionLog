@echo off
echo ============================================
echo  MissionLog — First Time Setup
echo ============================================
echo.

REM Check .NET 8
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET 8 SDK not found.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [1/3] Restoring NuGet packages...
dotnet restore MissionLog.sln
if errorlevel 1 (echo [ERROR] Restore failed. && pause && exit /b 1)

echo.
echo [2/3] Building solution...
dotnet build MissionLog.sln --no-restore -c Release
if errorlevel 1 (echo [ERROR] Build failed. && pause && exit /b 1)

echo.
echo [3/3] Done! Starting API (DB will migrate + seed automatically on first run)...
echo.
echo  API will be available at: https://localhost:7100
echo  Swagger UI:               https://localhost:7100/swagger
echo.
echo  Then in a second terminal run:
echo    cd src\MissionLog.BlazorApp ^&^& dotnet run
echo  Blazor:                   https://localhost:7200
echo.
echo  Demo login: admin / Admin123!
echo.

cd src\MissionLog.API && dotnet run
