using System.ComponentModel.DataAnnotations;

namespace MhpdCommon.Models.OpenApi;

[AttributeUsage(AttributeTargets.Property)]
public class FixedValueAttribute(object expectedValue) : ValidationAttribute
{
    public object ExpectedValue { get; } = expectedValue;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (object.Equals(value, ExpectedValue))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The value of '{validationContext.MemberName}' must be '{ExpectedValue}'.");
    }
}
