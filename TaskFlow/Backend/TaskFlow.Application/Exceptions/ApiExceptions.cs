namespace TaskFlow.Application.Exceptions;

// 400
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}

// 401
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Authentication is required or the token is invalid.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }
}

// 403
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to access this resource.")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }
}

// 404
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

// 409
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
