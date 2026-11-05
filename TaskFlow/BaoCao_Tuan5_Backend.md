# Bao cao tuan 5 - Backend TaskFlow

## 1. Noi dung da thuc hien

Trong tuan 5, phan backend cua TaskFlow tiep tuc duoc mo rong theo kien truc nhieu tang gom API, Application, Domain va Infrastructure.

### 1.1. Chuyen endpoint sang Controller

- Chuyen cac endpoint Minimal API sang ASP.NET Core Controller.
- Them cac controller chinh:
  - `AuthController`
  - `TasksController`
  - `ProjectsController`
  - `TaskAssignmentsController`
- Cau hinh `Program.cs` su dung:
  - `AddControllers()`
  - `MapControllers()`
  - Swagger UI de test API bang trinh duyet
  - HTTP local de thuan tien test bang Postman

### 1.2. Chuc nang Auth

- Hoan thien API dang nhap:
  - `POST /api/auth/login`
- Them chuc nang dang ky tai khoan:
  - `POST /api/auth/register`
- Khi dang ky, he thong:
  - Kiem tra trung username
  - Kiem tra trung email
  - Kiem tra role ton tai
  - Bam mat khau bang PBKDF2 truoc khi luu vao database
  - Tra ve access token sau khi dang ky thanh cong
- Khi dang nhap, he thong:
  - Tim user theo username hoac email
  - Kiem tra user dang active
  - Xac thuc mat khau
  - Tao JWT access token

### 1.3. Chuc nang Task

- Hoan thien cac API CRUD cho task:
  - `POST /api/tasks`
  - `GET /api/tasks`
  - `GET /api/tasks/{id}`
  - `PUT /api/tasks/{id}`
  - `DELETE /api/tasks/{id}`
- Viec xoa task dang duoc xu ly theo huong soft delete bang cot `IsDeleted`.

### 1.4. Chuc nang Status

- Them entity, DTO, service va controller cho Task Status.
- Cac API da them:
  - `POST /api/statuses`
  - `GET /api/statuses`
  - `GET /api/statuses/{id}`
  - `PUT /api/statuses/{id}`
  - `DELETE /api/statuses/{id}`
- Status duoc sap xep theo `DisplayOrder`, phu hop voi bang `TaskStatuses` trong database.

### 1.5. Chuc nang Priority

- Them entity, DTO, service va controller cho Task Priority.
- Cac API da them:
  - `POST /api/priorities`
  - `GET /api/priorities`
  - `GET /api/priorities/{id}`
  - `PUT /api/priorities/{id}`
  - `DELETE /api/priorities/{id}`
- Priority duoc sap xep theo `Level`, phu hop voi bang `TaskPriorities`.

### 1.6. Chuc nang Comment

- Them entity `TaskComment`, DTO, service va controller cho binh luan task.
- Cac API da them:
  - `POST /api/tasks/{taskId}/comments`
  - `GET /api/tasks/{taskId}/comments`
  - `GET /api/comments/{id}`
  - `PUT /api/comments/{id}`
  - `DELETE /api/comments/{id}`
- Comment duoc xoa mem bang cot `IsDeleted`.

### 1.7. Chuc nang Task History

- Them entity `TaskHistory`, DTO, service va controller cho lich su thay doi task.
- Cac API da them:
  - `POST /api/tasks/{taskId}/histories`
  - `GET /api/tasks/{taskId}/histories`
  - `GET /api/task-histories/{id}`
- Lich su task luu cac thong tin:
  - Task lien quan
  - User thuc hien
  - Hanh dong
  - Gia tri cu
  - Gia tri moi
  - Thoi gian tao

## 2. Ket qua dat duoc

- Backend da co cac API can thiet cho cac nghiep vu chinh cua TaskFlow.
- Cac API duoc tach theo controller ro rang, de quan ly va de test bang Swagger/Postman.
- Tang Application chua DTO va interface service.
- Tang Infrastructure chua EF Core DbContext va service xu ly truy van database.
- Tang Domain chua cac entity anh xa voi bang trong SQL Server.
- Build solution thanh cong sau khi them cac chuc nang moi.

## 3. Kho khan va vuong mac

### 3.1. Dong bo voi database co san

Database da co san cac bang va rang buoc khoa ngoai, nen khi viet backend can anh xa dung ten bang, cot va quan he. Neu sai mapping, EF Core co the truy van sai hoac loi khi ghi du lieu.

### 3.2. Trung ten entity voi kieu he thong

Entity `TaskStatus` bi trung ten voi `System.Threading.Tasks.TaskStatus`, vi vay can chi ro namespace `TaskFlow.Domain.Entities.TaskStatus` trong mot so vi tri de tranh loi compile.

### 3.3. Xu ly rang buoc khoa ngoai

Mot so bang nhu `Tasks`, `TaskComments`, `TaskHistories` phu thuoc vao `Users`, `Projects`, `TaskStatuses`, `TaskPriorities`. Khi them du lieu can bat loi `DbUpdateException` de tra ve thong bao de hieu neu khoa ngoai khong ton tai.

### 3.4. Xoa du lieu

Mot so bang co cot `IsDeleted`, vi vay nen xoa mem thay vi xoa khoi database. Tuy nhien cac bang cau hinh nhu Status va Priority hien tai dang xoa that, nen khi Status/Priority dang duoc Task su dung thi database se chan xoa.

### 3.5. Validation chua tap trung

Validation hien tai chu yeu duoc viet thu cong trong controller. Cach nay de lam nhanh, nhung khi project lon hon se bi lap code va kho bao tri. Nen chuyen sang DataAnnotations hoac FluentValidation de validation tap trung hon.

## 4. Danh gia model binding va validation hien tai

### 4.1. Model binding

Project da co model binding cua ASP.NET Core Controller.

Bang chung:

- Cac controller deu gan `[ApiController]`.
- Request body duoc bind vao DTO nhu `LoginRequest`, `RegisterRequest`, `CreateTaskRequest`, `CreateCommentRequest`.
- Route parameter duoc bind tu URL, vi du `id`, `taskId`.
- Swagger da nhan duoc schema request/response cho cac DTO.

Ket luan: project da co model binding.

### 4.2. Validation

Project da co validation o muc co ban, nhung chu yeu la validation thu cong.

Vi du:

- Kiem tra username/password rong trong `AuthController`.
- Kiem tra title rong trong `TasksController`.
- Kiem tra name rong trong `StatusesController` va `PrioritiesController`.
- Kiem tra content rong trong `CommentsController`.
- Kiem tra action rong trong `TaskHistoriesController`.
- Kiem tra `taskId` tren route co khop voi `TaskId` trong body.

Tuy nhien, DTO hien tai chua dung cac attribute validation nhu:

- `[Required]`
- `[StringLength]`
- `[Range]`
- `[EmailAddress]`

Ket luan: project da co validation thu cong, nhung chua co validation chuan bang DataAnnotations/FluentValidation.

## 5. Huong phat trien tiep theo

- Them DataAnnotations vao DTO de ASP.NET Core tu dong validate request.
- Chuan hoa response loi bang `ProblemDetails`.
- Them authentication middleware de bao ve cac API can dang nhap.
- Tu dong ghi Task History khi tao, cap nhat, xoa task thay vi bat frontend goi API history rieng.
- Bo sung unit test hoac integration test cho cac API chinh.
