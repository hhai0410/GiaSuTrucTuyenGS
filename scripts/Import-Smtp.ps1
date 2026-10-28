# Nạp cấu hình riêng trên máy; không đưa thông tin bí mật vào tham số dòng lệnh.
$smtpSettingsFile = Join-Path $projectRoot '.local/smtp.json'
$smtpCredentialFile = Join-Path $projectRoot '.local/smtp-credential.xml'
$smtpEnvironmentBackup = @{}
if ((Test-Path -LiteralPath $smtpSettingsFile) -and (Test-Path -LiteralPath $smtpCredentialFile)) {
    if ([Environment]::OSVersion.Platform -ne 'Win32NT') {
        throw 'Cấu hình SMTP mã hóa này dành cho Windows. Máy khác dùng biến môi trường hoặc User Secrets.'
    }
    $smtpSettings = Get-Content -LiteralPath $smtpSettingsFile -Raw | ConvertFrom-Json
    $smtpCredential = Import-Clixml -LiteralPath $smtpCredentialFile
    if ($smtpCredential -isnot [System.Management.Automation.PSCredential]) { throw 'Tệp SMTP không chứa thông tin đăng nhập hợp lệ.' }
    $smtpValues = @{
        Email__Mode = 'Smtp'; Email__Host = [string]$smtpSettings.Host
        Email__Port = [string]$smtpSettings.Port; Email__From = [string]$smtpSettings.From
        Email__Username = $smtpCredential.UserName
        Email__Password = $smtpCredential.GetNetworkCredential().Password
        Email__UseStartTls = ([bool]$smtpSettings.UseStartTls).ToString()
    }
    foreach ($smtpKey in $smtpValues.Keys) {
        $smtpEnvironmentBackup[$smtpKey] = [Environment]::GetEnvironmentVariable($smtpKey, 'Process')
        [Environment]::SetEnvironmentVariable($smtpKey, $smtpValues[$smtpKey], 'Process')
    }
    $smtpValues = $null
    $smtpCredential = $null
}
function Restore-SmtpEnvironment {
    foreach ($smtpKey in $smtpEnvironmentBackup.Keys) {
        [Environment]::SetEnvironmentVariable($smtpKey, $smtpEnvironmentBackup[$smtpKey], 'Process')
    }
}
