# Báo cáo EF Core và Migration - TaskFlow

**Ngày:** 02/10/2026  
**Phạm vi:** Backend .NET 10, EF Core SQL Server và cơ sở dữ liệu `TaskFlowDb`

## 1. Vì sao nhóm chọn nội dung này?

Nhóm chọn EF Core và migration vì đây là lớp kết nối trực tiếp giữa nghiệp vụ TaskFlow và dữ liệu bền vững. Đồ án có các đối tượng liên kết nhiều tầng như `User`, `Role`, `Project`, `ProjectMember`, `Task`, `TaskAssignment`, trạng thái và độ ưu tiên. Nếu truy cập cơ sở dữ liệu bằng SQL rời rạc ở từng API, mã nguồn sẽ dễ lặp lại, khó kiểm soát quan hệ và khó duy trì khi schema thay đổi.

EF Core giúp nhóm:

- Biểu diễn bảng bằng entity C# và truy vấn bằng LINQ.
- Kiểm tra quan hệ, khóa chính, khóa ngoại và hành vi xóa ở một nơi.
- Tách lớp persistence khỏi controller và application service.
- Có khả năng tiến hóa schema có kiểm soát thông qua migration khi dự án chuyển sang quy trình migration của EF Core.
- Hỗ trợ truy vấn bất đồng bộ, `AsNoTracking()` cho dữ liệu chỉ đọc và parameterization mặc định để giảm rủi ro SQL injection.

Nội dung này phù hợp với bài toán vì TaskFlow không chỉ lưu dữ liệu đơn lẻ mà còn phải kiểm tra quyền theo project, thành viên và người được giao task.

## 2. Cơ chế kỹ thuật thật bên dưới là gì?

### 2.1. EF Core DbContext và provider SQL Server

`TaskFlowDbContext` kế thừa `DbContext` và khai báo các `DbSet` cho domain model. Trong `DependencyInjection`, context được đăng ký với SQL Server provider:

```csharp
services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

Khi service gọi `ToListAsync`, `SingleAsync`, `AnyAsync` hoặc `SaveChangesAsync`, EF Core thực hiện các công việc chính sau:

1. Phân tích biểu thức LINQ thành cây truy vấn.
2. Chuyển cây truy vấn thành SQL Server SQL có parameter.
3. Gửi SQL qua provider `Microsoft.EntityFrameworkCore.SqlServer`.
4. Materialize kết quả thành entity hoặc DTO projection.
5. Theo dõi thay đổi entity trong `ChangeTracker` nếu entity đang được tracking.
6. Khi gọi `SaveChangesAsync`, phát sinh `INSERT`, `UPDATE` hoặc `DELETE` trong transaction mặc định của EF Core.

### 2.2. Mapping entity và quan hệ

Các mapping quan trọng nằm trong [TaskFlowDbContext.cs](../Backend/TaskFlow.Infrastructure/Persistence/TaskFlowDbContext.cs):

| Thành phần | Mapping kỹ thuật | Ý nghĩa |
|---|---|---|
| `Projects` | `OwnerId` -> `Users.Id`, `Restrict` | Không cho xóa user nếu còn project sở hữu |
| `ProjectMembers` | khóa ghép `(ProjectId, UserId)` | Một user không thể được thêm trùng vào project |
| `ProjectMembers` | project -> member, `Cascade` | Xóa project sẽ xóa membership liên quan |
| `ProjectMembers` | user -> membership, `Restrict` | Không xóa user đang tham gia project |
| `Tasks` | khóa ngoại tới project, creator, status, priority | Bảo đảm task tham chiếu dữ liệu hợp lệ |
| `TaskAssignments` | khóa ghép `(TaskId, UserId)` | Tránh assignment trùng |
| `TaskAssignments` | task `Cascade`, user/assignee `Restrict` | Xóa task dọn assignment, nhưng bảo vệ dữ liệu user |
| `Tasks.EstimatedHours` | `decimal(10,2)` | Giữ độ chính xác cho số giờ |

### 2.3. Truy vấn và hiệu năng

Service dùng projection trực tiếp sang DTO `ProjectResponse` và `TaskResponse`, thay vì trả entity EF Core ra API. Các truy vấn đọc danh sách dùng `AsNoTracking()` để không tạo chi phí theo dõi thay đổi không cần thiết.

Database script tạo index cho các cột thường dùng để lọc hoặc join:

- `Projects.OwnerId`
- `ProjectMembers.UserId`
- `Tasks.ProjectId`, `StatusId`, `PriorityId`, `DueDate`
- `TaskAssignments.UserId`
- `TaskComments.TaskId`, `TaskHistories.TaskId`
- `Notifications.UserId`, `IsRead`

### 2.4. Migration trong trạng thái hiện tại

Cần phân biệt hai khái niệm:

- **EF Core mapping/runtime:** Đã được sử dụng trong backend.
- **EF Core migration:** Chưa được commit trong repository.

Hiện repository chưa có thư mục `Migrations`, chưa có migration snapshot, chưa thấy package `Microsoft.EntityFrameworkCore.Design` hoặc `dotnet ef`, và `Program.cs` chưa gọi `Database.Migrate()`. Schema hiện tại được tạo bằng bốn script thủ công:

- [01_CreateDatabase.sql](01_CreateDatabase.sql)
- [02_CreateTables.sql](02_CreateTables.sql)
- [03_CreateIndexes.sql](03_CreateIndexes.sql)
- [04_SeedData.sql](04_SeedData.sql)

Vì vậy, trong báo cáo này, “migration” được đánh giá ở góc độ quản lý thay đổi schema và khả năng chuyển đổi sang EF migration, không khẳng định dự án đã chạy EF migration tự động.

## 3. Nhóm đã thử, viết và chạy những gì?

### 3.1. Đã viết

Nhóm đã viết và tích hợp:

- `TaskFlowDbContext` với 8 `DbSet` cho project, user, role, task, status, priority, membership và assignment.
- Fluent API cho khóa ghép, khóa ngoại, độ dài chuỗi, precision và delete behavior.
- `ProjectService` cho CRUD project, quản lý thành viên và assignment task.
- `TaskService` sử dụng EF Core async query và kiểm tra phạm vi project.
- `ProjectsController`, `TaskAssignmentsController` và `TasksController`.
- JWT authentication và role-based authorization ở ASP.NET Core pipeline.

### 3.2. Đã chạy

Nhóm đã chạy build với target framework `net10.0` cho project API và dependency project:

```powershell
dotnet build .\Backend\TaskFlow.API\TaskFlow.API.csproj `
  --configuration Debug `
  --framework net10.0
