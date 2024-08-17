
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightInformationApi;

/// <summary>Validation of models that use .NET Validation Attributes</summary>
public interface IModelValidator
{
    /// <summary>Validate model which uses .NET Validation Attributes</summary>
    bool Validate(object o);
}

public class ModelValidator : IModelValidator
{
    public bool Validate(object toValidate)
    {
        ValidationContext vc = new ValidationContext(toValidate);
        List<ValidationResult> results = new List<ValidationResult>();
        return Validator.TryValidateObject(toValidate, vc, results, true);
    }
}
