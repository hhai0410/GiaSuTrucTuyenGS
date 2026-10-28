param([switch]$CreateAdmin, [switch]$Sqlite, [switch]$PickupEmail)
. "$PSScriptRoot/Common.ps1"
$previousEmailMode = $env:Email__Mode
$previousDatabaseProvider = $env:DatabaseProvider
$previousConnection = $env:ConnectionStrings__DefaultConnection
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
function Restore-SmtpEnvironment { }
try {
    if (!$PickupEmail) { . "$PSScriptRoot/Import-Smtp.ps1" }
    if ($PickupEmail) { $env:Email__Mode = 'Pickup' }
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    if ($Sqlite) {
        $env:DatabaseProvider = 'Sqlite'
        $env:ConnectionStrings__DefaultConnection = 'Data Source=App_Data/gs-dev.db'
    }
    if ($CreateAdmin) {
        $env:SeedAdmin__Email = Read-Host 'Email quản trị mới (không dùng email đã tự đăng ký)'
        $adminSecurePassword = Read-Host 'Mật khẩu quản trị (ít nhất 8 ký tự)' -AsSecureString
        $env:SeedAdmin__Password = [System.Net.NetworkCredential]::new('', $adminSecurePassword).Password
    }
    if ($env:Email__Mode -eq 'Smtp') { Write-Host 'Email: SMTP thật (đã nạp cấu hình riêng trên máy).' }
    else { Write-Host 'Email: dùng cấu hình mặc định; Development lưu thư thử vào App_Data/mail.' }
    Write-Host 'Mở https://localhost:7043. Nhấn Ctrl+C tại đây để dừng web.'
    & $dotnet run --project src/GiaSuOnline.Web --launch-profile https --no-restore
    Assert-Command
} finally {
    Restore-SmtpEnvironment
    $env:Email__Mode = $previousEmailMode
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    Remove-Item Env:SeedAdmin__Email,Env:SeedAdmin__Password -ErrorAction SilentlyContinue
    if ($Sqlite) { $env:DatabaseProvider = $previousDatabaseProvider; $env:ConnectionStrings__DefaultConnection = $previousConnection }
}
