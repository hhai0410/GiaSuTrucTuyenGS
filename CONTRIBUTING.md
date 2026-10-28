# Quy ước đóng góp đề xuất

Các quy ước dưới đây là đề xuất để nhóm thống nhất, không phải nội dung đã được nhóm phê duyệt.

1. Mỗi việc gắn với mã User Story/task trong workbook, ví dụ US03/T03.1.
2. Tạo nhánh riêng từ nhánh chính, ví dụ `feature/US03-tutor-profile`, `fix/US08-double-booking`, `docs/sprint-1-review`.
3. Mở Pull Request mô tả thay đổi, US liên quan và cách kiểm tra. Có ít nhất một thành viên khác review trước khi gộp.
4. Giữ controller ngắn; nghiệp vụ đặt trong Services; cấu hình CSDL trong Data; giao diện trong Views/ViewModels.
5. Migration đi cùng thay đổi dữ liệu; thống nhất trong nhóm trước khi sửa cùng bảng.
6. Không commit mật khẩu, connection string có thông tin thật, khóa SMTP/thanh toán, tệp người dùng hoặc dữ liệu cá nhân. Dùng User Secrets trên máy phát triển và biến môi trường khi triển khai.
7. Đối chiếu Definition of Done và acceptance criteria trong workbook trước khi đánh dấu story hoàn thành. Ghi minh chứng kiểm thử, demo, review trong docs.
8. Không đánh dấu các README khung là chức năng đã triển khai. Chỉ thêm thư viện, cấu hình CI và lệnh chạy sau khi đã chọn phiên bản/công cụ và xác minh chúng.
