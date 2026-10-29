# Authentication và Authorization - TaskFlow

**Phạm vi:** Backend .NET 10 của TaskFlow  
**Tài liệu:** Phần báo cáo độc lập, không chỉnh sửa báo cáo tuần 5  
**Ngày:** 09/10/2026

## 1. Vì sao nhóm chọn nội dung này?

TaskFlow có nhiều dữ liệu gắn với người dùng và project: tài khoản, vai trò, project, thành viên project, task và người được giao task. Vì vậy API không chỉ cần biết request đến từ ai mà còn phải kiểm tra người đó được phép làm gì trên từng project hoặc task.

Nhóm chọn Authentication và Authorization để giải quyết hai câu hỏi khác nhau:

- **Authentication:** Người đang gọi API là tài khoản nào? Mật khẩu có đúng không? Token có hợp lệ và còn hạn không?
- **Authorization:** Tài khoản đã đăng nhập có được phép thực hiện thao tác này không? Người dùng có thuộc project không? Có phải owner, manager hoặc người được giao task không?

Đây là nội dung phù hợp trực tiếp với bài toán TaskFlow vì nếu chỉ kiểm tra đăng nhập ở mức chung, một user có thể nhìn thấy hoặc sửa dữ liệu của project khác. Phân quyền theo project giúp giới hạn quyền ở đúng phạm vi nghiệp vụ thay vì chỉ dựa vào một role toàn hệ thống.

## 2. Cơ chế kỹ thuật thật bên dưới là gì?

### 2.1. Authentication bằng tài khoản và mật khẩu

API cung cấp hai endpoint:

- `POST /api/auth/register`
- `POST /api/auth/login`

Khi đăng ký, `AuthController` kiểm tra các trường bắt buộc và độ dài mật khẩu tối thiểu 6 ký tự. `AuthService` tiếp tục kiểm tra username và email không bị trùng, kiểm tra role tồn tại, sau đó băm mật khẩu trước khi lưu.

Mật khẩu không được lưu dạng plaintext. `PasswordHasher` sử dụng PBKDF2 với SHA-256:

- Salt ngẫu nhiên: 16 byte.
- Kích thước hash: 32 byte.
- Số vòng lặp: 100.000.
- Chuỗi lưu trữ có dạng: `PBKDF2$iterations$salt$hash`.

Khi đăng nhập, `PasswordVerifier` đọc cấu trúc hash, tính lại PBKDF2 với salt và số vòng lặp đã lưu, sau đó dùng `CryptographicOperations.FixedTimeEquals` để so sánh hash. So sánh constant-time giúp giảm nguy cơ lộ thông tin qua thời gian xử lý khác nhau.

Tài khoản phải có `IsActive = true` mới được đăng nhập. User có thể đăng nhập bằng username hoặc email.

### 2.2. Tạo và kiểm tra JWT access token

Sau khi xác thực mật khẩu thành công, hệ thống tạo JWT chứa các claim chính:

| Claim | Mục đích |
|---|---|
| `NameIdentifier` | ID của user hiện tại |
| `Name` | Username |
| `Email` | Email |
| `Role` | Tên role dùng cho role authorization |
| `roleId` | ID role |
| `fullName` | Tên hiển thị |
| `iss` | Issuer của token |
| `aud` | Audience của token |
| `iat` | Thời điểm phát hành |
| `exp` | Thời điểm hết hạn |

`JwtTokenGenerator` tự tạo header và payload, mã hóa Base64URL, sau đó ký chuỗi token bằng HMAC-SHA256. Token được trả về theo dạng Bearer token cùng thời điểm hết hạn.

Trong `Program.cs`, ASP.NET Core đăng ký JWT Bearer authentication với các điều kiện:

- Kiểm tra chữ ký bằng secret key.
- Kiểm tra `Issuer`.
- Kiểm tra `Audience`.
- Kiểm tra thời hạn token.
- Cho phép sai lệch đồng hồ tối đa 1 phút (`ClockSkew`).

Pipeline xử lý theo thứ tự:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

`UseAuthentication()` đọc header `Authorization: Bearer <token>` và tạo `HttpContext.User`. `UseAuthorization()` sau đó đánh giá các thuộc tính `[Authorize]` hoặc role requirement trên endpoint.

### 2.3. Authorization theo role

API dùng role claim trong JWT để phân quyền ở mức hệ thống. Ví dụ endpoint tạo project yêu cầu:

```csharp
[Authorize(Roles = "Admin,Manager")]
```

Điều này có nghĩa request phải có token hợp lệ và claim role phải là `Admin` hoặc `Manager`.

Các endpoint project, task và assignment yêu cầu `[Authorize]`, nghĩa là user phải đăng nhập trước. Với thao tác sâu hơn, service đọc `NameIdentifier` từ JWT để lấy `currentUserId` và không tin ID người dùng do client gửi lên.

