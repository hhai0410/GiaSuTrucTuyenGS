# GS – Web đặt lịch học gia sư online

Dự án học phần Phát triển dự án Công nghệ thông tin, Nhóm 8.

Ứng dụng **ASP.NET Core MVC (.NET 10)**, Razor + Bootstrap, Identity, EF Core và SQL Server. Sprint 1 đã có mã cho tài khoản, hồ sơ gia sư, kiểm duyệt và trang công khai. Đặt lịch, thanh toán và phòng video triển khai ở sprint sau.

## Bắt đầu dành cho thành viên mới

1. Đọc [hướng dẫn chạy và thử Sprint 1](docs/user-guide/GETTING_STARTED.md).
2. Đọc [cách các phần liên kết với nhau](docs/design/HOW_IT_WORKS.md).
3. Đối chiếu [tình trạng từng task Sprint 1](docs/scrum/sprint-01/STATUS.md).

Máy hiện tại dùng `LOQ\SQLEXPRESS`, database riêng `GiaSuOnlineGS`, Windows Authentication. Email Development lưu thành tệp trong `src/GiaSuOnline.Web/App_Data/mail/`, chưa gửi email thật.

```powershell
# Tại thư mục gốc; lần đầu trên máy mới
.\scripts\Setup.ps1
# Chạy web
.\scripts\Start.ps1
# Kiểm thử
.\scripts\Test.ps1
```

Mở **https://localhost:7043**. Nếu PowerShell chặn script, dùng `powershell -ExecutionPolicy Bypass -File .\scripts\Start.ps1` cho lần chạy đó; không cần đổi chính sách toàn máy.

## Phạm vi đã kiểm tra

Có bộ kiểm thử tích hợp cho đăng ký, khóa tài khoản, reset token, phân quyền, CSRF, upload, duyệt/từ chối, 404 và sửa hồ sơ phải duyệt lại. Luồng lõi đã được thử thêm với SQL Server thật. Chi tiết và giới hạn nằm trong [báo cáo kiểm thử](docs/testing/SPRINT_1_RESULTS.md).

Chưa coi toàn Sprint là Done: SMTP thật, bảng Scrum trực tuyến, review PR/merge và kiểm tra trực quan trên thiết bị của nhóm vẫn cần hoàn tất. Không sửa trạng thái trong workbook gốc để tránh ghi nhận hoàn thành trước khi nhóm nghiệm thu.

## Tổ chức mã

- `src/GiaSuOnline.Web`: Controller → Service → EF Core/SQL Server; ViewModel → Razor → HTML/CSS/JS.
- `tests/GiaSuOnline.IntegrationTests`: kiểm thử HTTP và dữ liệu dùng SQLite cô lập, không xóa dữ liệu SQL Server.
- `database/diagrams`: ERD.
- `scripts`: chuẩn bị môi trường, khởi chạy và kiểm thử.
- `docs`: tài liệu nguồn và hướng dẫn dự án.

Không commit `.tools/`, `.local/`, `App_Data/`, khóa, mật khẩu hoặc connection string có bí mật. Tài liệu cấu trúc ban đầu trong `docs/PROJECT_STRUCTURE.md` được giữ để tham khảo lịch sử; các mô tả “chưa code” trong đó phản ánh thời điểm tạo khung, không phải trạng thái hiện tại.
