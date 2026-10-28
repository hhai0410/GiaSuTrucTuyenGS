# Cấu trúc dự án GS

## Nguyên tắc

Một repository, một ứng dụng ASP.NET Core MVC, chia trách nhiệm theo tầng bên trong. Đây là khung tổ chức dự kiến dựa trên backlog; không chứa mã chức năng. Tên thư mục dùng tiếng Anh để thuận tiện khi lập trình, giải thích dùng tiếng Việt.

## Vai trò từng file quản lý

| File | Vai trò |
| --- | --- |
| `README.md` | Giới thiệu và điểm bắt đầu đọc dự án |
| `CONTRIBUTING.md` | Quy ước Git, review và phân chia trách nhiệm đề xuất |
| `.gitignore` | Loại tệp build, cấu hình cục bộ, bí mật và dữ liệu tải lên khỏi Git |
| `docs/README.md` | Phân biệt tài liệu gốc và tài liệu nhóm sẽ bổ sung |
| `docs/PROJECT_OVERVIEW.md` | Tóm tắt tình hình từ tài liệu, phạm vi và các điểm cần chốt |
| `docs/PROJECT_STRUCTURE.md` | Bản đồ toàn bộ khung thư mục này |
| Các `README.md` còn lại | Giải thích thư mục chứa nó; giúp Git giữ thư mục, thay cho `.gitkeep` |

## Vai trò từng thư mục

