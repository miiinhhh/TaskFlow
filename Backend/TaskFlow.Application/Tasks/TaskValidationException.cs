namespace TaskFlow.Application.Tasks;

public sealed class TaskValidationException(string message) : Exception(message);