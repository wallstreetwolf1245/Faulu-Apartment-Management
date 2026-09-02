try {
    $response = Invoke-WebRequest -Uri "http://localhost:5154/api/webhooks/stkpush/initiate" -Method Post -Headers @{ Authorization = "Bearer $token" } -ContentType "application/json" -Body '{"tenantId":1,"leaseId":1,"amount":1,"phoneNumber":"254708374149","rentalPeriod":"Test"}'
    Write-Output $response.Content
} catch {
    Write-Output $_.Exception.Response.StatusCode
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    Write-Output $reader.ReadToEnd()
}