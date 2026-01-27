Write-Host "=== Testing Market Price API (Database) ===" -ForegroundColor Cyan
Write-Host "Make sure backend is running on http://localhost:5000" -ForegroundColor Yellow
Write-Host ""

$baseUrl = "http://localhost:5000/api/MarketPrice"

# Test 1: Get latest prices
Write-Host "1️⃣ Testing GET /api/MarketPrice (Latest prices)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri $baseUrl
    
    if ($response.success) {
        Write-Host "✅ SUCCESS!" -ForegroundColor Green
        Write-Host "`nCommodities:" -ForegroundColor Cyan
        $response.data.commodities | ForEach-Object {
            Write-Host "  - $($_.name): $($_.currentPrice) $($_.unit) (Change: $($_.change))" -ForegroundColor White
        }
        
        Write-Host "`nRegional Prices:" -ForegroundColor Cyan
        $response.data.regionalPrices | ForEach-Object {
            Write-Host "  - $($_.region): $($_.coffeePrice) VND/kg (Change: $($_.change))" -ForegroundColor White
        }
    } else {
        Write-Host "❌ API returned error: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n" -NoNewline

# Test 2: Update price (Admin)
Write-Host "2️⃣ Testing POST /admin/update (Update single price)..." -ForegroundColor Yellow
try {
    $updateRequest = @{
        productId = "00000000-0000-0000-0000-000000000001" # Cà phê Robusta Seed ID
        productName = "Cà phê Robusta"
        regionCode = "DAK_LAK"
        region = "Đắk Lắk"
        price = 102500
        change = 1600
        source = "Manual test"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/admin/update" -Method POST -Body $updateRequest -ContentType "application/json"
    
    if ($response.success) {
        Write-Host "✅ SUCCESS! Updated: $($response.data.region) = $($response.data.price)" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n" -NoNewline

# Test 3: Get history
Write-Host "3️⃣ Testing GET /history (Price history)..." -ForegroundColor Yellow
try {
    # Using ProductId in query
    $response = Invoke-RestMethod -Uri "$baseUrl/history?productId=00000000-0000-0000-0000-000000000001&regionCode=DAK_LAK&limit=5"
    
    if ($response.success) {
        Write-Host "✅ SUCCESS! Found $($response.data.Count) records" -ForegroundColor Green
        Write-Host "`nPrice History (Đắk Lắk - Coffee):" -ForegroundColor Cyan
        $response.data | ForEach-Object {
            $date = ([DateTime]$_.recordedDate).ToString("yyyy-MM-dd")
            Write-Host "  - $date: $($_.price) VND/kg (Change: $($_.change))" -ForegroundColor White
        }
    } else {
        Write-Host "❌ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test completed ===" -ForegroundColor Cyan
