#!/bin/bash
# ─────────────────────────────────────────────────────────────────────────────
# ProjectForge – Script de configuración inicial (.NET 10)
# Uso: chmod +x scripts/setup.sh && ./scripts/setup.sh
# ─────────────────────────────────────────────────────────────────────────────
set -e

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; CYAN='\033[0;36m'; NC='\033[0m'

echo -e "${CYAN}"
echo "  ██████╗ ██████╗  ██████╗      ██╗███████╗ ██████╗████████╗    ███████╗ ██████╗ ██████╗  ██████╗ ███████╗"
echo "  ██╔══██╗██╔══██╗██╔═══██╗     ██║██╔════╝██╔════╝╚══██╔══╝    ██╔════╝██╔═══██╗██╔══██╗██╔════╝ ██╔════╝"
echo "  ██████╔╝██████╔╝██║   ██║     ██║█████╗  ██║        ██║       █████╗  ██║   ██║██████╔╝██║  ███╗█████╗  "
echo "  ██╔═══╝ ██╔══██╗██║   ██║██   ██║██╔══╝  ██║        ██║       ██╔══╝  ██║   ██║██╔══██╗██║   ██║██╔══╝  "
echo "  ██║     ██║  ██║╚██████╔╝╚█████╔╝███████╗╚██████╗   ██║       ██║     ╚██████╔╝██║  ██║╚██████╔╝███████╗"
echo "  ╚═╝     ╚═╝  ╚═╝ ╚═════╝  ╚════╝ ╚══════╝ ╚═════╝   ╚═╝       ╚═╝      ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝"
echo -e "${NC}"
echo -e "${GREEN}  Setup para .NET 10${NC}\n"

# ── 1. Verificar dependencias ─────────────────────────────────────────────────
echo -e "${YELLOW}[1/5] Verificando dependencias...${NC}"

check_cmd() {
  if ! command -v "$1" &>/dev/null; then
    echo -e "  ${RED}✗ '$1' no está instalado. Instálalo antes de continuar.${NC}"
    exit 1
  fi
  echo -e "  ${GREEN}✓${NC} $1"
}

check_cmd dotnet
check_cmd git
check_cmd docker

# Verificar .NET 10
DOTNET_VER=$(dotnet --version 2>/dev/null || echo "0.0.0")
DOTNET_MAJOR=$(echo "$DOTNET_VER" | cut -d. -f1)
if [ "$DOTNET_MAJOR" -lt 10 ]; then
  echo -e "  ${RED}✗ Se requiere .NET SDK 10.0 o superior. Versión detectada: $DOTNET_VER${NC}"
  echo -e "  ${YELLOW}  Instálalo desde: https://dotnet.microsoft.com/download/dotnet/10.0${NC}"
  exit 1
fi
echo -e "  ${GREEN}✓${NC} .NET SDK $DOTNET_VER"

# ── 2. Variables de entorno ───────────────────────────────────────────────────
echo -e "\n${YELLOW}[2/5] Configurando variables de entorno...${NC}"
if [ ! -f .env ]; then
  cp .env.example .env
  echo -e "  ${GREEN}✓ .env creado desde .env.example${NC}"
  echo -e "  ${RED}  ⚠  Edita .env con tus credenciales antes de continuar:${NC}"
  echo -e "  ${YELLOW}     - GITHUB_CLIENT_ID / GITHUB_CLIENT_SECRET${NC}"
  echo -e "  ${YELLOW}     - ANTHROPIC_API_KEY${NC}"
  echo -e "  ${YELLOW}     - ENCRYPTION_KEY (mínimo 32 caracteres)${NC}"
  echo ""
  read -rp "  ¿Ya editaste el .env? (s/n): " yn
  [ "$yn" != "s" ] && echo -e "  ${YELLOW}Edita .env y vuelve a ejecutar el script.${NC}" && exit 0
else
  echo -e "  ${GREEN}✓ .env ya existe${NC}"
fi

# ── 3. Restaurar paquetes NuGet ───────────────────────────────────────────────
echo -e "\n${YELLOW}[3/5] Restaurando paquetes NuGet...${NC}"
dotnet restore ProjectForge.sln --verbosity minimal
echo -e "  ${GREEN}✓ Paquetes restaurados${NC}"

# ── 4. Iniciar SQL Server ─────────────────────────────────────────────────────
echo -e "\n${YELLOW}[4/5] Iniciando SQL Server con Docker...${NC}"
source .env 2>/dev/null || true
docker-compose up -d sqlserver
echo -n "  Esperando que SQL Server esté listo"
for i in $(seq 1 30); do
  if docker-compose exec -T sqlserver \
       /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
       -P "${DB_PASSWORD:-ProjectForge!23}" -Q "SELECT 1" -No &>/dev/null; then
    echo -e "\n  ${GREEN}✓ SQL Server listo${NC}"
    break
  fi
  echo -n "."
  sleep 2
done

# ── 5. Migraciones ────────────────────────────────────────────────────────────
echo -e "\n${YELLOW}[5/5] Aplicando migraciones EF Core...${NC}"
cd src/ProjectForge.Web
dotnet ef database update \
  --project ../ProjectForge.Infrastructure/ProjectForge.Infrastructure.csproj \
  --startup-project . \
  --verbose
cd ../..
echo -e "  ${GREEN}✓ Base de datos lista${NC}"

# ── Listo ─────────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}╔══════════════════════════════════════════════════╗"
echo -e "║  ✅  Setup completado – ProjectForge .NET 10     ║"
echo -e "╚══════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "  Para iniciar en desarrollo:"
echo -e "    ${YELLOW}cd src/ProjectForge.Web && dotnet run${NC}"
echo ""
echo -e "  O con Docker Compose (producción):"
echo -e "    ${YELLOW}docker-compose up --build${NC}"
echo ""
echo -e "  Aplicación disponible en: ${CYAN}http://localhost:5000${NC}"
echo -e "  OpenAPI (dev):            ${CYAN}http://localhost:5000/openapi/v1.json${NC}"
