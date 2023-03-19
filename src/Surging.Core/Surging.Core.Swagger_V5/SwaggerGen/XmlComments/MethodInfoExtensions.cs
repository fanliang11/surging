using System.Linq;
using System.Reflection;

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public static class MethodInfoExtensions
    {
        public static MethodInfo GetUnderlyingGenericTypeMethod(this MethodInfo constructedTypeMethod)
        {
            var constructedType = constructedTypeMethod.DeclaringType;
            var genericTypeDefinition = constructedType.GetGenericTypeDefinition();

            // Retrieve list of candidate methods that match name and parameter count
            var candidateMethods = genericTypeDefinition.GetMethods()
                .Where(m =>
                {
                    return (m.Name == constructedTypeMethod.Name)
                        && (m.GetParameters().Length == constructedTypeMethod.GetParameters().Length);
                });


            // If inconclusive, just return null
            return (candidateMethods.Count() == 1) ? candidateMethods.First() : null;
        }
    }
}
