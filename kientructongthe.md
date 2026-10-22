# Kiến trúc tổng thể TaskFlow

## 1. Tổng quan

TaskFlow là hệ thống quản lý dự án và công việc, gồm:

- Frontend: React + Vite
- Backend: ASP.NET Core Web API
- Database: SQL Server
- ORM: Entity Framework Core
- Authentication: JWT
- Password hashing: PBKDF2

## 2. Kiến trúc Backend

Backend được chia thành các tầng:

### TaskFlow.API

Chịu trách nhiệm:

- Controller
- HTTP request/response
- JWT authentication
- Authorization
- Swagger
- Middleware xử lý lỗi
- CORS

### TaskFlow.Application

Chứa:

- DTO
- Request model
- Response model
- Service interface
- Validation model

### TaskFlow.Domain

Chứa các entity nghiệp vụ:

- User
- Role
- Project
- Task
- TaskStatus
- TaskPriority
- TaskComment
- TaskHistory
- TaskAssignment

### TaskFlow.Infrastructure

Chịu trách nhiệm:

- Entity Framework Core
- DbContext
- Kết nối SQL Server
- Service implementation
- Password hashing
- JWT token generation
- Database migration

### TaskFlow.Tests

Chứa unit test cho:

- Authentication
- Password hashing
- Auth service
- Validation nghiệp vụ

## 3. Luồng xử lý request

```text
Frontend
   ↓
Controller
   ↓
Application interface
   ↓
Infrastructure service
   ↓
DbContext
   ↓
SQL Server