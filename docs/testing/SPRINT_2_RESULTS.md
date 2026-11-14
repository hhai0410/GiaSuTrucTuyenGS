# Kết quả Sprint 2

Ngày kiểm chứng: 14/11/2026 (UTC+7). Nguồn yêu cầu: workbook của nhóm, sheet Sprint backlog 2/Product Backlog. Trạng thái từng task được theo dõi duy nhất tại [README](../../README.md#sprint2).

## Kết quả

- Build: 0 lỗi, 0 cảnh báo. Hồi quy: 38/38 đạt, 0 bỏ qua khi bật SQL Server.
- SQL Server thật: tranh cùng slot chỉ một booking/email yêu cầu được tạo; thêm slot chồng lấn đồng thời chỉ một thành công; xác nhận không thắng được expiry sau hạn.
- Migration từ schema Sprint 1 được kiểm thử giữ hồ sơ/slot; migration mới đã áp dụng vào GiaSuOnlineGS.
- HTTP thật trên GiaSuOnlineGS_Demo: đăng ký và xác nhận Pickup, duyệt hồ sơ, thêm lịch, chặn chồng lấn, chọn tuần, tạo booking, xác nhận, chặn sửa slot đã đặt và chặn gửi lại form.
- Tác vụ nền đã xử lý 2/2 email demo qua Pickup, 0 chờ. Không gửi email ngoài trong lần thử Sprint 2.
- Trình duyệt: tìm kiếm/lịch tuần desktop và mobile 390×844, không tràn ngang toàn trang; trạng thái Đã xác nhận hiển thị đúng; không thấy lỗi console trong các màn hình đã kiểm tra.

## Tốc độ tìm kiếm

- SQL lần đầu 537.5 ms; warm max 20.7 ms.
- HTTP max 445.8 ms trong 6 lượt lọc trên 500 hồ sơ, mục tiêu ≤ 2.000 ms.
- Phép đo cục bộ, không phải kết quả load test. Data seed và đánh giá là giả, chỉ ở database demo.

## Danh sách kiểm thử tự động

| Trường hợp | Kết quả |
| --- | --- |
| GiaSuOnline.IntegrationTests.EmailTests.InvalidConfigurationRejectedBeforeConnection(key: "Email:From", value: "invalid") | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.ExpirationAt24HoursReleasesSlotOnceAndRejectsLateConfirmation | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.WeeklyCalendarAndEventsAreOwnerScopedAndUtc | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.SecurityBlocksCsrfRoleEscalationAndRedirectsHttp | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.ExpiredResetTokenRejected | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.Search500ProfilesHasStableTenItemPagesAndSorts | Passed |
| GiaSuOnline.IntegrationTests.EmailTests.InvalidConfigurationRejectedBeforeConnection(key: "Email:Host", value: "https://smtp.gmail.com") | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.SqlServerMigrationPreservesSprintOneProfilesAndExistingSlots | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.LogoutRevokesCopiedCookieAndPostRequired | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.TwoStudentsCompeteForOneSlotOnSeparateSqliteConnections | Passed |
| GiaSuOnline.IntegrationTests.EmailTests.MisspelledModeDoesNotSendOverNetwork | Passed |
| GiaSuOnline.IntegrationTests.EmailTests.InvalidConfigurationRejectedBeforeConnection(key: "Email:Password", value: "") | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.PendingTutorCannotCreateSlots | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.ChangedProfileOrSlotAndPastSlotCannotBeBooked | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.RegistrationHashesPasswordSeedsRolesAndSendsConfirmation | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.StartTimeBefore24HoursAlsoExpiresPendingRequest | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.RegistrationRejectsInvalidInputs(role: "Tutor", password: "short", email: "valid@example.test") | Passed |
| GiaSuOnline.IntegrationTests.EmailTests.GmailConfigurationRequiresTlsAndHidesPasswordInDiagnosticString | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.LockoutAfterFiveFailuresForFifteenMinutes | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.RejectReopensSlotAndRebookingPreservesHistory | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.InvalidUploadsRejected(name: "photo.png", oversized: True) | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.InvalidPriceRejectedAndSqlInputDoesNotChangeSchema | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.BookingSnapshotsPriceBlocksChangesAndCreatesOneEmail | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.RegistrationRejectsInvalidInputs(role: "Student", password: "abcdefgh", email: "invalid-email") | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.SlotCrudRejectsOverlapPastAndOtherOwnerAndConvertsUtc | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.InvalidUploadsRejected(name: "photo.jpg", oversized: False) | Passed |
| GiaSuOnline.IntegrationTests.EmailTests.PickupCannotBeUsedOutsideDevelopment | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.ResetTokenSingleUseAndInvalidatesOldPassword | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.HttpBookingFlowOwnershipRoleCsrfAndFriendlyDoubleSubmit | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.EmailFailureRetriesWithoutDuplicateBookingAndDispatcherDoesNotResendSent | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.RegistrationRejectsInvalidInputs(role: "Admin", password: "abcdefgh", email: "valid@example.test") | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.CombinedSearchLanguagePriceRatingAndPagination | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.SqlServerConcurrentBookingAndOverlappingSlots | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.InvalidUploadsRejected(name: "photo.exe", oversized: False) | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.ConfirmCannotBeProcessedTwiceAndSlotStaysReserved | Passed |
| GiaSuOnline.IntegrationTests.EmailTests.InvalidConfigurationRejectedBeforeConnection(key: "Email:Port", value: "70000") | Passed |
| GiaSuOnline.IntegrationTests.SprintOneTests.ProfileApprovalRejectionEditingPrivacyAndXssEndToEnd | Passed |
| GiaSuOnline.IntegrationTests.SprintTwoTests.ParallelEmailDispatchersClaimOnlyOneDelivery | Passed |

## Bằng chứng và giới hạn

- [Kết quả JSON đã loại mật khẩu/token](../evidence/sprint-02/verification.json).
- [Tìm kiếm desktop](../evidence/sprint-02/search-desktop.jpg), [mobile](../evidence/sprint-02/search-mobile.jpg), [lịch desktop](../evidence/sprint-02/calendar-desktop.jpg), [lịch mobile](../evidence/sprint-02/calendar-mobile.jpg), [booking đã xác nhận](../evidence/sprint-02/booking-confirmed.jpg).
- TRX đầy đủ trên máy: tests/GiaSuOnline.IntegrationTests/TestResults/sprint2-final.trx (Git bỏ qua).
- Chưa có review độc lập, buổi nghiệm thu nhóm hoặc tag phát hành chính thức. Không ghi nhận giờ công thực tế hoặc thay đổi workbook của người dùng.
- Lịch tự hết hạn cần web đang chạy; job mỗi 30 giây và xử lý bù khi khởi động. Bảo vệ hạn xác nhận được áp dụng ngay trong transaction, không chỉ chờ job.
- Outbox chống nhiều worker cùng nhận một thư; trường hợp crash sau SMTP nhận nhưng trước ghi SentAt vẫn có thể gửi lại.
