# 🚀 MissionLog — Azure Deployment Guide

## Prerequisites

1. [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed
2. [Free Azure account](https://azure.microsoft.com/en-us/free/) (or existing subscription)
3. Logged in: `az login`

---

## Step 1 — Run the Bootstrap Script

This is a **one-time setup** that creates all Azure resources.

```bash
# Make executable
chmod +x azure-bootstrap.sh

# Edit the top of the file first:
#   API_APP_NAME  → must be globally unique (e.g. missionlog-api-yourname)
#   SQL_SERVER_NAME → must be globally unique (e.g. missionlog-sql-yourname)
#   SQL_ADMIN_PASS → change to a strong password

./azure-bootstrap.sh
```

The script will:
- Create a Resource Group
- Create a **Free F1 App Service** for the API
- Create an **Azure SQL Serverless** database (auto-pauses when idle)
- Create an **Azure Static Web App** for Blazor (free tier, global CDN)
- Configure all environment variables on the API
- Output the 3 GitHub secrets you need to paste

**Estimated time:** ~5 minutes

---

## Step 2 — Add GitHub Secrets

Go to: `https://github.com/TheAstrelo/MissionLog/settings/secrets/actions`

Add these 3 secrets from the bootstrap script output:

| Secret Name | Value |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | XML blob from bootstrap output |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Token from bootstrap output |
| `API_BASE_URL` | `https://missionlog-api.azurewebsites.net` |

---

## Step 3 — Push to Deploy

```bash
git push origin main
```

The CI pipeline runs first → if it passes, the deploy pipeline fires automatically.

**What deploys where:**

| Component | Azure Service | URL |
|---|---|---|
| ASP.NET Core API | App Service (F1 Free) | `https://missionlog-api.azurewebsites.net` |
| Swagger UI | App Service | `https://missionlog-api.azurewebsites.net/swagger` |
| Blazor WASM | Static Web Apps (Free) | `https://<auto-name>.azurestaticapps.net` |
| Database | Azure SQL Serverless | Internal to App Service |

---

## Step 4 — Custom Domain (Optional)

### Blazor (Static Web Apps — free custom domain)
```
Azure Portal → Static Web Apps → missionlog-blazor → Custom domains → Add
```

### API (App Service — requires paid tier for custom domain)
Free F1 tier does not support custom domains on the API.
Upgrade to B1 (~$13/month) if you need `api.yourdomain.com`.

---

## Environment Variables Reference

These are set automatically by the bootstrap script on the API App Service:

| Variable | Description |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string |
| `Jwt__Key` | JWT signing secret (auto-generated) |
| `Jwt__Issuer` | `MissionLog.API` |
| `Jwt__Audience` | `MissionLog.BlazorApp` |
| `Cors__AllowedOrigins` | Static Web App URL |

To update any value:
```bash
az webapp config appsettings set \
  --name missionlog-api \
  --resource-group missionlog-rg \
  --settings "Key=Value"
```

---

## Tear Down (Stop Billing)

```bash
# Delete everything — one command
az group delete --name missionlog-rg --yes --no-wait
```

Free tier resources have no ongoing cost, but this is useful if you want a clean slate.

---

## Troubleshooting

**API returns 500 on first request after deploy**
→ Azure SQL serverless auto-pauses. First request wakes it up — takes ~10 seconds.

**Blazor routing 404 on page refresh**
→ `staticwebapp.config.json` handles this via `navigationFallback`. If still failing, check it was included in the publish output.

**SignalR not connecting**
→ WebSockets must be enabled on App Service. Check:
```
Azure Portal → App Service → Configuration → General settings → Web sockets → On
```

**CORS errors in browser**
→ Confirm `Cors__AllowedOrigins` on the API App Service exactly matches your Static Web App URL (no trailing slash).
