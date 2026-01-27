Write-Host "`n=== Testing Puppeteer with Debug Logs ===" -ForegroundColor Cyan
Write-Host "Check logs at: AgriLink_DH.Api/Logs/*.log" -ForegroundColor Gray
Write-Host "`nSending request... (may take 60s)`n" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/MarketPrice/regional/puppeteer" -TimeoutSec 90
    
    if ($response.success) {
        Write-Host "✅ SUCCESS!" -ForegroundColor Green
        Write-Host "`nPrices:" -ForegroundColor Cyan
        $response.data.regionalPrices | ForEach-Object {
            $sign = if ($_.change -gt 0) { "+" } elseif ($_.change -lt 0) { "" } else { "" }
            $color = if ($_.change -gt 0) { "Green" } elseif ($_.change -lt 0) { "Red" } else { "Yellow" }
            Write-Host "  $($_.region): " -NoNewline
            Write-Host "$($_.coffeePrice) VND/kg " -NoNewline -ForegroundColor White
            Write-Host "($sign$($_.change))" -ForegroundColor $color
        }
    } else {
        Write-Host "❌ FAILED: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n📋 Check debug logs with:" -ForegroundColor Gray
Write-Host "Get-Content 'AgriLink_DH.Api\Logs\errors\error-$(Get-Date -Format yyyyMMdd).log' -Tail 50" -ForegroundColor Gray