| Thư mục | Vai trò |
| --- | --- |
| `src/` | Chứa mã nguồn sản phẩm. Khung hiện tại dành cho một ứng dụng web MVC; chưa sinh project .NET. |
| `src/GiaSuOnline.Web/` | Ứng dụng ASP.NET Core MVC dự kiến: giao diện Razor và xử lý phía máy chủ cùng một project. Chưa có mã chạy được. |
| `src/GiaSuOnline.Web/Controllers/` | Nhận yêu cầu HTTP, kiểm tra đầu vào, gọi Services và trả View hoặc kết quả. Không đặt truy vấn CSDL hay toàn bộ nghiệp vụ ở controller. |
| `src/GiaSuOnline.Web/Models/` | Các thực thể dữ liệu và kiểu trạng thái nghiệp vụ: tài khoản, hồ sơ gia sư, môn học, khung giờ, booking, giao dịch và phòng học. Chỉ tạo khi triển khai story tương ứng. |
| `src/GiaSuOnline.Web/ViewModels/` | Dữ liệu dành riêng cho từng form/trang Razor; tách khỏi entity để tránh cho người dùng sửa các trường nội bộ như vai trò, trạng thái duyệt hoặc số tiền. |
| `src/GiaSuOnline.Web/Views/` | Các trang Razor của học viên, gia sư và khách. Khi viết code, tạo thư mục con theo controller; giao diện quản trị đặt trong Areas/Admin/Views. |
| `src/GiaSuOnline.Web/Views/Shared/` | Layout, thanh điều hướng, partial view và các phần giao diện dùng chung. |
| `src/GiaSuOnline.Web/Areas/Admin/Controllers/` | Controller khu vực quản trị, trước mắt phục vụ duyệt/từ chối hồ sơ (US04). Phải kiểm tra quyền Admin phía máy chủ; chia thư mục không tự tạo phân quyền. |
| `src/GiaSuOnline.Web/Areas/Admin/Views/` | Giao diện quản trị tương ứng với các controller trong cùng Area. |
| `src/GiaSuOnline.Web/Services/` | Xử lý nghiệp vụ như hồ sơ, tìm kiếm, lịch rảnh, đặt lịch, thanh toán và phòng học. Chống đặt trùng phải phối hợp với giao dịch/ràng buộc CSDL. |
| `src/GiaSuOnline.Web/Services/Interfaces/` | Hợp đồng dịch vụ cần thay thế hoặc giả lập trong kiểm thử, ví dụ email, thanh toán, video. Không bắt buộc tạo interface cho mọi lớp. |
| `src/GiaSuOnline.Web/Data/` | DbContext, cấu hình entity và truy cập EF Core. Không thêm một tầng Repository chung nếu chưa có nhu cầu rõ ràng. |
| `src/GiaSuOnline.Web/Data/Configurations/` | Cấu hình quan hệ bảng, khóa, chỉ mục và ràng buộc EF Core; bao gồm các ràng buộc phục vụ tính nhất quán khi đặt lịch. |
| `src/GiaSuOnline.Web/Data/Migrations/` | Lịch sử thay đổi schema do EF Core sinh khi đã có project và thiết kế dữ liệu. Commit migration cùng thay đổi entity; không sửa schema bằng SQL rời mà bỏ qua migration. |
| `src/GiaSuOnline.Web/Data/Seed/` | Khởi tạo vai trò Student/Tutor/Admin, danh mục và dữ liệu demo. Không ghi mật khẩu quản trị thật trong mã nguồn. |
| `src/GiaSuOnline.Web/Integrations/` | Kết nối hệ thống ngoài. Nhà cung cấp cụ thể được chọn trong task nghiên cứu của Sprint 3. |
| `src/GiaSuOnline.Web/Integrations/Email/` | Gửi email SMTP: xác nhận đăng ký, đặt lại mật khẩu và thông báo kết quả nghiệp vụ. |
| `src/GiaSuOnline.Web/Integrations/Payments/` | Tích hợp cổng thanh toán sandbox; xác thực chữ ký callback/IPN và chống xử lý trùng. VNPay/MoMo là phương án khảo sát, chưa phải quyết định. |
| `src/GiaSuOnline.Web/Integrations/Video/` | Tích hợp nhà cung cấp video sau khi thống nhất US14. Cần làm rõ yêu cầu nhúng và quyền truy cập phòng thực tế trước khi lựa chọn. |
| `src/GiaSuOnline.Web/Jobs/` | Tác vụ nền: hủy yêu cầu không phản hồi sau 24 giờ, ghi nhận buổi học hoàn thành. Nhắc lịch chỉ bổ sung khi US16 được đưa vào phạm vi. |
| `src/GiaSuOnline.Web/Options/` | Các lớp cấu hình có kiểu cho email, thanh toán, video. Giá trị bí mật lấy từ User Secrets hoặc môi trường triển khai. |
| `src/GiaSuOnline.Web/wwwroot/css/` | CSS công khai của giao diện. |
| `src/GiaSuOnline.Web/wwwroot/js/` | JavaScript công khai của giao diện; không đặt khóa bí mật hoặc coi kiểm tra phía trình duyệt là phân quyền. |
| `src/GiaSuOnline.Web/wwwroot/images/` | Logo, biểu tượng và ảnh tĩnh công khai thuộc ứng dụng. Không chứa chứng chỉ/hồ sơ tải lên của người dùng. |
| `storage/private-uploads/` | Vị trí dự kiến trên máy phát triển cho tệp người dùng tải lên cần kiểm soát quyền. Nằm ngoài wwwroot. Dữ liệu thực bị Git bỏ qua; môi trường triển khai cần cấu hình nơi lưu bền vững riêng. |
| `database/diagrams/` | ERD, sơ đồ quan hệ và mô tả bảng. Nguồn thay đổi schema thực tế nằm trong Data/Migrations. |
| `database/scripts/` | Script phụ trợ quản trị/kiểm tra dữ liệu khi cần, không thay thế migration và không chứa dữ liệu người dùng thật. |
| `database/sample-data/` | Dữ liệu mẫu đã ẩn danh hoặc giả lập, phục vụ demo và thử tìm kiếm với 500 hồ sơ. |
| `tests/GiaSuOnline.UnitTests/` | Dự kiến kiểm thử quy tắc nghiệp vụ độc lập: trạng thái booking, thời gian, tính hợp lệ. Chưa khởi tạo project kiểm thử. |
| `tests/GiaSuOnline.IntegrationTests/` | Dự kiến kiểm thử phối hợp web–Identity–EF Core–CSDL, đặc biệt đặt cùng slot đồng thời và callback thanh toán trùng. Cần CSDL riêng cho kiểm thử. |
| `tests/e2e/` | Dự kiến kịch bản tự động xuyên suốt trình duyệt: đăng ký → tìm → đặt → thanh toán → vào học. Công cụ chưa chọn; biên bản UAT lưu trong docs/testing. |
| `deploy/` | Tài liệu và cấu hình triển khai Kestrel/IIS, HTTPS, SQL Server, sao lưu và phục hồi khi bước triển khai bắt đầu. Chưa tạo cấu hình triển khai hoặc pipeline. |
| `docs/requirements/` | Đặc tả yêu cầu chi tiết, acceptance criteria và các quyết định phạm vi. Dùng mã USxx/TCxx nhất quán với workbook của nhóm. |
| `docs/design/` | Thiết kế kiến trúc, luồng nghiệp vụ, wireframe và quyết định kỹ thuật; ERD đặt tại database/diagrams. |
| `docs/scrum/` | Biên bản Daily/Planning/Review/Retrospective và minh chứng theo sprint. Workbook gốc tiếp tục quản lý backlog, không sao chép thành một backlog khác. |
| `docs/testing/` | Test plan, test case thủ công, kết quả kiểm thử và biên bản UAT; tệp thực thi kiểm thử đặt tại tests. |
| `docs/reports/` | Báo cáo bài tập lớn và slide do nhóm thực hiện; không nhầm với mẫu báo cáo có sẵn ở docs. |
| `docs/user-guide/` | Hướng dẫn sử dụng cho học viên, gia sư và quản trị viên khi chức năng đã có. |
| `docs/evidence/` | Ảnh demo, minh chứng kiểm thử, phát hành và làm việc nhóm. Không đưa bí mật hoặc dữ liệu cá nhân thật vào ảnh. |

