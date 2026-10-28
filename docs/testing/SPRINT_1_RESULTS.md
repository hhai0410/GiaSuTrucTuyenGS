# Kết quả kiểm thử Sprint 1

Ngày thực hiện: 29/10/2026 (UTC+7).

## Bổ sung cấu hình SMTP (29/10/2026)

- Bộ kiểm thử mở rộng: **21/21 đạt**, gồm 14 trường hợp lõi và 7 trường hợp cấu hình email. TRX: `tests/GiaSuOnline.IntegrationTests/TestResults/sprint1-smtp.trx` (không commit).
- Bổ sung kiểm tra địa chỉ gửi, host/port, mật khẩu bắt buộc, TLS, che mật khẩu trong chuỗi chẩn đoán, không dùng Pickup ngoài Development và từ chối tên mode sai trước khi kết nối.
- Windows PowerShell 5.1: script UTF-8 có BOM, không có lỗi parse; đã thử lưu/đọc credential DPAPI và khôi phục biến môi trường bằng credential giả.
- Kết nối Gmail 587 STARTTLS và 465 SSL/TLS đã thực hiện được, không bỏ kiểm tra chứng chỉ.
- Gmail thật đã đạt sau khi người dùng tạo mật khẩu ứng dụng hợp lệ. Gmail chấp nhận thư thử và người nhận xác nhận đã nhận. Lỗi 5.7.9 ban đầu đã được xử lý, không còn là blocker.
- Công cụ Test-Smtp dùng EmailSender thật và chỉ chạy khi người dùng cung cấp người nhận; không mở endpoint gửi thư trên web.
- Toàn bộ tiến độ hiện tại và các task còn chờ được theo dõi trong [README chính](../../README.md#tien-do). Nội dung 14 test phía dưới là bằng chứng của lượt triển khai trước.

## Nghiệm thu email nghiệp vụ thật

Người dùng đã cho phép tạo riêng một tài khoản Tutor thử bằng Gmail alias của hộp nhận để không đổi tài khoản GS hiện có. Toàn bộ thao tác chạy trên SQL Server của dự án và SMTP Gmail thật.

| Kiểm chứng | Kết quả |
| --- | --- |
| Email xác nhận đăng ký | Người nhận đã mở link và thấy xác nhận thành công |
| Đăng nhập sau xác nhận | HTTP thực đăng nhập thành công |
| Upload hồ sơ, quyền hiển thị | Lưu được; khách nhận 404 khi Pending |
| Admin duyệt | Cập nhật thành công; khách nhận 200 khi Approved |
| Email kết quả duyệt | Người nhận xác nhận đã nhận |
| Email đặt lại mật khẩu | Người nhận mở link, đặt mật khẩu mới và xác nhận đăng nhập thành công |

Mật khẩu và token không được ghi trong bằng chứng. Tệp [kết quả đã loại thông tin nhạy cảm](../evidence/sprint-01/smtp-check.json) lưu các trạng thái kiểm chứng, phân biệt phần HTTP tự động với phần người dùng xác nhận. Các email này dùng chính AccountController/ProfilesController và EmailSender của ứng dụng, không chỉ gửi một thư độc lập.

## Kiểm chứng đã thực hiện

- Build dự án web và project kiểm thử thành công trên .NET SDK 10.0.403. Lượt cuối: 14/14 test đạt, 0 thất bại, không có test bỏ qua.
- SQL Server: migration InitialSprintOne đã áp dụng thành công tại LOQ\SQLEXPRESS / GiaSuOnlineGS.
- Luồng HTTP thật với SQL Server và HTTPS đã đạt: đăng ký Tutor → email Pickup → xác nhận → đăng nhập → tải ảnh/chứng chỉ → khách xem hồ sơ chờ duyệt nhận 404 → admin duyệt → khách mở chi tiết/ảnh/trang khung giờ nhận 200.
- Ba vai trò và danh mục môn học đã được seed; tài khoản quản trị local được tạo bằng mật khẩu ngẫu nhiên lưu trong .local, không chứa trong mã.
- `dotnet list ... package --vulnerable --include-transitive --no-restore`: không có gói dễ tổn thương theo nguồn NuGet được kiểm tra lúc chạy.

## Bộ kiểm thử tự động

| Trường hợp | Kết quả |
| --- | --- |
| RegistrationRejectsInvalidInputs(role: "Student", password: "abcdefgh", email: "invalid-email") | Passed |
| InvalidUploadsRejected(name: "photo.exe", oversized: False) | Passed |
| ExpiredResetTokenRejected | Passed |
| InvalidUploadsRejected(name: "photo.png", oversized: True) | Passed |
| LockoutAfterFiveFailuresForFifteenMinutes | Passed |
| SecurityBlocksCsrfRoleEscalationAndRedirectsHttp | Passed |
| RegistrationRejectsInvalidInputs(role: "Tutor", password: "short", email: "valid@example.test") | Passed |
| ProfileApprovalRejectionEditingPrivacyAndXssEndToEnd | Passed |
| RegistrationHashesPasswordSeedsRolesAndSendsConfirmation | Passed |
| RegistrationRejectsInvalidInputs(role: "Admin", password: "abcdefgh", email: "valid@example.test") | Passed |
| ResetTokenSingleUseAndInvalidatesOldPassword | Passed |
| InvalidPriceRejectedAndSqlInputDoesNotChangeSchema | Passed |
| InvalidUploadsRejected(name: "photo.jpg", oversized: False) | Passed |
| LogoutRevokesCopiedCookieAndPostRequired | Passed |

Kiểm thử tích hợp dùng SQLite in-memory riêng cho từng test, HTTP qua TestServer và dịch vụ email giả lập. Chúng không xóa database SQL Server của nhóm. Lượt SQL Server thật là kiểm chứng bổ sung, không có nghĩa mọi ca lỗi đã được chạy trên cả hai provider.

Kết quả máy đọc TRX lưu tại `tests/GiaSuOnline.IntegrationTests/TestResults/sprint1-final.trx` (Git bỏ qua); mã test được lưu cùng dự án.

## Kịch bản để tái hiện và nghiệm thu bổ sung

| Mã | Thao tác | Kỳ vọng |
| --- | --- | --- |
| MAN01 | Đăng ký Tutor, xác nhận email thật sau khi bật SMTP | Email đến hộp thư; xác nhận xong đăng nhập được |
| MAN02 | Nhập sai mật khẩu 5 lần, thử lại sau 15 phút | Khóa tạm; hết thời gian được thử lại |
| MAN03 | Yêu cầu reset rồi chờ quá 30 phút | Link hết hạn; yêu cầu link mới được |
| MAN04 | Form hồ sơ ở điện thoại và desktop, dùng bàn phím | Không tràn; stepper, lỗi và ảnh xem trước rõ ràng |
| MAN05 | Tệp thực JPG/PNG/PDF; sai đuôi, giả mạo, quá 5 MB | Tệp hợp lệ được lưu; trường hợp sai bị từ chối |
| MAN06 | Mở chứng chỉ bằng tài khoản khác | Chủ hồ sơ/Admin được tải; người khác bị chặn |
| MAN07 | Admin mở hồ sơ, Tutor sửa ở tab khác, Admin duyệt phiên bản cũ | Nhận xung đột; không duyệt nhầm nội dung mới |
| MAN08 | Từ chối thiếu lý do rồi từ chối hợp lệ, gửi lại và duyệt | Thiếu lý do bị chặn; trạng thái/email đúng |
| MAN09 | Sửa hồ sơ đã duyệt | Pending trở lại; khách nhận 404 |
| MAN10 | HTTP→HTTPS trên trình duyệt thực; thử Production tại môi trường phù hợp | Redirect HTTPS; HSTS trên host triển khai, cookie an toàn |

## Giới hạn và việc chưa xác nhận

- SMTP Gmail đã được kiểm chứng gửi/nhận thật; Pickup vẫn là lựa chọn để thử offline.
- Trình duyệt trong Codex báo ERR_CERT_AUTHORITY_INVALID dù chứng chỉ đã được Windows trust. Không bỏ qua cảnh báo; chưa kiểm tra trực quan bằng trình duyệt tự động. HTTP tích hợp và kiểm chứng SQL thật đã chạy với xác thực chứng chỉ của Windows/Python.
- Chưa có kiểm thử trình duyệt tự động cho stepper, preview ảnh hoặc kích thước màn hình. Cần MAN04 trên Edge/Chrome của nhóm.
- HSTS bật ngoài Development theo thông lệ ASP.NET Core, chưa triển khai host Production để đo header tại đó.
- Chưa có PR được thành viên khác review/merge hoặc buổi nghiệm thu; không gán nhãn toàn Sprint Done.
- Các phép thử bảo mật là kiểm tra cơ bản, không phải kiểm toán bảo mật hay kiểm thử thâm nhập đầy đủ.