### 2.4. Authorization theo project và task

Role không đủ để quyết định quyền vì hai manager khác nhau có thể quản lý hai project khác nhau. `ProjectService` kiểm tra thêm dữ liệu trong database:

- Admin có thể truy cập mọi project.
- Manager chỉ quản lý project mình sở hữu.
- User chỉ thấy project mà mình là thành viên.
- Owner được tự động thêm vào `ProjectMembers` khi tạo project.
- Chỉ user active mới được thêm vào project.
- Không thể xóa owner khỏi project.
- Chỉ thành viên project mới có thể được assign task.
- User thường chỉ thấy task thuộc project mình sở hữu hoặc task được assign cho mình.
- User thường không được chuyển task sang project khác khi update.

Như vậy, hệ thống kết hợp hai lớp:

1. **Role-based authorization:** Admin/Manager/Member.
2. **Resource-based authorization:** owner, membership và assignment của từng project/task.

## 3. Nhóm đã thử, viết và chạy những gì?

### 3.1. Đã viết

Nhóm đã triển khai các thành phần sau:

- `AuthController` với register và login.
- `AuthService` truy vấn user/role bằng EF Core.
- `PasswordHasher` và `PasswordVerifier` dùng PBKDF2.
- `JwtTokenGenerator` tạo access token HMAC-SHA256.
- JWT Bearer authentication trong `Program.cs`.
- `[Authorize]` trên các controller cần đăng nhập.
- Role restriction cho việc tạo project.
- Kiểm tra user hiện tại từ claim `ClaimTypes.NameIdentifier`.
- Kiểm tra project owner trong `ProjectService`.
- Kiểm tra membership trước khi add member, xem project và assign task.
- Kiểm tra assignment khi user đọc hoặc sửa task.

### 3.2. Đã kiểm tra theo luồng

Các luồng chính cần kiểm tra bằng Swagger hoặc Postman:

1. Đăng ký user mới với username/email hợp lệ.
2. Đăng nhập bằng username hoặc email và nhận Bearer token.
3. Gọi API được bảo vệ không có token để xác nhận bị từ chối.
4. Gọi API với token hợp lệ để xác nhận `HttpContext.User` có claims.
5. User không phải Admin/Manager thử tạo project và bị từ chối.
6. Manager tạo project và owner được thêm vào `ProjectMembers`.
7. Manager assign task cho user không thuộc project và bị từ chối.
8. Manager assign task cho thành viên hợp lệ và thành công.
9. User đọc task được giao thành công nhưng không đọc task ngoài phạm vi.
10. Token hết hạn hoặc sai signature bị từ chối.

Build backend đã được thực hiện với target framework `net10.0`. Tuy nhiên repository hiện chưa có test project tự động cho authentication/authorization, nên các luồng trên cần được bổ sung thành integration test để có bằng chứng lặp lại được.

## 4. Kết quả hoặc số liệu cho thấy điều gì?

| Hạng mục | Kết quả hiện tại | Ý nghĩa |
|---|---:|---|
| Authentication endpoint | 2 | Có register và login |
| Cơ chế băm mật khẩu | PBKDF2-SHA256 | Không lưu plaintext password |
| Số vòng PBKDF2 | 100.000 | Tăng chi phí đoán mật khẩu so với hash đơn giản |
| Salt mỗi password | 16 byte ngẫu nhiên | Hai mật khẩu giống nhau không nhất thiết có cùng chuỗi hash |
| Access token | JWT Bearer | Client gửi token ở header Authorization |
| Thuật toán ký token | HMAC-SHA256 | Phát hiện token bị sửa nếu secret được bảo vệ |
| Kiểm tra JWT | Signature, issuer, audience, lifetime | Token sai hoặc hết hạn bị từ chối |
| Role chính | Admin, Manager, Member | Phân quyền mức hệ thống |
| Phạm vi tài nguyên | Owner, member, assignee | Phân quyền theo project/task |
| Build API | Thành công trong lần kiểm tra trước | Code authentication/authorization biên dịch được |
| Benchmark latency/error rate | Chưa đo | Chưa đủ dữ liệu để kết luận hiệu năng |

Các kết quả trên cho thấy backend đã có chuỗi xác thực hoàn chỉnh từ mật khẩu đến token và có lớp kiểm tra quyền theo tài nguyên. Tuy nhiên, chưa có số đo latency p50/p95, throughput hoặc tỷ lệ lỗi theo tải. Vì vậy báo cáo không kết luận rằng hệ thống đã tối ưu hiệu năng; mới chỉ kết luận rằng cơ chế đã được tích hợp và build được.

## 5. Nội dung này được dùng ở đâu trong đồ án?

Authentication và Authorization được dùng trực tiếp trong các nghiệp vụ:

