# ─── Stage 1: Build the worker app ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ProjectForge.Worker/ProjectForge.Worker.csproj src/ProjectForge.Worker/
RUN dotnet restore src/ProjectForge.Worker/ProjectForge.Worker.csproj

COPY src/ProjectForge.Worker/ src/ProjectForge.Worker/
RUN dotnet publish src/ProjectForge.Worker/ProjectForge.Worker.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# ── PHP + Composer — same packages the monolithic image used ──
# (curl is only here for the docker-compose healthcheck)
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    php-cli php-xml php-mbstring php-zip php-curl php-sqlite3 composer unzip \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectForge.Worker.dll"]
