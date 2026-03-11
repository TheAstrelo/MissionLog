# 🚀 MissionLog — Deployment Guide

**Stack:** Railway (API + Postgres) + Cloudflare Pages (Blazor)
**Cost:** Free on both platforms. No credit card required for Railway hobby tier.
**Time:** ~15 minutes end to end.

---

## Architecture

```
Browser
  │
  ├── Cloudflare Pages (free, global CDN)
  │     └── Blazor WASM static files
  │           └── calls →
  │
  └── Railway (free hobby tier)
        ├── ASP.NET Core 8 API  (Dockerfile build)
        └── Postgres DB         (Railway managed)
```

---

## Step 1 — Deploy API to Railway

### 1a. Create Railway account
Go to [railway.app](https://railway.app) → Sign up with GitHub (free, no credit card).

### 1b. Create a new project
```
Railway dashboard → New Project → Deploy from GitHub repo
→ Select: TheAstrelo/MissionLog
→ Railway detects Dockerfile automatically
```

### 1c. Add Postgres database
```
Railway project → New Service → Database → PostgreSQL
→ Railway creates the DB and sets DATABASE_URL automatically
```

### 1d. Set environment variables
In your API service on Railway → Variables → Add:

| Variable | Value |
|---|---|
| `JWT__KEY` | Any random 40+ char string — e.g. `openssl rand -base64 40` |
| `CORS__ALLOWEDORIGINS` | Your Cloudflare Pages URL (add after Step 2) |

Railway automatically provides `DATABASE_URL` and `PORT` — you don't set those.

### 1e. Get your API URL
```
Railway → your API service → Settings → Networking → Generate Domain
→ Copy: https://missionlog-api-xxxx.up.railway.app
```

---

## Step 2 — Deploy Blazor to Cloudflare Pages

### 2a. Create Cloudflare account
Go to [pages.cloudflare.com](https://pages.cloudflare.com) → Sign up free.

### 2b. Connect GitHub repo
```
Cloudflare Pages → Create application → Connect to Git
→ Select: TheAstrelo/MissionLog
→ Framework preset: None (we handle the build in CI)
```

### 2c. Build settings
```
Build command:    (leave blank — we push pre-built output via wrangler)
Output directory: publish/blazor/wwwroot
```

> Actually the deploy workflow pushes the pre-built output via wrangler — no build config needed in CF.

---

## Step 3 — Wire it all together

### Add GitHub Secrets
`github.com/TheAstrelo/MissionLog/settings/secrets/actions`

| Secret | Where to get it |
|---|---|
| `API_BASE_URL` | Railway API domain from Step 1e |
| `CLOUDFLARE_API_TOKEN` | Cloudflare → My Profile → API Tokens → Create Token → "Edit Cloudflare Workers" template |
| `CLOUDFLARE_ACCOUNT_ID` | Cloudflare → Workers & Pages → right sidebar |

### Update CORS on Railway
Once you have your Cloudflare Pages URL:
```
Railway → API service → Variables → CORS__ALLOWEDORIGINS
→ Set to: https://missionlog.pages.dev   (your actual CF URL)
```

### Push to deploy
```bash
git push origin main
```
CI runs → passes → deploy workflow fires → Blazor publishes to Cloudflare Pages.
Railway auto-detects the push and rebuilds the API via Dockerfile simultaneously.

---

## Demo credentials (seeded automatically)

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `supervisor` | `Super123!` | Supervisor |
| `engineer` | `Eng123!` | Engineer |
| `tech` | `Tech123!` | Technician |

---

## Local development (Postgres via Docker)

If you want to dev against Postgres locally instead of SQL Server Express:

```bash
# Start Postgres
docker run -d \
  --name missionlog-db \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=MissionLogDb \
  -p 5432:5432 \
  postgres:16

# Run API
cd src/MissionLog.API
dotnet run
```

appsettings.json already has the right connection string for this setup.

---

## Tear down

```
Railway dashboard → Project → Settings → Delete Project
Cloudflare Pages → your project → Settings → Delete project
```

---

## Troubleshooting

**First request to API is slow (~5-10s)**
Railway free tier spins down after 30 min of inactivity. First request wakes it up.

**CORS error in browser console**
`CORS__ALLOWEDORIGINS` on Railway must exactly match your Cloudflare Pages URL — no trailing slash, correct protocol (https).

**Blazor page refresh returns 404**
`staticwebapp.config.json` handles routing fallback for Cloudflare Pages — make sure it's present in `wwwroot/`.

**Database errors on first deploy**
EF Core runs `db.Database.Migrate()` on startup — tables are created automatically. Check Railway logs if it fails.