```

Nhóm cũng đã rà soát schema SQL, mapping EF Core và các đường đi quan trọng:

1. Tạo project và tự thêm owner vào `ProjectMembers`.
2. Thêm user vào project bằng khóa ghép `(ProjectId, UserId)`.
3. Gán task chỉ khi user được gán là thành viên của project.
4. Lọc task của user theo project ownership hoặc assignment.
5. Không cho member tự chuyển task sang project khác qua payload update.

Trong trạng thái repository hiện tại chưa có test project hoặc migration test tự động. Do đó việc kiểm chứng đã thực hiện ở mức compile/build và kiểm tra code path; chưa phải là benchmark runtime hoặc integration test với SQL Server đang chạy.

## 4. Kết quả hoặc số liệu cho thấy điều gì?

| Hạng mục | Kết quả quan sát được | Ý nghĩa |
|---|---:|---|
| Target framework | `.NET 10.0` | Backend sử dụng runtime hiện đại |
| EF Core package | `10.0.12` | Đồng bộ với target framework |
| SQL Server provider | `Microsoft.EntityFrameworkCore.SqlServer 10.0.12` | Có thể thực thi LINQ trên SQL Server |
| `DbSet` được khai báo | 8 | Các aggregate chính đã có lớp truy cập dữ liệu |
| Bảng trong schema | 14 | Bao phủ user, project, task, notification và token |
| Index trong script | 11 | Có index cho các khóa lọc/join chính |
| Khóa ghép | 2 | `ProjectMembers` và `TaskAssignments` chống bản ghi trùng ở database |
| Build API | Thành công | Code và dependency hiện tại biên dịch được |
| Benchmark latency/throughput | Chưa đo | Chưa đủ dữ liệu để kết luận hiệu năng runtime |
| EF migration files | 0 | Schema hiện đang được quản lý bằng SQL script thủ công |

Các con số trên cho thấy thiết kế dữ liệu đã có ràng buộc ở database, không chỉ kiểm tra bằng code. Tuy nhiên, chưa thể kết luận SQL query nhanh bao nhiêu mili-giây, throughput bao nhiêu request/giây hoặc index cải thiện bao nhiêu phần trăm vì nhóm chưa chạy benchmark trên dữ liệu đủ lớn.

Một warning dependency đã xuất hiện khi build: `Microsoft.OpenApi 2.3.0` có cảnh báo `NU1903`. Warning này không làm build thất bại nhưng cần được xử lý trước khi triển khai.

## 5. Nội dung này được dùng ở đâu trong đồ án?

EF Core được dùng trực tiếp trong backend TaskFlow tại:

- Auth: đọc user, role và kiểm tra đăng nhập.
- Project: tạo, sửa, archive và xem project.
- ProjectMember: kiểm soát user thuộc project.
- Task: tạo, đọc, sửa, xóa mềm và lọc task theo quyền.
- TaskAssignment: gán hoặc đổi người thực hiện task.
- Authorization: dùng dữ liệu từ project membership và assignment để giới hạn phạm vi thao tác.

Các script SQL hiện được dùng để khởi tạo môi trường SQL Server ban đầu. Chúng là cách triển khai schema hiện tại, còn `TaskFlowDbContext` là lớp runtime mapping và truy vấn.

### Kết luận về migration

Nếu nhóm cần triển khai nhiều môi trường hoặc thay đổi schema thường xuyên, bước tiếp theo nên là chuyển phần schema sang EF Core migration:

```powershell
dotnet add .\Backend\TaskFlow.Infrastructure\TaskFlow.Infrastructure.csproj `
  package Microsoft.EntityFrameworkCore.Design

dotnet ef migrations add InitialCreate `
  --project .\Backend\TaskFlow.Infrastructure `
  --startup-project .\Backend\TaskFlow.API

dotnet ef database update `
  --project .\Backend\TaskFlow.Infrastructure `
  --startup-project .\Backend\TaskFlow.API
```

