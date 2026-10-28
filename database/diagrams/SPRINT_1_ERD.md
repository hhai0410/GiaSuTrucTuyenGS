# ERD Sprint 1

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : has
    AspNetRoles ||--o{ AspNetUserRoles : assigned
    AspNetUsers ||--o| TutorProfiles : owns
    TutorProfiles ||--o{ TutorSubjects : teaches
    Subjects ||--o{ TutorSubjects : classified
    TutorProfiles ||--o{ TutorReviews : summary
    TutorProfiles ||--o{ AvailabilitySlots : summary
    AspNetUsers {
        string Id PK
        string Email UK
        string PasswordHash
        bool EmailConfirmed
        datetime LockoutEnd
        int AccessFailedCount
        string FullName
    }
    AspNetRoles {
        string Id PK
        string Name UK
    }
    AspNetUserRoles {
        string UserId PK,FK
        string RoleId PK,FK
    }
    TutorProfiles {
        int Id PK
        string UserId FK,UK
        string Introduction
        decimal HourlyRate
        int ExperienceYears
        int Status
        string RejectionReason
        string PhotoKey
        string CertificateKey
        string Revision
    }
    Subjects {
        int Id PK
        string Name UK
    }
    TutorSubjects {
        int TutorProfileId PK,FK
        int SubjectId PK,FK
    }
    TutorReviews {
        int Id PK
        int TutorProfileId FK
        int Stars
        string Comment
    }
    AvailabilitySlots {
        int Id PK
        int TutorProfileId FK
        datetime StartsAtUtc
        datetime EndsAtUtc
        bool IsBooked
    }
```

Identity còn tạo bảng claim, login ngoài và token theo cấu trúc chuẩn của thư viện. Email chuẩn hóa có unique index. Một tài khoản chỉ có tối đa một hồ sơ gia sư. TutorSubjects dùng khóa ghép; không xóa môn đang được hồ sơ sử dụng.

`Revision` là concurrency token: ngăn ghi đè hoặc duyệt phiên bản hồ sơ đã bị thay đổi ở tab khác. `PhotoKey`/`CertificateKey` là tên tệp ngẫu nhiên, không phải đường dẫn công khai. Status: 0 Pending, 1 Approved, 2 Rejected.

TutorReviews/AvailabilitySlots hiện chỉ hỗ trợ đọc tóm tắt US06. Quan hệ với booking/học viên và quy tắc chống trùng/đánh giá sẽ hoàn thiện ở sprint tương ứng; chưa cho phép ghi từ giao diện.

Migration SQL Server: `src/GiaSuOnline.Web/Data/Migrations/*_InitialSprintOne.cs`; schema đã áp dụng bằng EF Core vào GiaSuOnlineGS.
