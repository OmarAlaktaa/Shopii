# Build stage: .NET 8 SDK image (musl/Alpine variant, matching net8.0).
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy only the project files first. This lets Docker cache the NuGet restore
# layer until a .csproj (or central package/build file) actually changes.
COPY ["Shopii.API/Shopii.API.csproj", "Shopii.API/"]
COPY ["Shopii.Application/Shopii.Application.csproj", "Shopii.Application/"]
COPY ["Shopii.Domain/Shopii.Domain.csproj", "Shopii.Domain/"]
COPY ["Shopii.Infrastructure/Shopii.Infrastructure.csproj", "Shopii.Infrastructure/"]

# Restoring the API project also restores its ProjectReferences (Application
# and Infrastructure, and transitively Domain).
RUN dotnet restore "Shopii.API/Shopii.API.csproj"

# Copy the remaining source. bin/obj, local databases, secrets and IDE files
# are excluded by .dockerignore.
COPY . .

# Publish the API as a framework-dependent app. UseAppHost=false avoids
# producing a native launcher we do not need in the runtime image.
RUN dotnet publish "Shopii.API/Shopii.API.csproj" \
    --configuration "$BUILD_CONFIGURATION" \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# Runtime stage: ASP.NET Core 8 runtime image (no SDK, no build tooling).
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

WORKDIR /app

# Production environment, HTTP-only listener inside the container, and a
# container-specific SQLite path. The path lives on a volume at runtime.
# Jwt__Key is intentionally NOT set here: inject it at run time.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/ShopiiDb.db"

# Copy the published output and give the built-in non-root "app" user
# ownership. /app/data is the mount point for the persistent SQLite volume.
COPY --from=build --chown=app:app /app/publish ./
RUN mkdir -p /app/data && chown -R app:app /app/data

# .NET 8 images include the non-root "app" user (UID 1654).
USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "Shopii.API.dll"]
