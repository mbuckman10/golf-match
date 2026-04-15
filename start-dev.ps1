# Start Golf Match Pro - Local Development
# Run from the repo root: .\start-dev.ps1

Write-Host "Starting Golf Match Pro..." -ForegroundColor Green

# Start API in background
Write-Host "Starting API server..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\GolfMatchPro.Api'; dotnet run"

# Start Web frontend in background
Write-Host "Starting Web dev server..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\GolfMatchPro.Web'; npm run dev"

Write-Host ""
Write-Host "API:      http://localhost:5189" -ForegroundColor Yellow
Write-Host "Swagger:  http://localhost:5189/swagger" -ForegroundColor Yellow
Write-Host "Frontend: http://localhost:5173" -ForegroundColor Yellow
Write-Host ""
Write-Host "Two new terminal windows opened. Close them to stop the servers." -ForegroundColor Gray
