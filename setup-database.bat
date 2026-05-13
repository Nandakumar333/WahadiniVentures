@echo off
echo Starting PostgreSQL container...
docker-compose up -d

echo Waiting for database to be ready...
timeout /t 15 /nobreak

echo Running EF Core migrations...
cd backend\src\WahadiniCryptoQuest.API
dotnet ef database update --project ..\WahadiniCryptoQuest.DAL\WahadiniCryptoQuest.DAL.csproj

echo.
echo Database setup complete!
pause
