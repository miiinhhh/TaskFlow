namespace TaskFlow.Application.Auth;

public sealed record RegisterResult(
    bool Succeeded,
    string? Error,
    LoginResponse? Login);
