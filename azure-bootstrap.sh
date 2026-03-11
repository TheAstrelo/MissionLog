#!/bin/bash
# ============================================================
#  MissionLog — Azure Bootstrap Script
#  Run ONCE to provision all Azure resources.
#  Prerequisites: Azure CLI installed + logged in (az login)
#
#  Usage:
#    chmod +x azure-bootstrap.sh
#    ./azure-bootstrap.sh
# ============================================================
set -e

# ── CONFIG — edit these ──────────────────────────────────────
RESOURCE_GROUP="missionlog-rg"
LOCATION="eastus"
APP_SERVICE_PLAN="missionlog-plan"
API_APP_NAME="missionlog-api"          # Must be globally unique → becomes missionlog-api.azurewebsites.net
SQL_SERVER_NAME="missionlog-sql"       # Must be globally unique
SQL_DB_NAME="MissionLogDb"
SQL_ADMIN_USER="missionlog_admin"
SQL_ADMIN_PASS="MissionLog#Prod2026!"  # Change this before running
JWT_SECRET="MissionLog-Prod-JWT-Secret-Key-Min32Chars-$(openssl rand -hex 8)"
GITHUB_REPO="TheAstrelo/MissionLog"
# ────────────────────────────────────────────────────────────

echo ""
echo "⬡ MISSIONLOG — AZURE BOOTSTRAP"
echo "================================"
echo ""

# 1. Resource Group
echo "[1/8] Creating resource group: $RESOURCE_GROUP..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

# 2. App Service Plan (Free F1)
echo "[2/8] Creating App Service Plan (Free F1)..."
az appservice plan create \
  --name "$APP_SERVICE_PLAN" \
  --resource-group "$RESOURCE_GROUP" \
  --sku F1 \
  --is-linux \
  --output none

# 3. API App Service (.NET 8)
echo "[3/8] Creating API App Service: $API_APP_NAME..."
az webapp create \
  --name "$API_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_SERVICE_PLAN" \
  --runtime "DOTNETCORE:8.0" \
  --output none

# 4. Azure SQL Server (serverless)
echo "[4/8] Creating Azure SQL Server: $SQL_SERVER_NAME..."
az sql server create \
  --name "$SQL_SERVER_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASS" \
  --output none

# Allow Azure services to access SQL
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

# 5. Azure SQL Database (Free serverless tier)
echo "[5/8] Creating Azure SQL Database (serverless free tier)..."
az sql db create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DB_NAME" \
  --edition GeneralPurpose \
  --family Gen5 \
  --capacity 1 \
  --compute-model Serverless \
  --auto-pause-delay 60 \
  --min-capacity 0.5 \
  --output none

# 6. Build connection string
SQL_CONNECTION="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DB_NAME};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASS};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 7. Configure API App Service settings
echo "[6/8] Configuring API App Service environment variables..."
az webapp config appsettings set \
  --name "$API_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --settings \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "ConnectionStrings__DefaultConnection=$SQL_CONNECTION" \
    "Jwt__Key=$JWT_SECRET" \
    "Jwt__Issuer=MissionLog.API" \
    "Jwt__Audience=MissionLog.BlazorApp" \
  --output none

# 8. Create Static Web App (Blazor)
echo "[7/8] Creating Azure Static Web App..."
SWA_OUTPUT=$(az staticwebapp create \
  --name "missionlog-blazor" \
  --resource-group "$RESOURCE_GROUP" \
  --location "eastus2" \
  --source "https://github.com/$GITHUB_REPO" \
  --branch "main" \
  --app-location "src/MissionLog.BlazorApp" \
  --output-location "wwwroot" \
  --login-with-github \
  --output json)

SWA_URL=$(echo "$SWA_OUTPUT" | python3 -c "import sys,json; print(json.load(sys.stdin)['defaultHostname'])")
SWA_TOKEN=$(az staticwebapp secrets list --name "missionlog-blazor" --resource-group "$RESOURCE_GROUP" --query "properties.apiKey" -o tsv)

# Set CORS on API to allow the Static Web App origin
echo "[8/8] Updating CORS to allow Static Web App origin..."
az webapp config appsettings set \
  --name "$API_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --settings "Cors__AllowedOrigins=https://$SWA_URL" \
  --output none

# Get API publish profile for GitHub Actions
API_PUBLISH_PROFILE=$(az webapp deployment list-publishing-profiles \
  --name "$API_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --xml)

API_URL="https://${API_APP_NAME}.azurewebsites.net"

# ── OUTPUT ───────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║         BOOTSTRAP COMPLETE — SAVE THESE VALUES          ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║"
echo "║  API URL:    $API_URL"
echo "║  Blazor URL: https://$SWA_URL"
echo "║"
echo "╠══ ADD THESE SECRETS TO GITHUB ═══════════════════════════╣"
echo "║  github.com/$GITHUB_REPO/settings/secrets/actions"
echo "║"
echo "║  Secret name: AZURE_WEBAPP_PUBLISH_PROFILE"
echo "║  Secret value: (see below — copy everything between ===)"
echo "║"
echo "═══════════════════════════════════════════════════════════"
echo "$API_PUBLISH_PROFILE"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "║  Secret name: AZURE_STATIC_WEB_APPS_API_TOKEN"
echo "║  Secret value: $SWA_TOKEN"
echo ""
echo "║  Secret name: API_BASE_URL"
echo "║  Secret value: $API_URL"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""
echo "Next: paste those 3 secrets into GitHub, then push to main."
echo "The deploy workflow will fire automatically."
