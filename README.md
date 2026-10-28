# GS – Web đặt lịch học gia sư online

Dự án học phần **Phát triển dự án Công nghệ thông tin**, Nhóm 8.

## Trạng thái hiện tại

Repository mới có tài liệu học phần và khung thư mục. **Chưa lập trình chức năng, chưa khởi tạo project .NET, chưa cài thư viện và chưa có ứng dụng để chạy.**

Định hướng từ backlog: ASP.NET Core MVC, Razor + Bootstrap, ASP.NET Core Identity, EF Core và SQL Server. Phiên bản .NET và thư viện sẽ được thống nhất khi bắt đầu triển khai.

## Bắt đầu đọc

- [Cấu trúc và vai trò từng thư mục/file](docs/PROJECT_STRUCTURE.md).
- [Tóm tắt tài liệu, phạm vi và điểm cần thống nhất](docs/PROJECT_OVERVIEW.md).
- [Quy ước đóng góp đề xuất](CONTRIBUTING.md).
- [Danh mục tài liệu](docs/README.md).

## Các phần chính

| Đường dẫn | Vai trò |
| --- | --- |
| `src/GiaSuOnline.Web/` | Ứng dụng MVC: giao diện, nghiệp vụ, dữ liệu và kết nối ngoài |
| `tests/` | Kiểm thử đơn vị, tích hợp và luồng trình duyệt |
| `database/` | ERD, script phụ trợ, dữ liệu demo |
| `storage/private-uploads/` | Nơi dự kiến lưu tệp riêng tư trên máy phát triển |
| `deploy/` | Hướng dẫn và cấu hình triển khai tương lai |
| `docs/` | Tài liệu gốc, yêu cầu, Scrum, thiết kế, kiểm thử và báo cáo |

Giữ một ứng dụng chung giúp nhóm 5 người phân công theo Controller/Service, View/UI, dữ liệu và kiểm thử như kế hoạch hiện có. Chưa cần tách frontend thành một ứng dụng riêng hoặc chia microservice.
