# Test Market Price API

Write-Host "=== Testing Market Price API Endpoints ===" -ForegroundColor Cyan

# Base URL
$baseUrl = "http://localhost:5000/api/MarketPrice"

Write-Host "`n1️⃣ Testing Manual Mock (Fastest)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/regional/manual" -Method Get
    Write-Host "✅ Manual Mock: " -NoNewline -ForegroundColor Green
    Write-Host "$($response.data.commodities[0].currentPrice) VND/kg" -ForegroundColor White
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n2️⃣ Testing Alpha Vantage API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/regional/conversion" -Method Get
    Write-Host "✅ Alpha Vantage: " -NoNewline -ForegroundColor Green
    Write-Host "$($response.data.commodities[0].name) - $($response.data.commodities[0].currentPrice) $($response.data.commodities[0].unit)" -ForegroundColor White
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n3️⃣ Testing Puppeteer Scraping (giacaphe.com)..." -ForegroundColor Yellow
Write-Host "⏳ This may take 30-60 seconds on first run (downloading Chromium)..." -ForegroundColor Gray
try {
    $startTime = Get-Date
    $response = Invoke-RestMethod -Uri "$baseUrl/regional/puppeteer" -Method Get -TimeoutSec 120
    $duration = ((Get-Date) - $startTime).TotalSeconds
    
    Write-Host "✅ Puppeteer Scraping completed in $([math]::Round($duration, 1))s" -ForegroundColor Green
    Write-Host "`nData from giacaphe.com:" -ForegroundColor Cyan
    
    foreach ($item in $response.data.regionalPrices) {
        $changeColor = if ($item.change -gt 0) { "Green" } elseif ($item.change -lt 0) { "Red" } else { "Yellow" }
        Write-Host "  - $($item.region): " -NoNewline
        Write-Host "$($item.coffeePrice) VND/kg" -NoNewline -ForegroundColor White
        Write-Host " (" -NoNewline
        Write-Host "$($item.change)" -NoNewline -ForegroundColor $changeColor
        Write-Host ")" -ForegroundColor White
    }
    
    if ($response.data.commodities | Where-Object { $_.code -eq "USD_VND" }) {
        $usd = $response.data.commodities | Where-Object { $_.code -eq "USD_VND" }
        Write-Host "`n  💵 USD/VND: $($usd.currentPrice)" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test completed ===" -ForegroundColor Cyan
