using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Validation;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NotWhiteSpaceAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is string text && !string.IsNullOrWhiteSpace(text);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DateRangeAttribute(string startProperty, string endProperty) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var start = validationContext.ObjectType.GetProperty(startProperty)?.GetValue(value);
        var end = validationContext.ObjectType.GetProperty(endProperty)?.GetValue(value);

        if (start is null || end is null)
        {
            return ValidationResult.Success;
        }

        if (start is IComparable comparableStart && comparableStart.CompareTo(end) > 0)
        {
            return new ValidationResult(
                ErrorMessage ?? $"{endProperty} must be greater than or equal to {startProperty}.",
                [endProperty]);
        }

        return ValidationResult.Success;
    }
}
