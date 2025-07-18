using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Exceptions
{
    public class RegisterConnectionException : CPlatformException
    {
        public RegisterConnectionException(string message, Exception innerException = null) : base(message, innerException)
        {
        }
    }
}