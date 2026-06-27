# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ProjectForge.Web/ProjectForge.Web.csproj                       src/ProjectForge.Web/
COPY src/ProjectForge.Application/ProjectForge.Application.csproj       src/ProjectForge.Application/
COPY src/ProjectForge.Core/ProjectForge.Core.csproj                     src/ProjectForge.Core/
COPY src/ProjectForge.Infrastructure/ProjectForge.Infrastructure.csproj src/ProjectForge.Infrastructure/

RUN dotnet restore src/ProjectForge.Web/ProjectForge.Web.csproj -m:1

# Cache bust: changing this value forces Docker to re-copy source files
ARG BUILD_DATE=20260627_v5
RUN echo "Build: ${BUILD_DATE}"
COPY src/ src/

RUN dotnet publish src/ProjectForge.Web/ProjectForge.Web.csproj \
    -c Release -o /app/publish /p:UseAppHost=false -m:1

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final
WORKDIR /app

# ── System tools + Node.js LTS ─────────────────────────────────────────────
RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl unzip zip openssh-client ca-certificates gnupg \
    # PHP + Composer
    php-cli php-xml php-mbstring php-zip php-curl php-sqlite3 composer \
    # Python 3 + venv
    python3 python3-pip python3-venv python3-dev \
    # Java 21 (Temurin via apt)
    wget \
    && rm -rf /var/lib/apt/lists/*

# ── Node.js 22 (LTS) via NodeSource ────────────────────────────────────────
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

# ── Java 21 + Maven via SDKMAN ─────────────────────────────────────────────
RUN curl -s "https://get.sdkman.io" | bash \
    && bash -c "source /root/.sdkman/bin/sdkman-init.sh \
        && sdk install java 21.0.5-tem \
        && sdk install maven 3.9.9 \
        && sdk flush archives"

ENV JAVA_HOME=/root/.sdkman/candidates/java/current
ENV MAVEN_HOME=/root/.sdkman/candidates/maven/current
ENV PATH="${JAVA_HOME}/bin:${MAVEN_HOME}/bin:${PATH}"

# ── Python frameworks global install ───────────────────────────────────────
RUN pip3 install --no-cache-dir --break-system-packages \
    django \
    flask \
    fastapi \
    uvicorn \
    click \
    && pip3 install --no-cache-dir --break-system-packages \
    "django==5.1.*" \
    "flask==3.1.*" \
    "fastapi==0.115.*" \
    "uvicorn[standard]"

# ── NestJS CLI global ───────────────────────────────────────────────────────
RUN npm install -g @nestjs/cli create-next-app

# ── .NET new templates ──────────────────────────────────────────────────────
RUN dotnet new install Microsoft.AspNetCore.SpaTemplates 2>/dev/null || true

# ── Cache bust (increment when forcing full rebuild) ─────────────────────────
ARG CACHE_BUST=20260627_v3

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