Các thư mục cha trung gian như `tests/`, `database/`, `storage/`, `Areas/`, `Areas/Admin/`, `wwwroot/` gom các thư mục con cùng trách nhiệm như mô tả ở bảng trên.

## Các file sẽ xuất hiện khi bắt đầu code (chưa tạo)

| File dự kiến | Vai trò |
| --- | --- |
| `GiaSuOnline.sln` hoặc `.slnx` | Gom project web và các project kiểm thử; định dạng tùy SDK đã chọn |
| `src/GiaSuOnline.Web/GiaSuOnline.Web.csproj` | Khai báo SDK, framework và các gói của ứng dụng |
| `src/GiaSuOnline.Web/Program.cs` | Khởi tạo ứng dụng, đăng ký dịch vụ, middleware và route |
| `src/GiaSuOnline.Web/appsettings.json` | Cấu hình chung không chứa bí mật |
| `src/GiaSuOnline.Web/appsettings.Development.json` | Cấu hình phát triển không chứa bí mật |
| `src/GiaSuOnline.Web/Properties/launchSettings.json` | Profile chạy ứng dụng trên máy phát triển |
| `src/GiaSuOnline.Web/Data/ApplicationDbContext.cs` | Kết nối mô hình dữ liệu với EF Core/Identity |
| `src/GiaSuOnline.Web/Views/_ViewImports.cshtml` | Namespace và Tag Helpers dùng chung cho View |
| `src/GiaSuOnline.Web/Views/_ViewStart.cshtml` | Chọn layout mặc định |
| `src/GiaSuOnline.Web/Views/Shared/_Layout.cshtml` | Khung giao diện dùng chung |
| Các `.csproj` trong `tests/` | Khai báo project kiểm thử sau khi chọn bộ công cụ |

Không tạo file `.cs`, `.cshtml`, `.csproj`, solution rỗng hoặc cấu hình chạy giả ở bước tổ chức thư mục. Những file trên chỉ là hướng dẫn cho giai đoạn triển khai.
