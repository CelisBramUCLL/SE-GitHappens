#!/usr/bin/env pwsh

Write-Host "🌱 Running Database Seeder..." -ForegroundColor Green

# Change to the backend directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendPath = Split-Path -Parent $scriptPath
Set-Location $backendPath

Write-Host "📂 Working directory: $backendPath" -ForegroundColor Yellow

# Build the project first
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    
    # Set environment variable to trigger seeding
    $env:SEED_DATABASE = "true"
    
    Write-Host "🌱 Starting application with seeding enabled..." -ForegroundColor Yellow
    
    # Run the application (it will seed and then exit)
    dotnet run
    
    Write-Host "✅ Seeding completed!" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}