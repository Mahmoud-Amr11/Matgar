# syntax=docker/dockerfile:1

# ------------------------------------------------------------------
# Build stage
# ------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/ ./src/

WORKDIR /source/src/Matgar.Api
RUN dotnet restore Matgar.Api.csproj
RUN dotnet publish Matgar.Api.csproj -c Release -o /app/publish --no-restore

# ------------------------------------------------------------------
# Runtime stage
# ------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Kestrel listens on 8080 (ASPNETCORE_URLS is set by docker-compose).
EXPOSE 8080

ENTRYPOINT ["dotnet", "Matgar.Api.dll"]