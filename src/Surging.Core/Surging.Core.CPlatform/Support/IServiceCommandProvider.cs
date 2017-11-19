using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support
{
    public interface IServiceCommandProvider
    {
        Task<ServiceCommand> GetCommand(string serviceId);
        Task<object> Run(string text, params string[] InjectionNamespaces);
    }
}
