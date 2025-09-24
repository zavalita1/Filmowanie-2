# Stage 1: Build Frontend
FROM node:24.8.0-alpine AS frontend-builder
WORKDIR /app

# Install pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

# Copy frontend project files
COPY ClientApp/package.json ClientApp/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY ClientApp .
#RUN pnpm builde2e

# Stage 2: Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-builder
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.sln .
COPY Filmowanie.csproj ./
COPY Filmowanie.Abstractions/Filmowanie.Abstractions.csproj ./Filmowanie.Abstractions/
COPY Filmowanie.Account/Filmowanie.Account.csproj ./Filmowanie.Account/
COPY Filmowanie.Database/Filmowanie.Database.csproj ./Filmowanie.Database/
COPY Filmowanie.Nomination/Filmowanie.Nomination.csproj ./Filmowanie.Nomination/
COPY Filmowanie.Notification/Filmowanie.Notification.csproj ./Filmowanie.Notification/
COPY Filmowanie.Voting/Filmowanie.Voting.csproj ./Filmowanie.Voting/
COPY Tests/Filmowanie.UnitTests/Filmowanie.UnitTests.csproj ./Tests/Filmowanie.UnitTests/
COPY Tests/Filmowanie.E2E/Filmowanie.E2E.csproj ./Tests/Filmowanie.E2E/
RUN dotnet restore Filmowanie.sln

# Copy everything else
COPY . ./
# Substitute config for E2E one.
COPY appsettings.E2E.json appsettings.json
# Copy the built frontend files to wwwroot
# COPY --from=frontend-builder app/dist ./wwwroot

# Build the application
RUN dotnet publish Filmowanie.csproj -c Development -o out -p:SkipFrontendBuild=Skip

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=backend-builder /app/out ./

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Filmowanie.dll"]