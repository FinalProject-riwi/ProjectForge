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
# php-mongodb: doctrine/mongodb-odm-bundle declares a hard "ext-mongodb" composer platform
# requirement — without it, "composer require" fails during generation itself (before even
# reaching the generated project's own Dockerfile, which already installs this via pecl for its
# own runtime image).
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    php-cli php-xml php-mbstring php-zip php-curl php-sqlite3 php-mongodb composer unzip \
    php-pear php8.3-dev build-essential libssl-dev \
    && rm -rf /var/lib/apt/lists/*

# ext-redis: Ubuntu's apt "php-redis" package is stuck on phpredis 5.3.7, but
# symfony/redis-messenger 7.4.x requires phpredis >= 6.1 ("ext-redis is present at version 5.3.7
# and cannot be modified by Composer ... conflict with ext-redis <6.1") — every Symfony + Redis
# project failed composer's platform check even with the extension "installed". Building it from
# PECL instead gets a current version; the build toolchain above (php-dev/build-essential) is only
# needed for this compile step and could be removed afterward, but keeping the image slim further
# isn't worth the extra Dockerfile complexity for a dev-tooling container that isn't shipped.
RUN pecl channel-update pecl.php.net \
    && pecl install redis \
    && echo "extension=redis.so" > /etc/php/8.3/cli/conf.d/20-redis.ini

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectForge.Worker.dll"]
