FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first (better layer caching)
COPY global.json .
COPY MissionLog.sln .
COPY src/MissionLog.Core/MissionLog.Core.csproj               src/MissionLog.Core/
COPY src/MissionLog.Infrastructure/MissionLog.Infrastructure.csproj src/MissionLog.Infrastructure/
COPY src/MissionLog.API/MissionLog.API.csproj                 src/MissionLog.API/

RUN dotnet restore src/MissionLog.API/MissionLog.API.csproj

# Copy source and publish
COPY src/MissionLog.Core/         src/MissionLog.Core/
COPY src/MissionLog.Infrastructure/ src/MissionLog.Infrastructure/
COPY src/MissionLog.API/          src/MissionLog.API/

RUN dotnet publish src/MissionLog.API/MissionLog.API.csproj \
    -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway assigns PORT dynamically
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "MissionLog.API.dll"]
