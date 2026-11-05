namespace TaskFlow.Application.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    AuthUserResponse User);
