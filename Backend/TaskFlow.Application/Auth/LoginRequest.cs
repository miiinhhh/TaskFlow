namespace TaskFlow.Application.Auth;

public sealed record LoginRequest(string UsernameOrEmail, string Password);
