param([switch]$CreateAdmin, [switch]$Sqlite)
. "$PSScriptRoot/Common.ps1"
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
try {
    Write-Host 'Mở https://localhost:7043. Nhấn Ctrl+C tại đây để dừng web.'
    & $dotnet run --project src/GiaSuOnline.Web --launch-profile https --no-restore
    Assert-Command
} finally {
    Remove-Item Env:SeedAdmin__Email,Env:SeedAdmin__Password -ErrorAction SilentlyContinue
    if ($Sqlite) { Remove-Item Env:DatabaseProvider,Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue }
}
