using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskFlow.API.Filters;

public sealed class RouteParameterValidationFilter : IActionFilter
{
    private static readonly string[] PositiveIntegerParameters = ["id", "taskId", "projectId", "userId"];

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var parameterName in PositiveIntegerParameters)
        {
            if (context.RouteData.Values.TryGetValue(parameterName, out var rawValue)
                && int.TryParse(rawValue?.ToString(), out var value)
                && value <= 0)
            {
                context.ModelState.AddModelError(parameterName, $"{parameterName} must be greater than 0.");
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
