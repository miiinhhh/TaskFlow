# Ghi chú ảnh minh chứng cần bổ sung

Các ảnh dưới đây cần chụp từ đúng phiên bản mã nguồn hiện tại. Không dùng lại ảnh cũ nếu payload, quyền hoặc kết quả đã thay đổi.

## 1. Build, test và dependency

- Chạy `dotnet build Backend/TaskFlow.slnx` và chụp dòng `Build succeeded`.
- Chạy `dotnet test Backend/TaskFlow.slnx --no-restore` và chụp kết quả `Passed: 7, Failed: 0`.
- Chụp cảnh báo NuGet về `System.Security.Cryptography.Pkcs 9.0.11`; báo cáo đang ghi đây là hạn chế chưa xử lý, không được chụp theo hướng “0 cảnh báo”.

## 2. Migration và SQL Server

- Chụp thư mục `Backend/TaskFlow.Infrastructure/Migrations` có `20261011132137_InitialCreate.cs`, file Designer và `TaskFlowDbContextModelSnapshot.cs`.
- Tạo một database trống dùng riêng cho minh chứng, chạy `dotnet ef database update` và chụp lệnh thành công.
- Chụp bảng `__EFMigrationsHistory` có migration `20261011132137_InitialCreate`.
- Chụp một số bảng chính sau migration: `Users`, `Projects`, `ProjectMembers`, `Tasks`, `TaskAssignments`, `TaskComments`, `TaskHistories`.

## 3. Đăng ký, đăng nhập và vòng đời JWT

- Swagger/Postman: đăng ký tài khoản và cố gửi thêm `RoleId: 1`; chụp response cho thấy tài khoản vẫn có role `Member`.
- Đăng nhập đúng: chụp `200 OK`, token và `expiresAt`.
- Đăng nhập sai mật khẩu: chụp `401 Unauthorized`.
- Dùng tài khoản seed còn `DEMO_PASSWORD_HASH`: chụp đăng nhập bị `401`.
- Gọi API được bảo vệ khi không có token, token sai chữ ký và token hết hạn: chụp từng response `401`.
- Ghi chú cạnh ảnh: access token hết hạn sau 8 giờ; hiện chưa có refresh token, thu hồi token hoặc logout phía server.

## 4. Ma trận quyền Admin, Manager, Member

- Member gọi `POST /api/projects`: chụp `403 Forbidden`.
- Admin hoặc Manager gọi `POST /api/projects`: chụp `201 Created`.
- Manager cố cập nhật/xóa project không sở hữu: chụp kết quả bị từ chối theo endpoint hiện tại.
- Người ngoài project gọi API xem project/task: chụp `404` theo chính sách không làm lộ tài nguyên.
- Phân công task cho user không thuộc project: chụp `400 Bad Request`.
- Member chỉ xem được task đã được giao; chụp danh sách task của hai tài khoản để so sánh phạm vi.

## 5. Comment, History, Status và Priority

- Không token gọi Comment/History/Status/Priority: chụp `401`.
- Assignee tạo và xem comment trên task được giao: chụp thành công.
- User ngoài phạm vi task xem hoặc tạo comment/history: chụp bị từ chối.
- Member gọi `POST`, `PUT` hoặc `DELETE /api/statuses` và `/api/priorities`: chụp `403`.
- Admin thực hiện cùng thao tác: chụp `201`, `200` hoặc `204` tương ứng.
- Chụp payload mới: comment chỉ gửi `Content`; history chỉ gửi `Action`, `OldValue`, `NewValue`. Không gửi `TaskId` hoặc `UserId` trong body.

## 6. Model Binding và Validation

- Đăng ký với email sai và mật khẩu dưới 6 ký tự: chụp `400` cùng `ValidationProblemDetails`.
- Tạo task với `ProjectId = 0` hoặc `EstimatedHours = -1`: chụp `400`.
- Gửi `EstimatedHours` là chuỗi không chuyển được sang số: chụp lỗi model binding `400`.
- Tạo project/task có ngày kết thúc trước ngày bắt đầu: chụp lỗi `400` từ kiểm tra liên trường trong Controller.
- Gửi JSON sai cú pháp: chụp `400` và `traceId`.

## 7. Transaction tạo project

- Chụp đoạn `BeginTransactionAsync` và `CommitAsync` trong `ProjectService.CreateAsync`.
- Sau khi tạo project thành công, chụp hai bản ghi liên quan trong `Projects` và `ProjectMembers`, trong đó owner đồng thời là member.
- Minh chứng rollback khi bước thêm member thất bại cần một integration test SQL Server có chủ đích; hiện chưa nên ghi là đã kiểm thử nếu chưa thực hiện ca này.

## 8. Middleware, log và truy vấn EF Core

- Gọi một endpoint, chụp cặp log `Request started` và `Request completed` có cùng `TraceId`, status code và `ElapsedMs`.
- Chụp thứ tự trong `Program.cs`: `ExceptionHandlingMiddleware`, `RequestTraceMiddleware`, `UseAuthentication`, `UseAuthorization`, `MapControllers`.
- Gọi `GET /api/tasks` với dữ liệu mẫu và chụp log `Microsoft.EntityFrameworkCore.Database.Command`; đếm số câu SQL để làm bằng chứng đánh giá N+1.
- Lặp lại với số lượng task lớn hơn. Nếu số câu SQL tăng theo số task thì cần tối ưu trước khi kết luận không có N+1.

## 9. Phản hồi lỗi

- Chụp mẫu thực tế cho `400`, `401`, `403`, `404`, `500`.
- Lưu ý: lỗi model validation dùng `ValidationProblemDetails`, nhưng một số `BadRequest` thủ công và middleware ngoại lệ chưa cùng một schema. Báo cáo đã ghi đây là hạn chế còn lại.
