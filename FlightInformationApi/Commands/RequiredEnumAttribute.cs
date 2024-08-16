using System;

namespace FlightInformationApi.Commands;

/// <summary>Validate enums on models</summary>
/// <remarks>Enums aren't validated as valid values by asp.net 
/// https://stackoverflow.com/questions/54202864/enum-as-required-field-in-asp-net-core-webapi
/// </remarks>
public class RequiredEnumAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null) return false;
        var type = value.GetType();
        return type.IsEnum && Enum.IsDefined(type, value);
    }
}