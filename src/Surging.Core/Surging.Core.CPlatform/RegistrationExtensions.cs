using Surging.Core.CPlatform.Filters;
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform
{
    public static class RegistrationExtensions
    { 
        public static void AddFilter(this ContainerBuilderWrapper builder, Type filter)
        {
           
            if (typeof(IExceptionFilter).IsAssignableFrom(filter))
            {
                builder.RegisterType(filter).As<IExceptionFilter>().SingleInstance();
            }
            else if (typeof(IAuthorizationFilter).IsAssignableFrom(filter))
            {
                builder.RegisterType(filter).As<IAuthorizationFilter>().SingleInstance();
            }
        }

       
    }
}
