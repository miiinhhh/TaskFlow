namespace TaskFlow.Application.Exceptions;

/// <summary>
/// Trả về 400 Bad Request – dữ liệu đầu vào không hợp lệ.
/// </summary>
public sealed class BadRequestException(string message) : Exception(message);

/// <summary>
/// Trả về 401 Unauthorized – chưa xác thực hoặc token không hợp lệ.
/// </summary>
public sealed class UnauthorizedException(string message = "Authentication is required or the token is invalid.")
    : Exception(message);

/// <summary>
/// Trả về 403 Forbidden – đã xác thực nhưng không có quyền.
/// </summary>
public sealed class ForbiddenException(string message = "You do not have permission to access this resource.")
    : Exception(message);

/// <summary>
/// Trả về 404 Not Found – tài nguyên không tồn tại.
/// </summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>
/// Trả về 409 Conflict – xung đột dữ liệu (trùng lặp, v.v.).
/// </summary>
public sealed class ConflictException(string message) : Exception(message);
