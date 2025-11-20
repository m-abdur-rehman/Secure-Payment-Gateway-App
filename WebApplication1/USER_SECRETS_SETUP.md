# User Secrets Setup Guide

## Overview
This project uses **User Secrets** to store sensitive configuration data (API keys, connection strings) securely. User Secrets are stored outside the project directory and are **never committed to source control**.

## Setup Instructions

### 1. Initialize User Secrets

Open a terminal in the project directory and run:

```bash
dotnet user-secrets init
```

This creates a `UserSecretsId` in your `.csproj` file.

### 2. Add Sensitive Configuration

Add your sensitive data using the following commands:

#### Database Connection String
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
```

#### Forex API Key
```bash
dotnet user-secrets set "Forex:ApiKey" "YOUR_FOREX_API_KEY"
```

### 3. Verify Secrets

To view all stored secrets:
```bash
dotnet user-secrets list
```

### 4. Remove Secrets (if needed)
```bash
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
dotnet user-secrets remove "Forex:ApiKey"
```

## Location of User Secrets

- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- **macOS/Linux**: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

## Important Notes

1. ✅ **User Secrets are automatically loaded in Development mode**
2. ✅ **Secrets are NOT committed to Git** (they're stored outside the project)
3. ✅ **Each developer needs to set their own secrets**
4. ⚠️ **For Production**: Use Azure Key Vault, AWS Secrets Manager, or environment variables

## Production Deployment

For production environments, use one of these secure options:

1. **Azure Key Vault** (if using Azure)
2. **AWS Secrets Manager** (if using AWS)
3. **Environment Variables** (set in hosting platform)
4. **HashiCorp Vault** (for enterprise)

## Example Production Configuration

### Using Environment Variables:
```bash
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=ProdDb;..."
export Forex__ApiKey="your-production-api-key"
```

### Using Azure Key Vault:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Security Best Practices

- ✅ Never commit secrets to source control
- ✅ Use different secrets for Development/Staging/Production
- ✅ Rotate API keys regularly
- ✅ Use least privilege principle for database access
- ✅ Monitor secret access in production

