# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo los .csproj primero para cachear el restore
COPY src/ProjectForge.Web/ProjectForge.Web.csproj                       src/ProjectForge.Web/
COPY src/ProjectForge.Application/ProjectForge.Application.csproj       src/ProjectForge.Application/
COPY src/ProjectForge.Core/ProjectForge.Core.csproj                     src/ProjectForge.Core/
COPY src/ProjectForge.Infrastructure/ProjectForge.Infrastructure.csproj src/ProjectForge.Infrastructure/

# Restore limpio — sin caché de Windows contaminando
RUN dotnet restore src/ProjectForge.Web/ProjectForge.Web.csproj

# Copiar el resto del código fuente
COPY src/ src/

# Publish
RUN dotnet publish src/ProjectForge.Web/ProjectForge.Web.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl nodejs npm python3 python3-pip python3-venv openssh-client unzip \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /root \
    && git config --global user.name "ProjectForge Bot" \
    && git config --global user.email "bot@projectforge.dev" \
    && git config --global init.defaultBranch main \
    && git config --global credential.helper store

COPY --from=build /app/publish .
RUN mkdir -p /workspaces && chmod 777 /workspaces

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectForge.Web.dll"]
