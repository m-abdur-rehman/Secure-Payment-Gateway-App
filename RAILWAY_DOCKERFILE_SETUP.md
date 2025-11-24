# Railway Dockerfile Setup

## Problem
Railway was using Nixpacks with .NET 6.0 instead of .NET 8.0, causing build failures.

## Solution
1. Created `Dockerfile` that explicitly uses .NET 8.0
2. Updated `railway.json` to use Dockerfile builder
3. Removed `.nixpacks.toml` to prevent Nixpacks from being used

## Files

### Dockerfile
- Uses .NET 8.0 SDK for building
- Uses .NET 8.0 SDK for runtime (needed for migrations)
- Automatically runs database migrations on startup
- Listens on port 8080

### railway.json
- Now specifies `"builder": "DOCKERFILE"` to force Railway to use the Dockerfile

## Next Steps

1. Commit and push:
   ```bash
   git add .
   git commit -m "Force Railway to use Dockerfile with .NET 8.0"
   git push
   ```

2. Railway will now:
   - Use the Dockerfile (not Nixpacks)
   - Build with .NET 8.0 SDK
   - Run migrations automatically
   - Start the application

## Environment Variables

Make sure these are set in Railway Dashboard:
```
ConnectionStrings__DefaultConnection = <Your MySQL connection string>
Forex__ApiKey = <Your API key>
ASPNETCORE_ENVIRONMENT = Production
```

## Verification

After deployment, check Railway logs to confirm:
- Build uses .NET 8.0 SDK
- Migrations run successfully
- Application starts on the correct port

