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

# ── Java 21 + Maven via SDKMAN — same versions the monolithic image used ──
RUN apt-get update && apt-get install -y --no-install-recommends curl unzip zip ca-certificates \
    && curl -s "https://get.sdkman.io" | bash \
    && bash -c "source /root/.sdkman/bin/sdkman-init.sh \
        && sdk install java 21.0.5-tem \
        && sdk install maven 3.9.9 \
        && sdk flush archives" \
    && rm -rf /var/lib/apt/lists/*

ENV JAVA_HOME=/root/.sdkman/candidates/java/current
ENV MAVEN_HOME=/root/.sdkman/candidates/maven/current
ENV PATH="${JAVA_HOME}/bin:${MAVEN_HOME}/bin:${PATH}"

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectForge.Worker.dll"]
