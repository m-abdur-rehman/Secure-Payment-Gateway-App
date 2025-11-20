# Security Setup Instructions

## Quick Start

This project uses **User Secrets** to store sensitive configuration data. Follow these steps to set up your development environment.

## Step 1: Initialize User Secrets

```bash
cd WebApplication1
dotnet user-secrets init
```

## Step 2: Add Your Configuration

### Database Connection String
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Forex API Key
```bash
dotnet user-secrets set "Forex:ApiKey" "YOUR_FOREX_API_KEY"
```

## Step 3: Apply Database Migration

After setting up User Secrets, apply the database migration for idempotency key support:

```bash
dotnet ef database update
```

## Step 4: Verify Setup

Run the application:
```bash
dotnet run
```

