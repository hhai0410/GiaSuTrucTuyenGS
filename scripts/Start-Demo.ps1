param(
    [ValidatePattern('^[a-zA-Z0-9_.\\,\-]+$')][string]$SqlServer = 'LOQ\SQLEXPRESS',
    [switch]$CreateAdmin
)
. "$PSScriptRoot/Common.ps1"
$demoPreviousConnection = $env:ConnectionStrings__DefaultConnection
$demoPreviousProvider = $env:DatabaseProvider
try {
    & "$PSScriptRoot/Benchmark-Search.ps1" -SqlServer $SqlServer
    Assert-Command
    $env:DatabaseProvider = 'SqlServer'
    $env:ConnectionStrings__DefaultConnection = "Server=$SqlServer;Database=GiaSuOnlineGS_Demo;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15"
    Write-Host 'Đang chạy database DEMO; mọi email lưu trên máy. Dừng bản web khác trước để tránh trùng cổng.'
    & "$PSScriptRoot/Start.ps1" -PickupEmail -CreateAdmin:$CreateAdmin
} finally {
    $env:ConnectionStrings__DefaultConnection = $demoPreviousConnection
    $env:DatabaseProvider = $demoPreviousProvider
}
