using System;

namespace Surging.Core.CPlatform.Validation
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ValidateAttribute : Attribute
    {
        public ValidateAttribute()
        {
            
        }
    }
}
