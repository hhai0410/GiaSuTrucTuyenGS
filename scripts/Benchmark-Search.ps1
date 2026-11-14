param([ValidatePattern('^[a-zA-Z0-9_.\\,\-]+$')][string]$SqlServer = 'LOQ\SQLEXPRESS')
. "$PSScriptRoot/Common.ps1"
$benchmarkPrevious = @{}
$benchmarkValues = @{
    ASPNETCORE_ENVIRONMENT = 'Development'
    DatabaseProvider = 'SqlServer'
    ConnectionStrings__DefaultConnection = "Server=$SqlServer;Database=GiaSuOnlineGS_Demo;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15"
    Logging__LogLevel__Default = 'Warning'
}
try {
    foreach ($benchmarkKey in $benchmarkValues.Keys) {
        $benchmarkPrevious[$benchmarkKey] = [Environment]::GetEnvironmentVariable($benchmarkKey, 'Process')
        [Environment]::SetEnvironmentVariable($benchmarkKey, $benchmarkValues[$benchmarkKey], 'Process')
    }
    Write-Host 'Tạo/đọc 500 hồ sơ giả trong GiaSuOnlineGS_Demo. Không seed vào database chính và không gửi email.'
    & $dotnet run --project src/GiaSuOnline.Web --no-launch-profile --no-build -- --SearchBenchmark=true
    Assert-Command
} finally {
    foreach ($benchmarkKey in $benchmarkPrevious.Keys) {
        [Environment]::SetEnvironmentVariable($benchmarkKey, $benchmarkPrevious[$benchmarkKey], 'Process')
    }
}
