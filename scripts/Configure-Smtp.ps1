param(
    [Parameter(Mandatory = $true)][string]$Sender,
    [string]$SmtpHost = 'smtp.gmail.com',
    [ValidateRange(1, 65535)][int]$Port = 587,
    [switch]$SslOnConnect,
    [string]$Username
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if ([Environment]::OSVersion.Platform -ne 'Win32NT') { throw 'Script lưu mật khẩu mã hóa bằng Windows DPAPI, chỉ dùng trên Windows.' }
$null = [System.Net.Mail.MailAddress]::new($Sender)
if ([string]::IsNullOrWhiteSpace($Username)) { $Username = $Sender }
if ([string]::IsNullOrWhiteSpace($SmtpHost)) { throw 'Cần có địa chỉ máy chủ SMTP.' }
Write-Host 'Gmail: nhập MẬT KHẨU ỨNG DỤNG tạo riêng cho GS, không nhập mật khẩu đăng nhập Google.'
Write-Host 'Nội dung nhập không hiển thị và không đưa vào Git. Không dán mật khẩu vào chat.'
$smtpPassword = Read-Host 'Mật khẩu ứng dụng SMTP' -AsSecureString
if ($smtpPassword.Length -eq 0) { throw 'Chưa nhập mật khẩu. Chưa lưu cấu hình.' }
if ($SmtpHost -eq 'smtp.gmail.com') {
    $gmailPassword = [System.Net.NetworkCredential]::new('', $smtpPassword).Password -replace '\s', ''
    if ($gmailPassword.Length -ne 16) {
        $gmailPassword = $null
        throw 'Gmail cần mật khẩu ứng dụng 16 ký tự do Google tạo. Không dùng mật khẩu đăng nhập Google. Chưa lưu thay đổi.'
    }
    $smtpPassword = ConvertTo-SecureString -String $gmailPassword -AsPlainText -Force
    $gmailPassword = $null
}
$smtpCredential = [System.Management.Automation.PSCredential]::new($Username, $smtpPassword)
$smtpDirectory = Join-Path $projectRoot '.local'
New-Item -ItemType Directory -Force $smtpDirectory | Out-Null
$smtpCredential | Export-Clixml -LiteralPath (Join-Path $smtpDirectory 'smtp-credential.xml')
@{ Host = $SmtpHost; Port = $Port; From = $Sender; UseStartTls = !$SslOnConnect.IsPresent } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $smtpDirectory 'smtp.json') -Encoding UTF8
$smtpCredential = $null
$smtpPassword = $null
Write-Host 'Đã lưu cấu hình và mật khẩu mã hóa cho tài khoản Windows này. Chưa gửi thư. Khởi động lại ứng dụng để áp dụng.'
