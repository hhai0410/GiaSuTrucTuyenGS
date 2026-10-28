# Đối chiếu Sprint Backlog 1

Nguồn: `docs/Nhom8_Scrum_GiaSuOnline.xlsx`, sheet `Sprint backlog 1`. Đọc 29 task thuộc US01, US02, US03, US19, US04, US06; tổng 124 giờ dự kiến theo workbook. Không ghi giờ thực tế giả và không sửa workbook gốc.

**Sprint chưa thể tuyên bố Done toàn bộ.** “Đã triển khai” ở đây mô tả mã đã có; “đã kiểm thử” chỉ các kiểm chứng nêu cụ thể, không thay review/acceptance của nhóm.

| Task | Công việc | Trạng thái thực tế | Bằng chứng / giới hạn |
| --- | --- | --- | --- |
| T01.1 | Thiết kế CSDL người dùng | Đã triển khai, đã áp dụng SQL Server | Identity, unique email; migration; seed Student/Tutor/Admin; ERD |
| T01.2 | Xử lý đăng ký (Controller/Service) | Đã triển khai; SMTP thật chờ cấu hình | AccountController, Identity, EmailSender; đã thử email Pickup |
| T01.3 | Giao diện đăng ký | Đã triển khai; chờ nhóm xem giao diện | Views/Account/Register.cshtml, Bootstrap, validation server và HTML |
| T01.4 | Kiểm thử đăng ký | Đã kiểm thử tự động | RegistrationHashesPasswordSeedsRolesAndSendsConfirmation; RegistrationRejectsInvalidInputs |
| T01.5 | Review code & merge | Chưa hoàn tất quy trình | Build đạt; chưa có PR, review thành viên khác hoặc merge |
| T02.1 | Đăng nhập / đăng xuất | Đã triển khai và kiểm thử | AccountController; khóa 5 lần/15 phút; logout đổi security stamp |
| T02.2 | Quên / đặt lại mật khẩu | Đã triển khai và kiểm thử; SMTP thật chờ cấu hình | Identity token 30 phút; token đã dùng/hết hạn bị từ chối |
| T02.3 | Giao diện đăng nhập & quên mật khẩu | Đã triển khai; chờ nhóm xem giao diện | Login, EmailForm, ResetPassword, Message |
| T02.4 | Kiểm thử đăng nhập | Đã kiểm thử tự động | LockoutAfterFiveFailuresForFifteenMinutes; ResetTokenSingleUseAndInvalidatesOldPassword; ExpiredResetTokenRejected |
| T02.5 | Review code & merge | Chưa hoàn tất quy trình | Chưa tạo develop/feature hoặc merge; giữ nguyên nhánh theo quyết định tại phiên làm việc |
| T03.1 | Thiết kế CSDL hồ sơ gia sư | Đã triển khai, đã áp dụng SQL Server | TutorProfile, TutorSubject, Subject; migration; seed 6 môn; ERD |
| T03.2 | CRUD hồ sơ gia sư | Đã triển khai và kiểm thử | ProfileService; form chỉ nhận dữ liệu cho phép; sửa luôn Pending; concurrency token |
| T03.3 | Upload ảnh và chứng chỉ | Đã triển khai và kiểm thử | UploadStore: JPG/PNG/PDF, kiểm tra signature, tối đa 5 MB/tệp, tên ngẫu nhiên, App_Data ngoài wwwroot |
| T03.4 | Giao diện form hồ sơ nhiều bước | Đã triển khai; chờ nhóm thử trực quan | Stepper 3 bước, kiểm tra client/server, ảnh xem trước; cần kiểm tra desktop/mobile thật |
| T03.5 | Trang xem hồ sơ cá nhân | Đã triển khai và kiểm thử HTTP | Mine: thông tin, trạng thái, lý do từ chối, liên kết tệp có quyền |
| T03.6 | Kiểm thử hồ sơ gia sư | Đã kiểm thử tự động | InvalidUploadsRejected; InvalidPriceRejectedAndSqlInputDoesNotChangeSchema; ProfileApprovalRejectionEditingPrivacyAndXssEndToEnd |
| T19.1 | Thiết lập repo Git, quy ước nhánh, bảng công việc | Hoàn thành một phần | Có repo main, CONTRIBUTING và mẫu PR; chưa tạo nhánh (yêu cầu bị từ chối); chưa có bảng Trello/Jira |
| T19.2 | Cấu hình HTTPS/HSTS | Đã cấu hình; HTTPS cục bộ đã chạy | Chứng chỉ .NET đã trust; HTTP→HTTPS kiểm thử; HSTS bật ngoài Development, cần kiểm tra tại môi trường triển khai |
| T19.3 | Chống CSRF/XSS/SQL Injection | Đã triển khai và kiểm thử cơ bản | AutoValidateAntiforgeryToken, Razor encode, EF Core query tham số, CSP, cookie Secure/HttpOnly |
| T19.4 | Phân quyền theo vai trò | Đã triển khai và kiểm thử | Authorize theo role; kiểm tra chủ sở hữu tệp; trang 403/404 tùy chỉnh |
| T19.5 | Kiểm thử bảo mật cơ bản | Đã kiểm thử tự động cơ bản | CSRF→400, sai role→403, HTTP→308, script bị encode, SQL input không thay đổi schema; không thay thế kiểm thử bảo mật đầy đủ |
| T04.1 | Trạng thái hồ sơ gia sư | Đã triển khai, đã áp dụng SQL Server | ProfileStatus Pending/Approved/Rejected, RejectionReason và Revision |
| T04.2 | Trang admin danh sách chờ duyệt | Đã triển khai; chờ nhóm xem giao diện | Admin/Profiles: lọc trạng thái, danh sách, chi tiết và tải chứng chỉ |
| T04.3 | Chức năng duyệt / từ chối | Đã triển khai và kiểm thử | Moderate kiểm tra Pending/Revision, từ chối bắt buộc lý do; chống duyệt hồ sơ đã đổi |
| T04.4 | Email kết quả duyệt | Đã triển khai và thử Pickup; SMTP thật chưa thử | EmailSender MailKit dùng chung, báo lỗi SMTP rõ ràng và có nút gửi lại |
| T04.5 | Kiểm thử duyệt hồ sơ | Đã kiểm thử tự động và thử SQL Server | Luồng pending→reject→resubmit→approve; pending không công khai |
| T06.1 | Giao diện trang chi tiết gia sư | Đã triển khai; phần booking giới hạn Sprint 1 | Trang chi tiết và nút Đặt lịch dẫn tới trang khung giờ; chưa tạo booking Sprint 2 |
| T06.2 | Dữ liệu đánh giá và lịch rảnh tóm tắt | Đã triển khai phần đọc; chưa có chức năng tạo dữ liệu | Truy vấn 5 đánh giá gần nhất và slot trống 7 ngày tới; trạng thái rỗng trung thực; tạo lịch/đánh giá theo sprint sau |
| T06.3 | Kiểm thử trang chi tiết | Đã kiểm thử tự động và thử SQL Server | Hồ sơ đã duyệt→200, chưa duyệt/ID không tồn tại→404, nút dẫn trang khung giờ |