Sau khi chuyển đổi, không nên chạy đồng thời hai nguồn thay đổi schema mà không có quy ước rõ ràng. Nhóm cần chọn một nguồn chuẩn: EF migration hoặc SQL migration được version hóa.

## 6. Bảo mật

- Connection string nằm trong configuration và không nên commit secret production.
- API project dùng JWT Bearer; secret, issuer, audience và thời hạn token được kiểm tra.
- Endpoint project/task yêu cầu `[Authorize]`.
- Role `Admin` và `Manager` được dùng cho thao tác quản lý project.
- Member chỉ thấy task thuộc project mình sở hữu hoặc task được assign.
- User ID lấy từ JWT claim `NameIdentifier`, không tin `CreatedById` từ request body.
- EF Core parameterize query LINQ mặc định, giảm rủi ro SQL injection.
- Password không được lưu plaintext; auth service dùng password hash/verifier.
- Cần thay secret JWT mặc định trong `appsettings.json` trước production và dùng Secret Manager/Key Vault.

## 7. Idempotency và khả năng phục hồi

### Idempotency

- Thêm membership trùng bị chặn bởi khóa ghép database và kiểm tra tồn tại trong service.
- Assignment được thay thế theo task: các assignment cũ bị xóa rồi tạo assignment mới.
- `PUT` có tính chất phù hợp với việc gọi lại cùng một trạng thái.
- Tạo project và membership owner hiện đang lưu qua hai lần `SaveChangesAsync`. Để bảo đảm nguyên tử tuyệt đối, nên gom vào một transaction hoặc một lần `SaveChangesAsync`; đây là điểm cần cải thiện nếu xảy ra lỗi giữa hai bước.

### Khả năng phục hồi

- Các thao tác database dùng async và truyền `CancellationToken`.
- Exception middleware trả về response JSON thống nhất khi có lỗi chưa xử lý.
- Foreign key và delete behavior giúp ngăn dữ liệu mồ côi.
- Chưa có retry policy cho lỗi tạm thời của SQL Server, chưa có health check database và chưa có chiến lược backup/restore được tự động hóa trong repository.

## 8. Testing và observability

### Đã có

- Compile/build backend với `net10.0` thành công ở project API.
- Logging exception có method, path và trace identifier trong `ExceptionHandlingMiddleware`.
- Swagger/OpenAPI được bật để kiểm tra thủ công endpoint.

### Cần bổ sung

- Integration test với SQL Server/Testcontainers cho CRUD project, membership và assignment.
- Test authorization cho Admin, Manager, Member và user không thuộc project.
- Test migration trên database trống và database đã có dữ liệu.
- Đo latency p50/p95, throughput và error rate khi dữ liệu tăng.
- Health check cho API và SQL Server.
- Structured logging cho `projectId`, `taskId`, `userId`, operation và kết quả authorization.
- Migration rollback test hoặc quy trình backup trước khi update production.

## 9. Khả năng tiếp cận

EF Core là lớp backend nên không trực tiếp quyết định accessibility của giao diện. Tuy vậy, API nên duy trì response JSON có tên trường rõ ràng, status code chuẩn và lỗi có cấu trúc để frontend có thể hiển thị thông báo, trạng thái validation và quyền truy cập một cách nhất quán.

## 10. Kết luận

EF Core đang đóng vai trò ORM và lớp truy cập dữ liệu chính của TaskFlow. Mapping hiện tại đã thể hiện được các quan hệ quan trọng và database script đã có khóa ngoại, khóa ghép và index nền tảng. Build thành công cho thấy phần tích hợp compile ổn định.

Tuy nhiên, dự án hiện chưa sử dụng EF Core migration thực sự; schema vẫn được khởi tạo bằng SQL script thủ công. Kết luận của nhóm là thiết kế hiện tại phù hợp cho prototype hoặc môi trường phát triển, nhưng cần chuẩn hóa một nguồn migration duy nhất, bổ sung integration test, retry/health check, benchmark và observability trước khi xem là quy trình production-ready.
