# Secure Payment Gateway

A secure, production-ready payment gateway application built with ASP.NET Core MVC.

## Features

- **Payment Processing**: Multi-currency payment support (PKR, USD, AED) with automatic conversion
- **Transaction Management**: View and lookup transactions with pagination
- **Security**: Comprehensive security implementation.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- SQL Server (LocalDB, Express, or full instance)
- Visual Studio 2022 or VS Code (optional, for development)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd newproject/WebApplication1
```

### 2. Configure User Secrets

The application uses User Secrets to store sensitive configuration data (API keys, connection strings).

#### Initialize User Secrets

```bash
dotnet user-secrets init
```

#### Set Database Connection String

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Example for LocalDB:**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=PaymentGatewayDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

#### Set Forex API Key

Get your API key from [APILayer](https://apilayer.com/marketplace/exchangerates_data-api) and set it:

```bash
dotnet user-secrets set "Forex:ApiKey" "YOUR_API_KEY_HERE"
```

### 3. Update Database

Apply the database migrations:

```bash
dotnet ef database update
```

This will create the necessary tables in your database.

### 4. Run the Application

```bash
dotnet run
```

The application will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Configuration

### User Secrets (Development)

For development, sensitive data is stored in User Secrets. The following keys are required:

- `ConnectionStrings:DefaultConnection` - SQL Server connection string
- `Forex:ApiKey` - APILayer Forex API key

### appsettings.json

The `appsettings.json` file contains non-sensitive configuration:

- **Forex API Base URL**: `https://api.apilayer.com/exchangerates_data`

### Production Configuration

For production deployments, use:
- **Azure Key Vault** (Azure deployments)
- **AWS Secrets Manager** (AWS deployments)
- **Environment Variables** (any platform)
- **HashiCorp Vault** (enterprise)

See `USER_SECRETS_SETUP.md` for detailed instructions.


## API Endpoints

### POST /api/transactions
Create a new payment transaction.

**Request Body:**
```json
{
  "bankAccountNumber": "123456789012",
  "bankName": "Bank Name",
  "cnic": "12345-1234567-1",
  "currency": "PKR",
  "amount": 1000.00,
  "email": "user@example.com",
  "mobileNumber": "+923001234567",
  "address": "Address",
  "idempotencyKey": "optional-uuid"
}
```

**Response:**
```json
{
  "transactionId": "T20251120181621-WVEgS39cyj2W",
  "createdAt": "2025-11-20T18:16:21Z"
}
```

### GET /api/transactions?page=1&pageSize=20
Get paginated list of transactions.

### GET /api/transactions/{transactionId}?mobile={mobileNumber}
Lookup a specific transaction by ID and mobile number.

## Currency Limits

- **PKR**: Minimum 0.01, Maximum 1,000,000
- **USD**: Minimum 0.01, Maximum 3,500
- **AED**: Minimum 0.01, Maximum 13,000

## Development

### Running in Development Mode

```bash
dotnet run
```

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating Migrations

```bash
dotnet ef migrations add MigrationName
```

### Applying Migrations

```bash
dotnet ef database update
```

## Troubleshooting

### Database Connection Issues

- Verify SQL Server is running
- Check connection string in User Secrets
- Ensure database exists or use `TrustServerCertificate=True` for local development

### Forex API Errors

- Verify API key is set correctly in User Secrets
- Check API key is active and has available quota
- Review application logs for detailed error messages

### Rate Limit Exceeded

- Wait for the rate limit window to reset (1 minute)
- Check rate limit configuration in `appsettings.json`
- Consider adjusting limits for development

## License

This project is for demonstration purposes.

## Additional Documentation

- `USER_SECRETS_SETUP.md` - Detailed User Secrets setup guide
- `README_SECURITY_SETUP.md` - Security setup instructions
- `SECURITY.md` - Comprehensive security documentation

