
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightInformationApi;

public interface IModelValidator
{
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
