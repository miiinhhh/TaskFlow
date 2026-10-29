namespace TaskFlow.Application.Tasks;

public class TaskValidationException : Exception
{
    public TaskValidationException(string message) : base(message)
    {
    }
}
