# GS – Web đặt lịch học gia sư online

Dự án học phần **Phát triển dự án Công nghệ thông tin**, Nhóm 8. GS kết nối học viên với gia sư: tài khoản/hồ sơ/kiểm duyệt (Sprint 1), tìm kiếm/lịch rảnh/đặt lịch/xác nhận (Sprint 2). Thanh toán và học video thuộc Sprint 3.

**Cập nhật: 15/11/2026 (UTC+7). Sprint 1 và chức năng lõi Sprint 2 đã triển khai; hồi quy 38/38 đạt, chờ nhóm nghiệm thu phát hành V1.0.** Đây là nơi theo dõi tiến độ và hướng dẫn cấu hình chính của repository. Không cần Trello để chạy ứng dụng; nhóm chọn theo dõi tại README.

- [Sprint 2: cách thử và tiến độ 22 task](#sprint2)
- [Chạy trên máy](#chay-du-an)
- [Cấu hình Gmail SMTP](#smtp)
- [Thử các chức năng](#thu-chuc-nang)
- [Tiến độ từng task](#tien-do)
- [Giải thích các thành phần](#thanh-phan)
- [Xử lý lỗi thường gặp](#xu-ly-loi)

<a id="thanh-phan"></a>
## 1. Các thành phần liên kết với nhau

```mermaid
flowchart LR
    Browser[Trình duyệt: HTML / CSS / JavaScript] -->|HTTPS| MVC[ASP.NET Core MVC / C#]
    MVC --> Identity[Identity: tài khoản và quyền]
    MVC --> Service[Service: nghiệp vụ]
    Service --> EF[EF Core: chuyển thao tác C# thành SQL]
    EF --> SQL[(SQL Server Express)]
    MVC --> Razor[Razor: ghép dữ liệu thành trang HTML]
    Razor --> Browser
    MVC --> SMTP[SMTP qua Gmail: gửi email]
```

| Thành phần | Vai trò | Có bắt buộc để chạy không? |
| --- | --- | --- |
| .NET 10 SDK | Biên dịch và chạy máy chủ C#, cú pháp gần Java | Có; Setup tải SDK cục bộ nếu máy chưa có |
| ASP.NET Core MVC | Nhận yêu cầu từ trình duyệt, xử lý và trả trang | Có, nằm trong project |
| Razor + Bootstrap + HTML/CSS/JS | Giao diện và thao tác trên trình duyệt | Có; Bootstrap đã lưu trong wwwroot, không cần npm |
| Identity | Băm mật khẩu, đăng nhập, khóa tài khoản, phân quyền | Có, tải bằng NuGet |
| EF Core | Ánh xạ class thành bảng, tạo/truy vấn SQL và migration | Có, tải bằng NuGet |
| SQL Server Express | Lưu tài khoản, hồ sơ và môn học | Dùng mặc định; có SQLite để học thử |
| SSMS | Công cụ để người lập trình xem dữ liệu SQL Server | Không cần để web chạy, nhưng hữu ích khi học SQL |
| SMTP | Chuyển email của ứng dụng đến hộp thư thật | Cần để nghiệm thu gửi thư thật; chế độ Pickup vẫn chạy được trên máy |
| Git/GitHub | Quản lý lịch sử mã nguồn, phối hợp và review | Không cần khi chỉ chạy bản đã tải về |
| Trello/Jira | Quản lý công việc nhóm | Không cần; bỏ qua theo lựa chọn nhóm |

Chi tiết ví dụ “bấm Lưu hồ sơ → Controller → Service → SQL → Razor” ở [HOW_IT_WORKS](docs/design/HOW_IT_WORKS.md). [ERD Sprint 1](database/diagrams/SPRINT_1_ERD.md) mô tả bảng và quan hệ.

<a id="chay-du-an"></a>
## 2. Chạy trên Windows

### 2.1. Chuẩn bị lần đầu

- Windows, PowerShell 5.1 hoặc PowerShell 7, Git và trình duyệt Edge/Chrome.
- SQL Server Express đang hoạt động; biết tên máy chủ và tài khoản có quyền tạo database. SSMS là công cụ quản trị, không thay SQL Server.
- Internet để tải SDK/thư viện lần đầu. Mã hiện dùng SDK **10.0.403**, EF Core/Identity **10.0.14**.

```powershell
git clone https://github.com/hhai0410/GiaSuTrucTuyenGS.git
cd GiaSuTrucTuyenGS

# Chỉ cần trên máy khác LOQ: thay bằng tên SQL Server của máy bạn.
$env:ConnectionStrings__DefaultConnection = 'Server=TENMAY\SQLEXPRESS;Database=GiaSuOnlineGS;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=10'

powershell -ExecutionPolicy Bypass -File .\scripts\Setup.ps1
```

Nếu đang ở máy LOQ của nhóm, không đặt dòng connection string mẫu. Mặc định là `LOQ\SQLEXPRESS`, database riêng `GiaSuOnlineGS`, xác thực Windows. `TrustServerCertificate=True` dành cho SQL Server phát triển trên máy; triển khai thực tế cần chứng chỉ SQL Server được tin cậy và cấu hình phù hợp.

Setup tải SDK vào `.tools/dotnet` nếu cần, tải thư viện, build, áp dụng migration vào database và yêu cầu tin cậy chứng chỉ HTTPS phát triển. Bạn tự xác nhận hộp thoại Windows. Nếu máy có SDK khác nhưng không đáp ứng `global.json`, cài .NET SDK 10.0.403 hoặc phiên bản 10.0 mới hơn tương thích.

### 2.2. Khởi động

```powershell
# Trên máy này, nếu bản xem thử nền còn chạy, dừng trước khi biên dịch/chạy lại:
powershell -ExecutionPolicy Bypass -File .\scripts\Stop-Preview.ps1

# Chạy bình thường
powershell -ExecutionPolicy Bypass -File .\scripts\Start.ps1
```

Mở **https://localhost:7043**. Giữ terminal mở; nhấn Ctrl+C để dừng. `http://localhost:5043` chuyển sang HTTPS. Các script đã lưu **UTF-8 có BOM**, đọc được tiếng Việt trong Windows PowerShell 5.1.

Trên máy mới chưa có admin:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Start.ps1 -CreateAdmin
```

Nhập email quản trị mới và mật khẩu khi được hỏi. Không dùng email đã tự đăng ký: seed không tự nâng quyền tài khoản đang tồn tại. Trên máy LOQ, thông tin quản trị thử đã có tại `.local/admin-login.txt`, chỉ mở trên máy; không đưa tệp này lên GitHub.

### 2.3. Chạy thử khi chưa có SQL Server hoặc chưa muốn gửi thư thật

```powershell
# SQLite demo + email lưu trên máy, không gửi qua Gmail
powershell -ExecutionPolicy Bypass -File .\scripts\Start.ps1 -Sqlite -PickupEmail -CreateAdmin
```

SQLite lưu riêng tại `src/GiaSuOnline.Web/App_Data/gs-dev-v1.db`, không tự đồng bộ sang SQL Server. Chế độ này dùng EnsureCreated để tạo schema demo; migration trong repository dành cho SQL Server.

<a id="smtp"></a>
## 3. Cấu hình Gmail SMTP

SMTP là giao thức để ứng dụng gửi thư qua nhà cung cấp email. GS dùng cho **xác nhận đăng ký, quên mật khẩu và kết quả duyệt hồ sơ**. Không phải tạo một tài khoản Gmail cho mỗi học viên; chỉ cần một tài khoản gửi của nhóm.

### 3.1. Ý nghĩa từng thông tin

| Thông tin | Gmail | Ý nghĩa |
| --- | --- | --- |
| Host | `smtp.gmail.com` | Địa chỉ máy chủ nhận yêu cầu gửi thư |
| Port | `587` | Cổng kết nối SMTP có STARTTLS |
| UseStartTls | `true` | Bắt buộc nâng kết nối lên TLS trước khi đăng nhập |
| From / Sender | Gmail gửi của nhóm | Địa chỉ học viên nhìn thấy là người gửi |
| Username | Cùng Gmail gửi | Tài khoản để đăng nhập SMTP |
| Password | Mật khẩu ứng dụng Google | Nhập riêng trên máy; không dùng mật khẩu Google thông thường |
| Người nhận thư thử | Email mà bạn sở hữu/được phép gửi tới | Kiểm tra thư thực sự đến hộp thư; không phải cấu hình gửi cố định cho mọi email |
| PublicOrigin | `https://localhost:7043` khi chạy local | Địa chỉ gốc dùng tạo link xác nhận/reset |

Khi triển khai lên tên miền thật, đổi `PublicOrigin` và `AllowedHosts` cho đúng máy chủ. Link localhost chỉ mở đúng ứng dụng trên máy đang chạy; người khác không thể xác nhận qua localhost của bạn từ máy họ.

### 3.2. Tạo mật khẩu ứng dụng

1. Đăng nhập tài khoản Gmail gửi.
2. Bật xác minh hai bước nếu chưa có.
3. Mở [Mật khẩu ứng dụng Google](https://myaccount.google.com/apppasswords), tạo mục riêng cho GS.
4. Dùng mật khẩu ứng dụng 16 ký tự vừa được Google cung cấp ở bước cấu hình dưới đây. Script tự bỏ khoảng trắng khi dán và từ chối chuỗi không đủ 16 ký tự; kiểm tra này không thay việc xác thực với Google. Không gửi vào chat, không ghi trong README hoặc appsettings đã commit.

Một số loại tài khoản không hiển thị lựa chọn này; xem [hướng dẫn chính thức của Google](https://support.google.com/accounts/answer/185833?hl=vi). Nếu không có tùy chọn, cần dùng phương án được tài khoản hỗ trợ (ví dụ OAuth), không tắt bảo vệ tài khoản để thử đăng nhập thường.

### 3.3. Nhập trên máy và lưu mã hóa

```powershell
# Thay email mẫu bằng Gmail của bạn; script sẽ hỏi mật khẩu một cách kín.
powershell -ExecutionPolicy Bypass -File .\scripts\Configure-Smtp.ps1 -Sender "email-cua-ban@gmail.com"
```

Các giá trị Gmail Host/Port/TLS được điền sẵn. Hai tệp riêng trên máy:

- `.local/smtp.json`: cấu hình máy chủ/người gửi, không chứa mật khẩu.
- `.local/smtp-credential.xml`: mật khẩu được Windows DPAPI mã hóa cho tài khoản Windows hiện tại trên máy này.

Cả thư mục `.local/` đã bị Git bỏ qua. Không chia sẻ cả tệp đã mã hóa; mỗi thành viên tự cấu hình. Nếu đổi tài khoản Windows/máy hoặc thu hồi mật khẩu Google, chạy lại Configure-Smtp. Không nhập mật khẩu ngay trong tham số dòng lệnh hoặc lịch sử shell.

`Start.ps1` tự nạp cấu hình riêng khi cả hai tệp tồn tại. Nếu chưa cấu hình, Development dùng Pickup. Muốn chủ động thử offline dù đã cấu hình Gmail: chạy `Start.ps1 -PickupEmail`.

### 3.4. Gửi một thư kiểm tra

Sau khi build thành công và trước khi khởi động web (thư kiểm tra và ba email nghiệp vụ đã được xác minh trên máy nhóm ngày 29/10/2026):

```powershell
# Thay địa chỉ mẫu bằng hộp thư nhận bạn cho phép.
powershell -ExecutionPolicy Bypass -File .\scripts\Test-Smtp.ps1 -To "email-nhan@example.com"
```

Lệnh dùng chính EmailSender của ứng dụng, không cần kết nối SQL Server và không mở endpoint gửi thư công khai. “Máy chủ SMTP đã chấp nhận” nghĩa là máy chủ đã nhận yêu cầu; cần người nhận kiểm tra Hộp thư đến/Spam để xác nhận giao thư cuối cùng. Lệnh này không thay việc kiểm thử các email nghiệp vụ.

### 3.5. Cấu hình bằng môi trường khi triển khai

Không dùng tệp DPAPI của một máy cho máy chủ khác. Ứng dụng hỗ trợ `Email__Mode`, `Email__Host`, `Email__Port`, `Email__From`, `Email__Username`, `Email__Password`, `Email__UseStartTls`, `PublicOrigin`, `AllowedHosts` và `ConnectionStrings__DefaultConnection` từ biến môi trường/secret store của môi trường triển khai. Không đặt bí mật trong repository. Nhà cung cấp cần OAuth phải có tích hợp OAuth tương ứng; script hiện dành cho SMTP dùng mật khẩu ứng dụng/credential được nhà cung cấp hỗ trợ.

<a id="thu-chuc-nang"></a>
## 4. Thử chức năng Sprint 1

1. **Gia sư:** đăng ký vai trò Tutor → mở thư xác nhận → đăng nhập → tạo hồ sơ (giới thiệu từ 20 ký tự, học phí dương, ít nhất một môn, ảnh JPG/PNG và chứng chỉ JPG/PNG/PDF; mỗi tệp tối đa 5 MB) → gửi duyệt.
2. **Admin:** đăng nhập ở cửa sổ riêng → Kiểm duyệt → mở hồ sơ/chứng chỉ → duyệt hoặc từ chối với lý do. Email kết quả được gửi qua cấu hình đang dùng.
3. **Khách:** chỉ thấy hồ sơ đã duyệt. Hồ sơ chờ duyệt hoặc không tồn tại trả 404. Gia sư sửa hồ sơ đã duyệt thì quay lại chờ duyệt và ẩn khỏi công khai.
4. **Tài khoản:** thử quên mật khẩu, token 30 phút và dùng một lần; sai mật khẩu 5 lần thì khóa 15 phút; đăng xuất hủy phiên.
5. **Quyền:** Student không vào khu vực Admin/Tutor; chứng chỉ chỉ chủ hồ sơ/Admin được tải.

Nếu dùng Pickup, mở tệp `.txt` mới nhất trong `src/GiaSuOnline.Web/App_Data/mail/`, kiểm tra đúng người nhận rồi mở link. Đây là email thử trên máy, không đến Gmail thật. Các tệp thư chứa token nên không được commit.

Sprint 2 đã nối trang chi tiết với lịch rảnh và tạo booking thật. Đánh giá mới vẫn thuộc US15 mở rộng; khi chưa có đánh giá, trang hiển thị trạng thái chưa có dữ liệu.

## 5. Kiểm thử

Dừng bản web đang chạy trước khi build/kiểm thử để tránh Windows khóa file:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Stop-Preview.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\Test.ps1
```

Bộ kiểm thử dùng SQLite in-memory và email giả lập, không xóa database SQL Server. Có kiểm chứng bổ sung luồng thật với SQL Server. Xem [bằng chứng kiểm thử](docs/testing/SPRINT_1_RESULTS.md) và các kịch bản nghiệm thu thủ công còn chờ.

<a id="tien-do"></a>
## 6. Tiến độ dự án

### Lộ trình từ workbook

| Giai đoạn | Kế hoạch | Phạm vi | Trạng thái |
| --- | --- | --- | --- |
| Sprint 1 | 26/10–13/11/2026 | US01, US02, US03, US04, US06, US19 | Đã triển khai và kiểm chứng email thật; chờ nhóm nghiệm thu cuối |
| Sprint 2 | 16/11–04/12/2026 | US05, US07, US08, US09: tìm kiếm/lọc, lịch rảnh, đặt và xác nhận | Đã triển khai, kiểm thử và đo hiệu năng; chờ nhóm nghiệm thu V1.0 |
| Sprint 3 | 07/12–25/12/2026 | US12, US14, US20: thanh toán sandbox, video, UAT/triển khai | Chưa triển khai; mục tiêu phát hành V2.0 |
| Mở rộng | Chưa cam kết | US10, US11, US13, US15–US18 | Chỉ làm khi được nhóm đưa vào phạm vi |

### Kết quả Sprint 1 và việc còn chờ

- Đã áp dụng migration vào `LOQ\SQLEXPRESS / GiaSuOnlineGS`; đã seed ba vai trò và sáu môn học.
- Luồng SQL Server thật đã chạy với **Gmail SMTP**: đăng ký tài khoản thử riêng → người dùng mở email xác nhận → đăng nhập → tạo hồ sơ → admin duyệt → khách xem. Người dùng đã nhận email duyệt, mở email đặt lại mật khẩu, đổi mật khẩu và đăng nhập lại thành công.
- Bộ kiểm thử mở rộng: **21/21 đạt**, gồm 14 ca lõi và 7 ca cấu hình email/TLS, từ chối cấu hình sai và chặn Pickup ngoài Development.
- Gmail SMTP: **đã cấu hình, xác thực TLS và gửi/nhận thành công**. Đã nhận thư kiểm tra và đủ ba email nghiệp vụ (xác nhận tài khoản, kết quả duyệt, đặt lại mật khẩu). Lỗi `5.7.9` ban đầu đã được khắc phục bằng mật khẩu ứng dụng hợp lệ. Mật khẩu chỉ lưu mã hóa trên máy, không ở GitHub.
- GitHub CLI: đã kiểm tra đăng nhập thành công ngày 29/10/2026. Mã Sprint 1 có trên `main`. Cập nhật cấu hình SMTP và README thực hiện bằng commit trực tiếp vào `main` theo xác nhận của chủ repository; chưa có PR/review độc lập của thành viên khác.
- Trello/Jira: bỏ qua theo lựa chọn nhóm; tiến độ thể hiện tại README này.
- Còn chờ nhóm thử giao diện trên desktop/mobile, review/acceptance, Sprint Review/Retrospective. Không tạo biên bản hoặc ghi giờ công giả.

### Đối chiếu đủ 29 task Sprint 1

Nguồn: `docs/Nhom8_Scrum_GiaSuOnline.xlsx`, sheet `Sprint backlog 1`, tổng 124 giờ dự kiến. “Đã triển khai” mô tả mã đã có; không đồng nghĩa toàn bộ task đã được nhóm nghiệm thu. Không sửa trạng thái/giờ thực tế trong workbook thay nhóm.

| Task | Công việc | Trạng thái thực tế | Bằng chứng / giới hạn |
| --- | --- | --- | --- |
| T01.1 | Thiết kế CSDL người dùng | Đã triển khai, đã áp dụng SQL Server | Identity, unique email; migration; seed Student/Tutor/Admin; ERD |
| T01.2 | Xử lý đăng ký (Controller/Service) | Đã triển khai và xác minh Gmail thật | AccountController, Identity, EmailSender; Gmail thật đã nhận và mở link xác nhận thành công |
| T01.3 | Giao diện đăng ký | Đã triển khai; chờ nhóm xem giao diện | Views/Account/Register.cshtml, Bootstrap, validation server và HTML |
| T01.4 | Kiểm thử đăng ký | Đã kiểm thử tự động | RegistrationHashesPasswordSeedsRolesAndSendsConfirmation; RegistrationRejectsInvalidInputs |
| T01.5 | Review code & merge | Chưa hoàn tất quy trình | Mã có trên main, build và kiểm thử đạt; chưa có PR/review của thành viên khác |
| T02.1 | Đăng nhập / đăng xuất | Đã triển khai và kiểm thử | AccountController; khóa 5 lần/15 phút; logout đổi security stamp |
| T02.2 | Quên / đặt lại mật khẩu | Đã triển khai và kiểm thử; Gmail thật đã đạt | Identity token 30 phút; kiểm thử dùng lại/hết hạn đạt; người dùng đã reset qua Gmail và đăng nhập bằng mật khẩu mới |
| T02.3 | Giao diện đăng nhập & quên mật khẩu | Đã triển khai; chờ nhóm xem giao diện | Login, EmailForm, ResetPassword, Message |
| T02.4 | Kiểm thử đăng nhập | Đã kiểm thử tự động | LockoutAfterFiveFailuresForFifteenMinutes; ResetTokenSingleUseAndInvalidatesOldPassword; ExpiredResetTokenRejected |
| T02.5 | Review code & merge | Chưa hoàn tất quy trình | Mã Sprint 1 đã có commit trên main; chưa có PR và review độc lập |
| T03.1 | Thiết kế CSDL hồ sơ gia sư | Đã triển khai, đã áp dụng SQL Server | TutorProfile, TutorSubject, Subject; migration; seed 6 môn; ERD |
| T03.2 | CRUD hồ sơ gia sư | Đã triển khai và kiểm thử | ProfileService; form chỉ nhận dữ liệu cho phép; sửa luôn Pending; concurrency token |
| T03.3 | Upload ảnh và chứng chỉ | Đã triển khai và kiểm thử | UploadStore: JPG/PNG/PDF, kiểm tra signature, tối đa 5 MB/tệp, tên ngẫu nhiên, App_Data ngoài wwwroot |
| T03.4 | Giao diện form hồ sơ nhiều bước | Đã triển khai; chờ nhóm thử trực quan | Stepper 3 bước, kiểm tra client/server, ảnh xem trước; cần kiểm tra desktop/mobile thật |
| T03.5 | Trang xem hồ sơ cá nhân | Đã triển khai và kiểm thử HTTP | Mine: thông tin, trạng thái, lý do từ chối, liên kết tệp có quyền |
| T03.6 | Kiểm thử hồ sơ gia sư | Đã kiểm thử tự động | InvalidUploadsRejected; InvalidPriceRejectedAndSqlInputDoesNotChangeSchema; ProfileApprovalRejectionEditingPrivacyAndXssEndToEnd |
| T19.1 | Thiết lập repo Git, quy ước nhánh, bảng công việc | Hoàn thành một phần | Có repo main, CONTRIBUTING và mẫu PR; dùng README theo dõi công việc, bỏ qua Trello theo lựa chọn nhóm; nhánh/PR còn chờ |
| T19.2 | Cấu hình HTTPS/HSTS | Đã cấu hình; HTTPS cục bộ đã chạy | Chứng chỉ .NET đã trust; HTTP→HTTPS kiểm thử; HSTS bật ngoài Development, cần kiểm tra tại môi trường triển khai |
| T19.3 | Chống CSRF/XSS/SQL Injection | Đã triển khai và kiểm thử cơ bản | AutoValidateAntiforgeryToken, Razor encode, EF Core query tham số, CSP, cookie Secure/HttpOnly |
| T19.4 | Phân quyền theo vai trò | Đã triển khai và kiểm thử | Authorize theo role; kiểm tra chủ sở hữu tệp; trang 403/404 tùy chỉnh |
| T19.5 | Kiểm thử bảo mật cơ bản | Đã kiểm thử tự động cơ bản | CSRF→400, sai role→403, HTTP→308, script bị encode, SQL input không thay đổi schema; không thay thế kiểm thử bảo mật đầy đủ |
| T04.1 | Trạng thái hồ sơ gia sư | Đã triển khai, đã áp dụng SQL Server | ProfileStatus Pending/Approved/Rejected, RejectionReason và Revision |
| T04.2 | Trang admin danh sách chờ duyệt | Đã triển khai; chờ nhóm xem giao diện | Admin/Profiles: lọc trạng thái, danh sách, chi tiết và tải chứng chỉ |
| T04.3 | Chức năng duyệt / từ chối | Đã triển khai và kiểm thử | Moderate kiểm tra Pending/Revision, từ chối bắt buộc lý do; chống duyệt hồ sơ đã đổi |
| T04.4 | Email kết quả duyệt | Đã triển khai và xác minh Gmail thật | EmailSender MailKit dùng chung; Gmail thật đã nhận email duyệt; có báo lỗi và nút gửi lại khi cần |
| T04.5 | Kiểm thử duyệt hồ sơ | Đã kiểm thử tự động và thử SQL Server | Luồng pending→reject→resubmit→approve; pending không công khai |
| T06.1 | Giao diện trang chi tiết gia sư | Đã triển khai; phần booking giới hạn Sprint 1 | Trang chi tiết và nút Đặt lịch dẫn tới trang khung giờ; chưa tạo booking Sprint 2 |
| T06.2 | Dữ liệu đánh giá và lịch rảnh tóm tắt | Đã triển khai phần đọc; chưa có chức năng tạo dữ liệu | Truy vấn 5 đánh giá gần nhất và slot trống 7 ngày tới; trạng thái rỗng trung thực; tạo lịch/đánh giá theo sprint sau |
| T06.3 | Kiểm thử trang chi tiết | Đã kiểm thử tự động và thử SQL Server | Hồ sơ đã duyệt→200, chưa duyệt/ID không tồn tại→404, nút dẫn trang khung giờ |


### Lưu ý kế hoạch

Hàng ngày trong sheet Sprint backlog 1 có ô công thức cached `#REF!`; cần kiểm tra công thức trước khi dùng biểu đồ burndown làm minh chứng. US06 có phần đọc lịch/đánh giá trong khi tạo lịch/đánh giá nằm ở sprint sau: nhóm cần nghiệm thu rõ trạng thái chưa có dữ liệu thay vì xem đây là chức năng booking/đánh giá đã hoàn thành.

<a id="sprint2"></a>
## Sprint 2 – tìm kiếm, lịch rảnh và đặt lịch

Triển khai và kiểm chứng kỹ thuật trong **14–15/11/2026**, trước lịch kế hoạch **16/11–04/12/2026** trong workbook. Phạm vi: **US05, US07, US08, US09 – 22 task, 116 giờ dự kiến**. Không ghi giờ thực tế hoặc đổi trạng thái workbook thay nhóm.

**Kết quả:** build không lỗi/cảnh báo; hồi quy **38/38 đạt, không bỏ qua**, gồm kiểm thử thực trên SQL Server. Luồng HTTP qua HTTPS đã đạt: đăng ký/xác nhận thử → duyệt gia sư → thêm lịch → chặn chồng lấn → học viên đặt → gia sư xác nhận → học viên xem kết quả. Mục tiêu chức năng của V1.0 đã có bản chạy để nhóm nghiệm thu; chưa tự gắn tag/phát hành chính thức hoặc thay biên bản review của nhóm.

### Chạy bản đã cập nhật trên máy đang có Sprint 1

```powershell
# Dừng bản web hiện tại; nếu chạy trong terminal khác thì Ctrl+C tại đó.
powershell -ExecutionPolicy Bypass -File .\scripts\Stop-Preview.ps1

# Tải thư viện, build và áp dụng migration mới; giữ tài khoản/hồ sơ SQL Server cũ.
powershell -ExecutionPolicy Bypass -File .\scripts\Setup.ps1

# Dùng cấu hình Gmail đã lưu trên máy, hoặc Pickup nếu chưa cấu hình.
powershell -ExecutionPolicy Bypass -File .\scripts\Start.ps1
```

Migration `20261114160003_SprintTwoScheduling` đã áp dụng vào `GiaSuOnlineGS` trên máy nhóm. Hồ sơ cũ mặc định ngôn ngữ Tiếng Việt và có thể được gia sư chỉnh lại. Không dùng EnsureDeleted/khởi tạo lại database chính. SQLite demo chuyển sang tệp `App_Data/gs-dev-v1.db`; tệp demo Sprint 1 cũ được giữ riêng, không tự chuyển dữ liệu.

### Thử theo vai trò

1. **Gia sư đã duyệt:** vào **Lịch rảnh → Thêm khung giờ**; nhập giờ Việt Nam (UTC+7). Chọn tuần/tháng/danh sách; điện thoại mặc định danh sách tuần. Các khung giờ không được chồng lấn; khung đã đặt không sửa/xóa được.
2. **Học viên/khách:** vào **Tìm gia sư**; lọc theo từ khóa, môn, giá, ngôn ngữ, đánh giá; đổi thứ tự và chuyển trang (10 hồ sơ/trang).
3. **Học viên:** mở hồ sơ → **Đặt lịch** → chọn tuần và slot trống → xem thời gian/học phí/tổng tiền → gửi yêu cầu. Nếu chưa đăng nhập, hệ thống yêu cầu đăng nhập; gia sư/Admin không có quyền đặt thay học viên.
4. **Gia sư:** vào **Yêu cầu học** → chấp nhận hoặc từ chối. Học viên xem ở **Lịch đã đặt**. Chỉ gia sư của booking và học viên đã đặt xem được chi tiết.
5. **Hết hạn:** yêu cầu chưa phản hồi tự hết hạn sau 24 giờ; nếu đến giờ học trước mốc đó, cũng không được xác nhận nữa. Tác vụ nền quét mỗi 30 giây; khi web tắt, tác vụ tạm dừng và xử lý bù khi khởi động. Không cần chờ thật 24 giờ để kiểm thử: bộ test có đồng hồ giả lập.

Giá/thời gian được chốt tại lúc đặt, không tin số tiền gửi từ trình duyệt. Gia sư đổi học phí sau đó không làm đổi booking đã tạo. Thanh toán, phòng video, hủy/đổi lịch theo yêu cầu người dùng và đánh giá mới vẫn ngoài phạm vi Sprint 2.

### Email đặt lịch

Email yêu cầu mới gửi cho gia sư; email kết quả/hết hạn gửi cho học viên. Thư lưu trong `BookingEmails` cùng giao dịch booking rồi tác vụ nền gửi bằng EmailSender đã có. Có retry khi SMTP lỗi và cơ chế nhận việc riêng để hai tiến trình không cùng gửi một thư. Nếu tiến trình bị dừng đúng sau khi SMTP nhận thư nhưng trước khi ghi nhận thành công, thư có thể được gửi lại; đây không phải cam kết giao email đúng một lần.

Bài kiểm chứng Sprint 2 dùng **Pickup** trên database demo: đã gửi thành công **2/2 thông báo**, không còn thư chờ. Không gửi email nghiệp vụ mới ra Gmail trong lần triển khai này. Gmail SMTP thật đã được kiểm chứng ở Sprint 1; hãy thử lại theo chính sách/tài khoản của nhóm khi nghiệm thu người dùng thật.

### Database demo và đo hiệu năng

```powershell
# Dùng database GiaSuOnlineGS_Demo riêng; không thêm 500 hồ sơ giả vào dữ liệu thật.
powershell -ExecutionPolicy Bypass -File .\scripts\Benchmark-Search.ps1

# Mở bản demo, toàn bộ email lưu trên máy; lần đầu có thể tự tạo admin demo.
powershell -ExecutionPolicy Bypass -File .\scripts\Start-Demo.ps1 -CreateAdmin

# Máy khác: thêm -SqlServer 'TENMAY\SQLEXPRESS' cho các lệnh demo.
```

Dừng bản web đang dùng cổng 7043 trước khi mở bản demo. Sau demo, Ctrl+C và chạy `Start.ps1` để trở về database chính. 500 hồ sơ seed chỉ là dữ liệu giả, không có mật khẩu đăng nhập; tạo Student/Tutor bằng form và xác nhận email Pickup để demo nghiệp vụ. Nếu nhóm thêm tài khoản/hồ sơ thử, tổng hồ sơ demo có thể lớn hơn 500; công cụ luôn in số lượng thực tế.

| Phép đo trên 500 hồ sơ (máy nhóm) | Kết quả |
| --- | --- |
| SQL/EF, lần truy vấn đầu | 537.5 ms |
| SQL/EF, chậm nhất trong 5 lần sau | 20.7 ms |
| HTTP qua HTTPS, chậm nhất trong 6 lần lọc kết hợp | 445.8 ms |
| Mục tiêu workbook | Không quá 2.000 ms |

Đây là phép đo cục bộ trước khi thêm hồ sơ demo nghiệp vụ, không phải cam kết tốc độ Internet hoặc kiểm thử tải nhiều người dùng đồng thời.

### Kiểm thử hồi quy

```powershell
# Có SQL Server Express trên máy nhóm: chạy đủ 38 test.
powershell -ExecutionPolicy Bypass -File .\scripts\Test.ps1 -SqlServer

# Máy SQL Server khác:
powershell -ExecutionPolicy Bypass -File .\scripts\Test.ps1 -SqlServer -SqlServerName 'TENMAY\SQLEXPRESS'

# Không có SQL Server: chạy SQLite và các kiểm thử còn lại; hai test SQL Server được báo Skip.
powershell -ExecutionPolicy Bypass -File .\scripts\Test.ps1
```

Test SQL Server chỉ tạo/xóa database ngẫu nhiên có tiền tố `GiaSuGS_Test_`, không xóa database chính hoặc demo. Đã kiểm tra: hai học viên tranh slot, hai thao tác thêm lịch chồng lấn, race xác nhận/hết hạn, giữ dữ liệu khi migrate từ Sprint 1, hai tiến trình gửi mail, retry SMTP, quyền sở hữu/CSRF và dữ liệu học phí. Xem [báo cáo Sprint 2](docs/testing/SPRINT_2_RESULTS.md).

### Đối chiếu 22 task Sprint 2

| Task | Công việc | Trạng thái | Bằng chứng / giới hạn |
| --- | --- | --- | --- |
| T05.1 | Truy vấn tìm kiếm và lọc | Đã triển khai/kiểm thử | Kết hợp từ khóa, môn, giá, ngôn ngữ và điểm đánh giá; sắp giá/đánh giá, chỉ hiện hồ sơ đã duyệt. |
| T05.2 | Tối ưu CSDL và phân trang | Đã triển khai/đo SQL Server | Index tìm kiếm và phân trang 10 hồ sơ; 500 hồ sơ giả trong GiaSuOnlineGS_Demo, không vào database chính. |
| T05.3 | Giao diện trang tìm kiếm | Đã triển khai/kiểm tra trình duyệt | Sidebar, thẻ gia sư, giữ bộ lọc khi chuyển trang, trạng thái rỗng/đang tải; desktop và mobile đã xem. |
| T05.4 | Gợi ý từ khóa tìm kiếm | Đã triển khai/kiểm thử | API gợi ý tối đa 8 tên gia sư/môn có hồ sơ đã duyệt; datalist, debounce và hủy yêu cầu cũ. |
| T05.5 | Kiểm thử chức năng và hiệu năng | Đã kiểm thử/đạt mục tiêu | SQL cold 537.5 ms, warm max 20.7 ms; HTTP max 445.8 ms trong bài đo 500 hồ sơ. Không cam kết tốc độ trên mọi máy. |
| T05.6 | Review code & merge | Build/test đạt; chờ review nhóm | Theo quy trình main đã chọn; không tự tạo bằng chứng review của thành viên khác hoặc merge vào develop. |
| T07.1 | Thiết kế CSDL lịch rảnh | Đã triển khai/migrate | AvailabilitySlot có UTC, cờ xóa mềm, revision và index; nâng cấp giữ slot Sprint 1. |
| T07.2 | CRUD khung giờ rảnh + chống chồng lấn | Đã triển khai/kiểm thử | Thêm/sửa/xóa mềm; khóa lịch theo gia sư trong transaction để chặn chồng lấn và chặn sửa/xóa slot đã đặt. |
| T07.3 | Giao diện lịch tuần của gia sư | Đã triển khai/kiểm tra trình duyệt | FullCalendar 6.1.22 miễn phí, tiếng Việt; tuần/tháng/danh sách, màu khác cho slot đã đặt; mobile dùng danh sách tuần. |
| T07.4 | Xử lý múi giờ và định dạng | Đã triển khai/kiểm thử | Lưu UTC, form/hiển thị cố định UTC+7, không phụ thuộc múi giờ trình duyệt; lịch hỗ trợ đủ 24 giờ. |
| T07.5 | Kiểm thử lịch rảnh | Đã kiểm thử | Chồng lấn, quá khứ, chủ sở hữu, revision cũ, giờ UTC và edit/delete slot đã đặt. |
| T07.6 | Review code và chuẩn bị demo Sprint 2 | Demo đã chuẩn bị; chờ review nhóm | Database demo riêng, Start-Demo.ps1, hướng dẫn tái hiện, ảnh desktop/mobile và luồng đã xác nhận. |
| T08.1 | Thiết kế CSDL đặt lịch | Đã triển khai/migrate | Bookings: Pending/Confirmed/Rejected/Expired; FK, học phí/thời gian snapshot và unique index cho slot đang giữ. |
| T08.2 | Logic đặt lịch và chống đặt trùng | Đã triển khai/kiểm thử đồng thời | Transaction + khóa ghi cùng hồ sơ gia sư + revision + unique filtered index. Hai kết nối SQL Server tranh slot: chỉ một thành công. |
| T08.3 | Giao diện chọn khung giờ và xác nhận | Đã triển khai/kiểm tra HTTP và trình duyệt | Chọn tuần/slot → xem thời gian, học phí, tổng tiền → POST xác nhận; giá tính ở server; lỗi thân thiện khi slot/giá vừa đổi. |
| T08.4 | Email thông báo yêu cầu cho gia sư | Đã triển khai; Pickup đã nhận | Email yêu cầu cho gia sư qua hàng đợi lưu cùng transaction; SMTP lỗi được retry, không hủy booking đã lưu. |
| T08.5 | Kiểm thử đặt lịch, kể cả đồng thời | Đã kiểm thử | Đặt đồng thời trên SQLite và SQL Server, slot quá khứ, chưa đăng nhập, sửa StudentId, gửi lại form, snapshot học phí. |
| T08.6 | Review code & merge | Build/test đạt; chờ review nhóm | Chưa tự ghi nhận review độc lập hoặc gắn tag phát hành v1.0 trước nghiệm thu. |
| T09.1 | Giao diện danh sách yêu cầu của gia sư | Đã triển khai/kiểm tra trình duyệt | Danh sách yêu cầu đúng gia sư, nút chấp nhận/từ chối cho Pending; trang chi tiết chỉ hai bên xem. |
| T09.2 | Chấp nhận / từ chối và cập nhật trạng thái | Đã triển khai/kiểm thử | Chấp nhận giữ slot; từ chối mở slot và giữ lịch sử. Email kết quả qua outbox; request cũ/đã xử lý không đổi trạng thái lại. |
| T09.3 | Tự hủy yêu cầu quá 24 giờ | Đã triển khai/kiểm thử | HostedService quét mỗi 30 giây; hết 24 giờ hoặc đã đến giờ học thì Expired, mở slot và thông báo. Tác vụ chạy lại khi web khởi động. |
| T09.4 | Kiểm thử xác nhận/từ chối | Đã kiểm thử | Xác nhận/từ chối, đặt lại sau từ chối, biên 24 giờ, đến giờ học sớm hơn 24 giờ và race xác nhận/hết hạn. |

### Minh chứng giao diện

Ảnh chụp từ **database Demo**; tên, học phí, lịch và đánh giá trong ảnh là dữ liệu thử.

![Tìm kiếm gia sư trên máy tính](docs/evidence/sprint-02/search-desktop.jpg)

[Ảnh tìm kiếm mobile](docs/evidence/sprint-02/search-mobile.jpg) · [Lịch tuần desktop](docs/evidence/sprint-02/calendar-desktop.jpg) · [Lịch mobile](docs/evidence/sprint-02/calendar-mobile.jpg) · [Booking đã xác nhận](docs/evidence/sprint-02/booking-confirmed.jpg).

### Việc nhóm cần nghiệm thu

- Thành viên khác review mã và xác nhận tiêu chí chấp nhận; việc commit vào main không thay review độc lập.
- Thử các tài khoản/thiết bị thật và xác nhận email đặt lịch qua SMTP nếu nhóm cần bằng chứng gửi ra ngoài cho Sprint 2.
- Sprint Review/Retrospective, cập nhật workbook và quyết định phát hành/tag V1.0 do nhóm thực hiện. Bản workbook đang sửa của người dùng được giữ nguyên và không đưa vào commit mã.

## 7. Tổ chức repository và quy trình làm việc

```text
src/GiaSuOnline.Web/    Ứng dụng MVC, Identity, dữ liệu, dịch vụ email và giao diện
  Controllers/         Nhận request của khách/học viên/gia sư
  Areas/Admin/         Giao diện và xử lý quản trị
  Models/              Thực thể tài khoản/hồ sơ/môn học
  ViewModels/          Dữ liệu form, không cho tự sửa quyền/trạng thái nội bộ
  Services/            Nghiệp vụ hồ sơ và lưu tệp riêng tư
  Data/Migrations/     Lịch sử cấu trúc bảng SQL Server
  Integrations/Email/  EmailSender và kiểm tra cấu hình SMTP
  Views/, wwwroot/     Razor, Bootstrap, CSS và JavaScript
scripts/               Chuẩn bị môi trường, chạy, cấu hình SMTP và kiểm thử
tests/                Kiểm thử HTTP, nghiệp vụ và cấu hình email
database/diagrams/    ERD
docs/                  Tài liệu nguồn, thiết kế, kiểm thử, hướng dẫn học phần
.local/, .tools/       Cấu hình riêng, công cụ cục bộ (không đưa lên Git)
```

[Xem quy ước đóng góp](CONTRIBUTING.md). Một thay đổi nên gắn mã US/task, chạy kiểm thử và được thành viên khác review trước khi merge. GitHub authentication chỉ cho phép thao tác repository; không tự thay thế review của con người. Không đưa mật khẩu SMTP, tài khoản demo, tệp người dùng, email có token hoặc thư mục build lên Git.

<a id="xu-ly-loi"></a>
## 8. Lỗi thường gặp

| Hiện tượng | Cách kiểm tra |
| --- | --- |
| `dotnet` không nhận | Chạy Setup.ps1; script dùng `.tools/dotnet`, không bắt buộc sửa PATH toàn máy |
| Lỗi không kết nối SQL | Kiểm tra SQL Server Express đang chạy, tên instance đúng và quyền Windows; thử cùng tên máy chủ trong SSMS |
| `Unexpected token` hoặc tiếng Việt thành `Cáº§n...` | Script cần UTF-8 có BOM cho Windows PowerShell 5.1. Bản hiện tại đã sửa; không lưu lại thành ANSI |
| Cổng đang dùng hoặc file exe bị khóa | Dừng terminal đang chạy web; bản xem thử nền thì dùng Stop-Preview.ps1 rồi thử lại |
| Chứng chỉ localhost không được tin cậy | Chạy `.tools/dotnet/dotnet.exe dev-certs https --trust`, tự xác nhận Windows rồi mở lại trình duyệt; không bỏ qua cảnh báo mù quáng |
| Không nhận email, chỉ có file trong App_Data | Đang dùng Pickup. Cấu hình SMTP và khởi động lại bằng Start.ps1 |
| Gmail báo `5.7.9 Application-specific password required` | Kiểm tra đúng Gmail, xác minh hai bước và mật khẩu ứng dụng còn hiệu lực; không dùng mật khẩu đăng nhập Google |
| Không giải mã được smtp-credential.xml | Tệp tạo từ máy/tài khoản Windows khác; chạy Configure-Smtp trên chính máy và tài khoản đang dùng |
| SMTP đã chấp nhận nhưng chưa thấy thư | Kiểm tra Spam, người nhận và độ trễ; chưa ghi nhận “đã đến hộp thư” cho đến khi người nhận xác nhận |
| Link xác nhận/reset không mở đúng web | PublicOrigin phải đúng địa chỉ bản đang chạy; localhost không dùng thay địa chỉ triển khai công khai |

## Tài liệu nguồn

- [Kế hoạch Scrum của nhóm](docs/Nhom8_Scrum_GiaSuOnline.xlsx).
- [Danh mục tài liệu học phần](docs/README.md).
- [Google: mật khẩu ứng dụng](https://support.google.com/accounts/answer/185833?hl=vi).
- [Google: SMTP và TLS](https://developers.google.com/workspace/gmail/imap/imap-smtp).

README ở thư mục gốc là nguồn hướng dẫn/tiến độ chính; không để một README khác trong `.github/` che nội dung này khi GitHub chọn file hiển thị.
