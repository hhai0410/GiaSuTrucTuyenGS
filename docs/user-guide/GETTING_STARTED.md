# Chạy và thử Sprint 1

## 1. Những gì đã có trên máy hiện tại

- Bộ công cụ .NET SDK 10.0.403 tại `.tools/dotnet/`, không cần cài lại toàn máy.
- Database `GiaSuOnlineGS` trên `LOQ\SQLEXPRESS`, kết nối bằng tài khoản Windows.
- Ba vai trò Student/Tutor/Admin và sáu môn học được tạo khi ứng dụng khởi động.
- Tài khoản quản trị thử lưu trong `.local/admin-login.txt` (mở tệp trên máy để xem; không đưa lên Git hoặc gửi vào chat).
- Có hai hồ sơ mang tên “Gia sư kiểm thử SQL” do kiểm chứng luồng, không phải người dùng thật.

SSMS chỉ là **công cụ xem/quản trị**; SQL Server Express mới là chương trình lưu dữ liệu. Việc dùng SSMS không tự cài .NET hoặc chạy web.

## 2. Chạy ứng dụng

Sau phiên triển khai, bản xem thử đang chạy nền nên bạn có thể mở địa chỉ web ngay. Trước khi tự khởi chạy hoặc biên dịch/kiểm thử, dừng bản nền bằng `.\scripts\Stop-Preview.ps1` để tránh trùng cổng hoặc khóa tệp. Script này chỉ dừng tiến trình xem thử được ghi nhận đúng đường dẫn dự án.

Mở terminal PowerShell tại thư mục gốc dự án:

```powershell
.\scripts\Start.ps1
```

Mở `https://localhost:7043` bằng Edge/Chrome trên máy. Giữ terminal mở; nhấn Ctrl+C để dừng. Nếu báo cổng đang dùng, có thể bản xem thử cũ đang chạy: kiểm tra ứng dụng đang mở, không khởi động nhiều bản cùng cổng.

