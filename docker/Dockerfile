# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ProjectForge.Web/ProjectForge.Web.csproj                       src/ProjectForge.Web/
COPY src/ProjectForge.Application/ProjectForge.Application.csproj       src/ProjectForge.Application/
COPY src/ProjectForge.Core/ProjectForge.Core.csproj                     src/ProjectForge.Core/
COPY src/ProjectForge.Infrastructure/ProjectForge.Infrastructure.csproj src/ProjectForge.Infrastructure/

RUN dotnet restore src/ProjectForge.Web/ProjectForge.Web.csproj -m:1

# Cache bust: changing this value forces Docker to re-copy source files
ARG BUILD_DATE=20260703_v1
RUN echo "Build: ${BUILD_DATE}"
COPY src/ src/

RUN dotnet publish src/ProjectForge.Web/ProjectForge.Web.csproj \
    -c Release -o /app/publish /p:UseAppHost=false -m:1

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
# No more Java/Maven/Python/Node/PHP toolchains installed here — those scaffolding commands now
# run in their own per-language worker containers (docker/Dockerfile.Worker.*) reached over HTTP
# via RoutedShellExecutor (see src/ProjectForge.Infrastructure/Infrastructure.cs). This image
# only needs git (for the GitHub push step) and the ASP.NET runtime, not the full SDK — "dotnet
# new"/"dotnet add package" etc. now happen in dotnet-worker instead of locally.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    git ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# ── Git config ──────────────────────────────────────────────────────────────
RUN mkdir -p /root \
    && git config --global user.name "ProjectForge Bot" \
    && git config --global user.email "bot@projectforge.dev" \
    && git config --global init.defaultBranch main \
    && git config --global credential.helper store

COPY --from=build /app/publish .
RUN mkdir -p /workspaces && chmod 777 /workspaces

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectForge.Web.dll"]
