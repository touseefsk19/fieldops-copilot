# ---- Stage 1: build with the full SDK ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, on its own layer: it's only redone when the .csproj changes
COPY src/FieldOps.Api/FieldOps.Api.csproj src/FieldOps.Api/
RUN dotnet restore src/FieldOps.Api/FieldOps.Api.csproj

# Then the code, and publish a Release build
COPY src/FieldOps.Api/ src/FieldOps.Api/
RUN dotnet publish src/FieldOps.Api/FieldOps.Api.csproj -c Release -o /app/publish --no-restore

# ---- Stage 2: run with the small ASP.NET runtime only (no SDK, no source code) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as the image's built-in non-root user, not root
USER $APP_UID

ENTRYPOINT ["dotnet", "FieldOps.Api.dll"]