# Tình hình dự án từ tài liệu hiện có

## Nguồn và cách hiểu

- `8-KeHoachDA_PhieuDeXuat.docx`: đề xuất của Nhóm 8, đề tài GS, mục tiêu Agile/Scrum và các sản phẩm cần nộp.
- `Nhom8_Scrum_GiaSuOnline.xlsx`: nguồn kế hoạch cụ thể cho dự án; gồm vai trò, release, story mapping, Product Backlog, Sprint backlog, US_Task, Review, Retrospective, rủi ro và kế hoạch nhóm.
- Các PDF học phần, biểu mẫu Word và workbook template hướng dẫn quy trình, báo cáo và minh chứng. Nội dung ví dụ trong mẫu không tự trở thành yêu cầu sản phẩm.
- `US-TASK_SprintGoal.xlsx` có ví dụ giao đồ ăn/nhà hàng; không đưa các chức năng đó vào dự án GS.

Đây là bản đọc sơ bộ phục vụ tổ chức repository, không phải đặc tả đã được nhóm duyệt. Các trạng thái trong workbook là thông tin kế hoạch, chưa chứng minh tiến độ thực tế. Tài liệu gốc được giữ nguyên vị trí và nội dung.

## Phạm vi và lộ trình đã ghi trong workbook

Ba nhóm người dùng chính: học viên (Student), gia sư (Tutor), quản trị viên (Admin); khách có thể xem hồ sơ đã duyệt.

| Giai đoạn | Thời gian kế hoạch | Story và kết quả |
| --- | --- | --- |
| Sprint 1 | 26/10–13/11/2026 | US01, US02, US03, US04, US06, US19: tài khoản, hồ sơ, duyệt, xem chi tiết và bảo mật |
| Sprint 2 | 16/11–04/12/2026 | US05, US07, US08, US09: tìm kiếm, lịch rảnh, đặt lịch và xác nhận; phát hành V1.0 |
| Sprint 3 | 07/12–25/12/2026 | US12, US14, US20: thanh toán sandbox, phòng học video, kiểm thử/UAT và triển khai; phát hành V2.0 |

US10, US11, US13, US15, US16, US17, US18 thuộc mở rộng, chưa cam kết. Các thư mục khung không đồng nghĩa mở rộng phạm vi sang những story này.

## Cơ sở lựa chọn cấu trúc

Task hiện có nhắc EF Core, ASP.NET Core Identity, form Razor + Bootstrap, Kestrel/IIS và SQL Server. Vì vậy cấu trúc dùng một ứng dụng ASP.NET Core MVC, tách Controller, Service, Model, ViewModel, View và Data. Đây là lựa chọn tổ chức phù hợp với các task, chưa phải việc cài đặt công nghệ.

Workbook phân công SV1 phụ trách PO + QA/Test, SV2 SM + DevOps/tài liệu, SV3 backend, SV4 giao diện, SV5 CSDL + backend/job. Cấu trúc tạo các khu vực tương ứng để phối hợp, không hạn chế quyền sửa file theo người.

## Điểm cần thống nhất trước khi triển khai

1. Phiên bản .NET/SDK, công cụ kiểm thử và môi trường SQL Server chưa được chốt trong bước này.
2. Cổng thanh toán chưa chọn: task yêu cầu khảo sát VNPay/MoMo sandbox. Chưa tích hợp hoặc cam kết một nhà cung cấp.
3. Đề xuất nói quản lý link Meet/Zoom, trong khi US14 yêu cầu phòng video, quyền vào phòng theo booking và task khảo sát Jitsi nhúng. Cần thống nhất yêu cầu cuối; bảo vệ trang chứa link không đủ chứng minh người ngoài không vào được phòng của nhà cung cấp.
4. US06/US05 có nội dung đánh giá dù US15 ở mở rộng; cần chốt cách hiển thị/lọc khi chưa có tính năng đánh giá. US20 nhắc luồng qua đánh giá nhưng US15 chưa cam kết: UAT phải bám phạm vi được PO thống nhất.
5. Email đăng ký, khôi phục mật khẩu và thông báo trạng thái nằm trong các story lõi; US16 mở rộng là thông báo/nhắc lịch đầy đủ. Không trì hoãn email bắt buộc chỉ vì US16 chưa được làm.

## Các lưu ý cho thiết kế sau này

- Đặt lịch phải chống tranh chấp tại CSDL, không chỉ kiểm tra slot trên giao diện.
- Thanh toán chỉ sau khi gia sư xác nhận; kiểm tra chữ ký và xử lý callback đúng một lần.
- Chứng chỉ/tệp riêng tư cần kiểm soát quyền và không đặt trong wwwroot.
- Bảo mật, kiểm thử và minh chứng Scrum được thực hiện theo từng sprint.
- Báo cáo, slide, BM01/BM02/BM03 và minh chứng Planning/Review/Retrospective là sản phẩm học phần bên cạnh ứng dụng.
