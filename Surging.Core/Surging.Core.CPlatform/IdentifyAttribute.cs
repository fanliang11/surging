using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IdentifyAttribute : Attribute
    {
        public IdentifyAttribute(CommunicationProtocol name)
        {
            this.Name = name;
        }

        public CommunicationProtocol Name { get; set; }
    }
}

