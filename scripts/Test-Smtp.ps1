param([Parameter(Mandatory = $true)][string]$To)
. "$PSScriptRoot/Common.ps1"
$null = [System.Net.Mail.MailAddress]::new($To)
. "$PSScriptRoot/Import-Smtp.ps1"
$previousRecipient = $env:SmtpCheckRecipient
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    if (!(Test-Path -LiteralPath (Join-Path $projectRoot '.local/smtp.json')) -or !(Test-Path -LiteralPath (Join-Path $projectRoot '.local/smtp-credential.xml'))) {
        throw 'Chưa cấu hình SMTP. Chạy Configure-Smtp.ps1 trước.'
    }
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:SmtpCheckRecipient = $To
    # Không khởi động web, không tạo tài khoản, không cần kết nối SQL Server.
    & $dotnet run --project src/GiaSuOnline.Web --no-launch-profile --no-build -- --SmtpCheck=true
    Assert-Command
} finally {
    Restore-SmtpEnvironment
    $env:SmtpCheckRecipient = $previousRecipient
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}
