using System.Reflection;

namespace Surging.Core.CPlatform.Validation
{
    public interface IValidationProcessor
    {
        void Validate(ParameterInfo parameterInfo, object value);
    }
}
