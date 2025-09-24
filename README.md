# Filmowanie-2

This application consists of an ASP.NET Core backend and a frontend application. Below are instructions on how to run the application in different modes.

## Prerequisites

- .NET 8.0 or later
- Node.js and pnpm package manager
- Docker (required for LocalWithCosmosEmulator mode)
- Azure Cosmos DB connection string (if not using emulator)

## Startup Modes

The application can be run in different modes, controlled by the `StartupMode` setting in `appsettings.Development.json`. Here are the available options:

### 1. Production Mode
```json
"StartupMode": "Production"
```
⚠️ This mode is for production deployment only. Do not use it for local development.

### 2. Local with Dev Frontend
```json
"StartupMode": "LocalWithDevFrontend"
```
This mode uses a dev server proxy for the frontend. To run the application:

1. Navigate to the ClientApp directory:
   ```powershell
   cd ClientApp
   ```

2. Install dependencies and start the dev server:
   ```powershell
   pnpm install
   pnpm run dev
   ```

3. In another terminal, build and run the ASP.NET application:
   ```powershell
   dotnet build
   dotnet run
   ```

### 3. Local with Compiled Frontend
```json
"StartupMode": "LocalWithCompiledFrontend"
```
This mode uses compiled frontend assets. To run the application:

1. Navigate to the ClientApp directory and build the frontend:
   ```powershell
   cd ClientApp
   pnpm install
   pnpm run build
   ```

2. Build and run the ASP.NET application:
   ```powershell
   dotnet build
   dotnet run
   ```

### 4. Local with Cosmos DB Emulator
```json
"StartupMode": "LocalWithCosmosEmulator"
```
This mode automatically starts Azure Cosmos DB emulator in a container and populates it with test data. You'll need to run this with elevated privilege as this will try to add emulator container's cert to trusted ones in cert store.

Prerequisites:
- Docker must be running on your machine
- Docker Desktop (for Windows/Mac) or Docker Engine (for Linux)

1. Navigate to the ClientApp directory:
   ```powershell
   cd ClientApp
   ```

2. Install dependencies and start the dev server:
   ```powershell
   pnpm install
   pnpm run dev
   ```

3. In another terminal, build and run the ASP.NET application:
   ```powershell
   dotnet build
   dotnet run
   ```

The application will automatically:
- Start the Cosmos DB emulator in a container
- Configure the connection
- Hydrate the database with test data

### 5. Local with Cosmos DB Emulator and Compiled Frontend
```json
"StartupMode": "LocalWithCosmosEmulator"
```
This mode automatically starts Azure Cosmos DB emulator in a container and populates it with test data. You'll need to run this with elevated privilege as this will try to add emulator container's cert to trusted ones in cert store.

Prerequisites:
- Docker must be running on your machine
- Docker Desktop (for Windows/Mac) or Docker Engine (for Linux)

1. Navigate to the ClientApp directory and build the frontend:
   ```powershell
   cd ClientApp
   pnpm install
   pnpm run build
   ```

2. Build and run the ASP.NET application:
   ```powershell
   dotnet build
   dotnet run
   ```

3. In another terminal, build and run the ASP.NET application:
   ```powershell
   dotnet build
   dotnet run
   ```

The application will automatically:
- Start the Cosmos DB emulator in a container
- Configure the connection
- Hydrate the database with test data

## Manual Azure Cosmos DB Configuration

If you're not using the LocalWithCosmosEmulator mode, you'll need to:

1. Provide a valid Azure Cosmos DB connection string in `appsettings.Development.json`:
   ```json
   "dbConnectionString": "your-cosmos-db-connection-string"
   ```

2. Manually create a saga instance in the database

## Notes

- The application uses pnpm as the package manager for the frontend. Make sure to use pnpm commands instead of npm.
- When switching between modes, make sure to update the `StartupMode` in `appsettings.Development.json`