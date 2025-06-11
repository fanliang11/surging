using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public class ObjectAttribute: ParameterBinder
    {
        public Type TargetType { get; set; }

        public override object Resolve(object value)
        {
            if (TargetType == null || value == null)
            {
                return value;
            }

            if (TargetType == value.GetType())
            {
                return value;
            }

            if (TargetType.IsInstanceOfType(value))
            {
                return value;
            }

            return null;
        }
    }
}