Nếu Windows chặn file script, có thể chạy đúng một lần bằng:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Start.ps1
```

Chứng chỉ HTTPS phát triển đã được tin cậy trên máy Windows. Nếu trình duyệt vẫn cảnh báo, đóng/mở lại trình duyệt; không bỏ qua cảnh báo một cách máy móc. Có thể chạy `.tools/dotnet/dotnet.exe dev-certs https --trust` và tự xác nhận Windows. Trình duyệt bên trong Codex vẫn có thể chưa nhận kho chứng chỉ Windows; hãy dùng trình duyệt trên máy.

## 3. Thử từ góc nhìn gia sư

1. Chọn Tạo tài khoản → Gia sư. Dùng email thử, mật khẩu từ 8 ký tự, nhập lại giống nhau.
2. Mở thư mục `src/GiaSuOnline.Web/App_Data/mail/`. Sắp theo thời gian sửa mới nhất, mở tệp `.txt` mới nhất, kiểm tra đúng người nhận rồi mở liên kết xác nhận. Các email cũ trước khi bổ sung bản văn bản có định dạng `.eml`, mở bằng ứng dụng email.
3. Đăng nhập. Chọn Tạo hồ sơ.
4. Điền giới thiệu tối thiểu 20 ký tự, học phí dương, kinh nghiệm 0–70 năm; chọn ít nhất một môn (giữ Ctrl để chọn nhiều).
5. Chọn ảnh JPG/PNG và chứng chỉ JPG/PNG/PDF thực, mỗi tệp tối đa 5 MB. Ảnh sẽ được xem trước.
6. Gửi duyệt. Trang Hồ sơ của tôi hiển thị Chờ duyệt. Chưa có trang công khai cho hồ sơ này.

Email hiện **chỉ lưu trên máy**, không đến hộp Gmail/Outlook thật. Các tệp thư có token nhạy cảm, chỉ dùng để thử, đã bị Git bỏ qua.

## 4. Thử quản trị và khách

1. Dùng cửa sổ riêng tư hoặc trình duyệt khác để tránh trộn phiên gia sư/quản trị.
2. Đăng nhập thông tin trong `.local/admin-login.txt`.
3. Vào Kiểm duyệt, lọc Chờ duyệt, mở hồ sơ và tải chứng chỉ để xem.
4. Từ chối cần lý do; kết quả hiển thị ở trang cá nhân gia sư và email thử được tạo. Có thể bấm Gửi lại email kết quả khi gặp lỗi gửi.
5. Duyệt hồ sơ chờ duyệt hợp lệ. Khách chưa đăng nhập có thể thấy hồ sơ ở trang chủ và mở chi tiết.
6. Gia sư sửa hồ sơ → hồ sơ trở lại Chờ duyệt và biến mất khỏi trang công khai.

Nút Đặt lịch mở trang khung giờ. Chưa tạo booking ở Sprint 1. Khi chưa có lịch/đánh giá, hệ thống hiển thị trạng thái chưa có dữ liệu; không tự bịa đánh giá.

## 5. Thử tài khoản và bảo mật

- Nhập sai mật khẩu 5 lần cho tài khoản đã xác nhận → khóa 15 phút.
- Quên mật khẩu → đọc email thử → mở link trong 30 phút → thay mật khẩu. Link dùng lại bị từ chối.
- Học viên truy cập `/Admin/Profiles` hoặc `/TutorProfiles/Edit` → 403.
- Hồ sơ không tồn tại hoặc chưa duyệt → 404.
- Đăng xuất dùng nút trên web; chỉ truy cập URL Logout bằng GET không thực hiện đăng xuất.

## 6. Chuẩn bị trên máy thành viên khác

1. Clone repository (khi mã đã được nhóm đưa lên GitHub), cài SQL Server Express và SSMS nếu chưa có.
2. Sửa tên máy chủ `LOQ\SQLEXPRESS` trong `src/GiaSuOnline.Web/appsettings.json` cho đúng máy đó. Windows Authentication cần tài khoản có quyền tạo database.
3. Chạy `.\scripts\Setup.ps1`: tải SDK nếu cần, thư viện, build, áp dụng migration và tin cậy chứng chỉ cục bộ. Cần Internet ở bước tải thư viện.
4. Lần đầu tạo admin trên máy mới: `.\scripts\Start.ps1 -CreateAdmin`. Nhập email mới và mật khẩu khi được hỏi. Không dùng email tài khoản tự đăng ký đã tồn tại; hệ thống không tự nâng quyền email đó.
5. Chạy `.\scripts\Test.ps1` để kiểm tra.

Nếu chỉ cần học thử mà chưa có SQL Server: `.\scripts\Start.ps1 -Sqlite -CreateAdmin`. SQLite dùng tệp riêng `App_Data/gs-dev.db`; dữ liệu không tự đồng bộ sang SQL Server. Migration đã tạo dành cho SQL Server; SQLite demo dùng EnsureCreated, không dùng migration SQL Server.

## 7. Gửi email thật khi nhóm sẵn sàng

Dừng web. Trong PowerShell đặt cấu hình cho phiên chạy hiện tại; không sửa mật khẩu vào file đã commit:

```powershell
$env:Email__Mode = 'Smtp'
$env:Email__Host = 'ten-may-chu-do-nha-cung-cap-cap'
$env:Email__Port = '587'
$env:Email__From = 'email-gui-cua-nhom'
$env:Email__Username = 'tai-khoan-SMTP'
$smtpSecret = Read-Host 'Mat khau SMTP / mat khau ung dung' -AsSecureString
$env:Email__Password = [System.Net.NetworkCredential]::new('', $smtpSecret).Password
.\scripts\Start.ps1
```

Điền thông tin thật theo nhà cung cấp; STARTTLS bắt buộc mặc định, không dùng mật khẩu đăng nhập email thường nếu nhà cung cấp yêu cầu mật khẩu ứng dụng. Sau khi dừng, xóa biến `Email__Password` khỏi phiên hoặc đóng terminal. Nhóm chưa có nhà cung cấp nên việc gửi email thật chưa được nghiệm thu.

## 8. Xem dữ liệu trong SSMS

Kết nối `LOQ\SQLEXPRESS` → Databases → GiaSuOnlineGS → Tables. Nếu chưa hiện, Refresh.

- AspNetUsers: email, mật khẩu **đã băm**, trạng thái xác nhận/khóa.
- AspNetRoles/AspNetUserRoles: vai trò và gán vai trò.
- TutorProfiles: nội dung, học phí, trạng thái (0 chờ, 1 duyệt, 2 từ chối).
- Subjects/TutorSubjects: môn học và quan hệ môn dạy.
- __EFMigrationsHistory: phiên bản cấu trúc đã áp dụng.

Không sửa PasswordHash hoặc quyền trực tiếp trong SSMS để thử đăng nhập. Không xóa database để xử lý một lỗi giao diện.
