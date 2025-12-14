# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Flowie.Api/Flowie.Api.csproj", "Flowie.Api/"]
RUN dotnet restore "Flowie.Api/Flowie.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/Flowie.Api"
RUN dotnet build "Flowie.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Flowie.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

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