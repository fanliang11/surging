using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.CPlatform.Validation.Implementation
{
    public class DefaultValidationProcessor : IValidationProcessor
    {
        private readonly ITypeConvertibleService _typeConvertibleService;

        public DefaultValidationProcessor(ITypeConvertibleService typeConvertibleService)
        {
            _typeConvertibleService = typeConvertibleService;
        }

        public void Validate(ParameterInfo parameterInfo, object value)
        {
            Check.NotNull(parameterInfo, nameof(parameterInfo));
            if (value != null)
            {
                var parameterType = parameterInfo.ParameterType;
                var parameter = _typeConvertibleService.Convert(value, parameterType);
                var customAttributes = parameterInfo.GetCustomAttributes(true); 
                var customValidAttributes = customAttributes
                    .Where(ca => ca.GetType() != typeof(ValidateAttribute))
                    .OfType<ValidationAttribute>()
                    .ToList(); 
                var validationContext = new ValidationContext(parameter);
                var validationResults = new List<ValidationResult>();
                var isObjValid = Validator.TryValidateObject(parameter, validationContext,
                    validationResults,
                    true);

                var isValueValid = Validator.TryValidateValue(parameter, validationContext,
                    validationResults, customValidAttributes);

                if (isObjValid && isValueValid) return;

                throw new ValidateException(validationResults.Select(p => p.ErrorMessage).First());

            }
        }
    }
}
