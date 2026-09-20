# Matgar API - Configuration & Security Guide

This document outlines the architecture, configuration strategy, secrets management, and execution instructions for the **Matgar.Api** ASP.NET Core project.

---

## 1. Configuration Architecture & Files

The project follows standard ASP.NET Core configuration hierarchy and the **Options Pattern**.

- **`appsettings.json`**: Contains shared, non-sensitive defaults (Logging, AllowedHosts, CORS defaults, RateLimiting defaults). It **never** contains secrets.
- **`appsettings.Development.json.example`**: Template for local development. Copy to `appsettings.Development.json` (or use User Secrets) and populate with local connection strings and keys.
- **`appsettings.Docker.json.example`**: Template for Docker environment configurations.
- **`appsettings.Production.json.example`**: Production template containing non-sensitive structural configuration and placeholders.

---

## 2. Environments Overview

| Environment | Purpose | Secrets Source |
|---|---|---|
| **Development** | Local debugging & testing | `dotnet user-secrets` (never committed) |
| **Docker** | Containerized local testing | `.env` file or Docker environment variables |
| **Production** | Live deployment | Platform Secret Manager / Environment variables / Key Vault |

---

## 3. Secrets Strategy & Security

- **Never** hardcode database passwords, JWT signing keys, SMTP passwords, or API keys in configuration files or source code.
- **Never** commit `.env`, `appsettings.Development.json`, `appsettings.Docker.json`, or `appsettings.Production.json` to version control.
- Only `.example` template files are tracked in Git.

---

## 4. Local Development with User Secrets

To set up development secrets without risking accidental commits:

1. Navigate to the API project directory:
   ```powershell
   cd src/Matgar.Api
   ```
2. Initialize User Secrets (if not already initialized):
   ```powershell
   dotnet user-secrets init
   ```
3. Set your development secrets:
   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=.;Initial Catalog=MatgarDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
   dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
   dotnet user-secrets set "JwtOptions:Key" "YOUR_SECURE_JWT_SIGNING_KEY_MIN_32_BYTES"
   dotnet user-secrets set "EmailOptions:Email" "your-email@gmail.com"
   dotnet user-secrets set "EmailOptions:Password" "your-gmail-app-password"
   ```
4. Verify secrets:
   ```powershell
   dotnet user-secrets list
   ```

---

## 5. Running Locally

1. Ensure prerequisites (.NET 10 SDK, SQL Server, Redis) are running.
2. Run the application using the HTTP or HTTPS development launch profile:
   ```powershell
   dotnet run --project src/Matgar.Api --launch-profile http
   ```

---

## 6. Running with Docker & Docker Compose

1. Copy the environment template to create your local `.env` file:
   ```powershell
   Copy-Item .env.example .env
   ```
2. Open `.env` and fill in your actual passwords and keys.
3. Start the application and dependencies with Docker Compose:
   ```powershell
   docker compose up --build
   ```
4. The API will be available at `http://localhost:5203`.

---

## 7. Production Configuration & Deployment

- In production, do not deploy `.env` or appsettings files containing secrets.
- Inject secrets via hosting platform environment variables using double-underscore hierarchical syntax:
  - `ConnectionStrings__DefaultConnection`
  - `JwtOptions__Key`
  - `EmailOptions__Password`
- Alternatively, use cloud secret managers (Azure Key Vault, AWS Secrets Manager, Kubernetes Secrets).

---

## 8. Ignored Files (Never Commit)

The following files are strictly ignored by `.gitignore`:
- `.env` and `.env.*`
- `appsettings.Development.json`
- `appsettings.Docker.json`
- `appsettings.Production.json`
