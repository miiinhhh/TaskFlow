using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Validation;

namespace TaskFlow.Application.Auth;

public sealed record LoginRequest(
    [param: Required, NotWhiteSpace, StringLength(255)] string UsernameOrEmail,
    [param: Required, NotWhiteSpace, StringLength(200)] string Password);