- Đăng ký và đăng nhập tài khoản.
- Bảo vệ API project, task, member và assignment.
- Xác định user hiện tại khi tạo project hoặc task.
- Giới hạn Admin/Manager trong các thao tác quản lý.
- Giới hạn user thường theo membership và assignment.
- Ngăn client giả mạo `CreatedById` hoặc tự assign task cho người ngoài project.

Nếu tách riêng thành thí nghiệm, nội dung này giúp nhóm kết luận rằng role toàn cục không đủ cho ứng dụng quản lý project. Cần kết hợp role authorization với resource authorization, trong đó quyền được xác định từ quan hệ giữa user, project và task.

## 6. Bảo mật và giới hạn hiện tại

### Điểm đã có

- Password được băm PBKDF2 với salt ngẫu nhiên.
- So sánh hash bằng constant-time comparison.
- JWT có issuer, audience, signature và expiry validation.
- API protected bằng `[Authorize]`.
- User ID lấy từ token thay vì tin hoàn toàn request body.
- Database membership và assignment được dùng cho authorization theo tài nguyên.

### Điểm cần cải thiện trước production

- Secret JWT hiện có giá trị mặc định trong cấu hình phát triển; production phải dùng secret mạnh từ Secret Manager, environment variable hoặc Azure Key Vault.
- `RegisterRequest` hiện nhận `RoleId` từ request. Public registration không nên cho client tự chọn role Admin/Manager; role đăng ký nên bị cố định thành Member hoặc chỉ cho admin quản lý.
- `PasswordVerifier` còn hỗ trợ password demo `password` hoặc `123456` cho dữ liệu seed. Cơ chế này chỉ được giữ ở môi trường demo và phải loại bỏ trong production.
- Nên thêm refresh token rotation, revoke token và logout nếu cần phiên đăng nhập dài hạn.
- Nên giới hạn số lần login thất bại, thêm lockout hoặc rate limiting để giảm password spraying.
- Nên dùng HTTPS bắt buộc trong production và không ghi access token/password vào log.
- Nên chuẩn hóa lỗi bằng `ProblemDetails`, tránh tiết lộ username/email nào đã tồn tại nếu yêu cầu threat model nghiêm ngặt.

## 7. Idempotency và khả năng phục hồi

- Login là thao tác đọc và tạo token, không tạo bản ghi nghiệp vụ mới nên có thể gọi lại an toàn.
- Register có kiểm tra username/email trùng và database có unique constraint, giúp tránh tạo tài khoản trùng trong các request thông thường.
- Thêm membership có kiểm tra tồn tại và khóa ghép `(ProjectId, UserId)`.
- Gán lại task sẽ thay thế assignment cũ bằng assignment mới.
- Tạo project hiện lưu project và owner membership qua hai lần `SaveChangesAsync`; nên gom vào transaction để nếu bước thứ hai lỗi thì không còn project thiếu membership owner.
- Các truy vấn async truyền `CancellationToken`, giúp dừng xử lý khi client ngắt kết nối.
- Chưa có retry policy cho lỗi SQL Server tạm thời, refresh-token persistence, audit log bảo mật hoặc cơ chế phát hiện token bị lạm dụng.

## 8. Testing và observability

### Nên kiểm thử tự động

- Login đúng mật khẩu và sai mật khẩu.
- Login user không active.
- Password hash bị hỏng hoặc sai format.
- Token hết hạn, sai issuer, sai audience và sai signature.
- Anonymous gọi endpoint có `[Authorize]`.
- Member gọi endpoint chỉ dành cho Admin/Manager.
- Manager truy cập project không sở hữu.
- User không thuộc project thử xem project hoặc nhận task.
- Assign task cho user ngoài project.
- Register cố gắng gửi `RoleId` của Admin.

### Nên quan sát trong runtime

- Log event login thành công/thất bại nhưng không log password hoặc token.
- Ghi `userId`, `projectId`, `taskId`, endpoint và kết quả authorization ở mức audit phù hợp.
- Theo dõi số lượng `401 Unauthorized` và `403 Forbidden`.
- Theo dõi latency p50/p95 của login và các endpoint protected.
- Thêm health check cho database và readiness check cho API.
- Đo error rate và throughput khi có nhiều request đồng thời.

## 9. Kết luận

TaskFlow đã triển khai được Authentication bằng PBKDF2 và JWT Bearer, đồng thời triển khai Authorization theo hai lớp: role hệ thống và quyền trên từng project/task. Cách kết hợp này phù hợp với bài toán vì một user có thể là thành viên của project này nhưng không có quyền trên project khác.

Kết quả hiện tại đủ cho môi trường học tập và prototype có kiểm soát. Để đạt mức production-ready, nhóm cần khóa quyền tự chọn role khi đăng ký, loại bỏ password demo, bảo vệ secret JWT, bổ sung refresh/revoke token, transaction cho các thao tác nhiều bước, integration test và số liệu runtime về latency, throughput, error rate.