## Phần nhóm cần hoàn tất

1. Chọn/tạo email SMTP; cấu hình bí mật tại máy rồi chứng minh email xác nhận, reset và kết quả duyệt đến hộp thư thật.
2. Tạo bảng Scrum Trello/Jira bằng tài khoản của nhóm; dùng đúng các mã task ở bảng trên, có cột To do / Doing / Review / Done. Bảng Markdown này là danh sách bàn giao, không thay bảng trực tuyến.
3. Chủ tài khoản tự đăng nhập lại GitHub (`gh auth login`); CLI hiện báo token không hợp lệ. Chưa push/commit/PR/merge. Việc tạo nhánh develop/codex/sprint-1 đã bị từ chối nên nhánh hiện tại giữ nguyên.
4. Thành viên khác review mã, thử giao diện máy tính/điện thoại và thử kịch bản theo docs/testing/SPRINT_1_RESULTS.md.
5. PO quyết định cách nghiệm thu US06 khi lịch/đánh giá chưa có chức năng nhập. Không tạo dữ liệu/đánh giá người dùng giả để lấp chỗ trống.
6. Họp Sprint Review/Retrospective thực, thu phản hồi, cập nhật workbook sau khi nhóm xác nhận. Không tạo biên bản họp hoặc chữ ký thay nhóm.

## Lưu ý workbook

Hàng ngày trong Sprint backlog 1 có các ô công thức `#REF!` khi đọc cached values. Chưa sửa công thức trong lần triển khai này; nhóm cần kiểm tra liên kết ngày/nguồn dữ liệu trước khi dùng burndown làm minh chứng.
