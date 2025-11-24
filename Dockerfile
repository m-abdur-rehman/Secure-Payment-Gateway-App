FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY WebApplication1/*.csproj ./WebApplication1/
WORKDIR /app/WebApplication1
RUN dotnet restore

# Copy everything else and build
WORKDIR /app
COPY . .
WORKDIR /app/WebApplication1
RUN dotnet publish -c Release -o /app/out

# Use SDK image for runtime (needed for migrations)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy project files for migrations (to a separate location)
COPY --from=build /app/WebApplication1 /app/src/WebApplication1

# Restore the project in runtime stage (needed for ef migrations)
WORKDIR /app/src/WebApplication1
RUN dotnet restore

# Copy published app
WORKDIR /app
COPY --from=build /app/out /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

# Run migrations and start app
CMD cd /app/src/WebApplication1 && dotnet ef database update && cd /app && dotnet WebApplication1.dll
