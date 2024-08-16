
using System;

/// <summary>Requires that a field be non-null and, where a value type, not contain the default value </summary>
/// <remarks>Based on https://andrewlock.net/creating-an-empty-guid-validation-attribute/ </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RequiresValueAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
{
    public const string DefaultErrorMessage = "The {0} field must have a non-default value";
    public RequiresValueAttribute() : base(DefaultErrorMessage) { }

    public override bool IsValid(object value)
    {
        if (value is null)
            return false;

        var type = value.GetType();
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return !value.Equals(defaultValue);
        }
        else if(type == typeof(string))
            return !string.IsNullOrEmpty((string)value);

        return true;
    }
}