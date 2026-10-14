# Phân chia nhiệm vụ tuần này

## 1. Phân công cho 4 thành viên

| Thành viên | Nhiệm vụ chính | Công việc cụ thể |
|---|---|---|
| **Thành viên 1: Validation** | Validation và chuẩn hóa request | Kiểm tra trường bắt buộc, độ dài chuỗi, email, password, ngày bắt đầu/kết thúc, `ProjectId`, `TaskId`; bổ sung DataAnnotations hoặc validation tập trung. |
| **Thành viên 2: Exception & Response** | Exception Handling Middleware và API response | Chuẩn hóa JSON lỗi; phân biệt `400`, `401`, `403`, `404`, `409`, `500`; xử lý `DbUpdateException`, lỗi validation và lỗi không xác định. |
| **Thành viên 3: Security & Business Rules** | Authentication, Authorization và rà soát nghiệp vụ | Kiểm tra JWT; kiểm tra quyền `Admin/Manager/Member`; rà soát Project, ProjectMember, Task Assignment; sửa lỗi quyền user ngoài project hoặc assign task sai phạm vi. |
| **Thành viên 4: Testing & Documentation** | Kiểm thử và hoàn thiện tài liệu API | Kiểm thử bằng Swagger/Postman; tạo checklist test; ghi nhận và kiểm tra lỗi; cập nhật mô tả endpoint, request, response và quyền truy cập. |

## 2. Phân chia rà soát nghiệp vụ

Thành viên 3 phụ trách chính các nghiệp vụ:

- **Project:** tạo, sửa, xóa, xem danh sách và xem chi tiết.
- **Project Member:** thêm user, xóa user và kiểm tra thành viên.
- **Task Assignment:** gán task, đổi người được giao và kiểm tra membership.
- **Comment và History:** kiểm tra user có quyền thao tác trên task hay không.

Thành viên 4 hỗ trợ kiểm thử toàn bộ các nghiệp vụ trên bằng các kịch bản thành công và thất bại.

## 3. Thứ tự thực hiện

1. Thành viên 1 hoàn thiện validation.
2. Thành viên 2 chuẩn hóa exception và API response.
3. Thành viên 3 rà soát authentication, authorization và nghiệp vụ.
4. Thành viên 4 viết test trên Swagger/Postman và cập nhật tài liệu.
5. Cả nhóm tích hợp, build solution và kiểm thử regression lần cuối.

## 4. Checklist bàn giao cuối tuần

- API thiếu dữ liệu trả về `400 Bad Request`.
- Request không có token trả về `401 Unauthorized`.
- User không đủ quyền trả về `403 Forbidden`.
- Không tìm thấy dữ liệu trả về `404 Not Found`.
- Dữ liệu bị trùng hoặc vi phạm khóa trả về `409 Conflict`.
- Lỗi hệ thống trả về JSON thống nhất có `traceId`.
- JWT và role `Admin/Manager/Member` hoạt động đúng.
- User không thuộc project không thể xem hoặc nhận task ngoài phạm vi.
- Tất cả endpoint chính có request, response và quyền truy cập được mô tả.
- Solution build thành công sau khi tích hợp các thay đổi.
