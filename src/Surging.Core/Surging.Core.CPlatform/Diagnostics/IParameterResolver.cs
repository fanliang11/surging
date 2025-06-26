using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public interface IParameterResolver
    {
        object Resolve(object value);
    }
}
