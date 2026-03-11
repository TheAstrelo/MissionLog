#!/bin/bash
set -e

echo "============================================"
echo " MissionLog — First Time Setup"
echo "============================================"
echo ""

# Check .NET 8
if ! command -v dotnet &> /dev/null; then
    echo "[ERROR] .NET 8 SDK not found."
    echo "Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

echo "[1/3] Restoring NuGet packages..."
dotnet restore MissionLog.sln

echo ""
echo "[2/3] Building solution..."
dotnet build MissionLog.sln --no-restore -c Release

echo ""
echo "[3/3] Done!"
echo ""
echo "  Start the API (auto-migrates + seeds DB on first run):"
echo "    cd src/MissionLog.API && dotnet run"
echo ""
echo "  In a second terminal, start Blazor:"
echo "    cd src/MissionLog.BlazorApp && dotnet run"
echo ""
echo "  API:     https://localhost:7100"
echo "  Swagger: https://localhost:7100/swagger"
echo "  Blazor:  https://localhost:7200"
echo ""
echo "  Demo login: admin / Admin123!"
