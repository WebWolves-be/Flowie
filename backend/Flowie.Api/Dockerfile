# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Flowie.Api/Flowie.Api.csproj", "Flowie.Api/"]
RUN dotnet restore "Flowie.Api/Flowie.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/Flowie.Api"

# Debug: Check if migration files exist
RUN echo "=== Checking for migration files ===" && \
    ls -la Shared/Infrastructure/Database/Migrations/ || echo "Migrations folder not found" && \
    find . -name "*Migration*.cs" -type f || echo "No migration files found"

RUN dotnet build "Flowie.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Flowie.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Debug: Check published DLL size
RUN echo "=== Published DLL info ===" && ls -la /app/publish/Flowie.Api.dll

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080

# Create logs directory
RUN mkdir -p /app/logs && chmod 777 /app/logs

ENTRYPOINT ["dotnet", "Flowie.Api.dll"]