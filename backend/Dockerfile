# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj", "src/WahadiniCryptoQuest.API/"]
COPY ["src/WahadiniCryptoQuest.Core/WahadiniCryptoQuest.Core.csproj", "src/WahadiniCryptoQuest.Core/"]
COPY ["src/WahadiniCryptoQuest.DAL/WahadiniCryptoQuest.DAL.csproj", "src/WahadiniCryptoQuest.DAL/"]
COPY ["src/WahadiniCryptoQuest.Service/WahadiniCryptoQuest.Service.csproj", "src/WahadiniCryptoQuest.Service/"]

# Restore dependencies
RUN dotnet restore "src/WahadiniCryptoQuest.API/WahadiniCryptoQuest.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/WahadiniCryptoQuest.API"
RUN dotnet build "WahadiniCryptoQuest.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WahadiniCryptoQuest.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "WahadiniCryptoQuest.API.dll"]
