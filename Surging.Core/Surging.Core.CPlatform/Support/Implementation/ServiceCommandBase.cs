using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
   public abstract class ServiceCommandBase: IServiceCommandProvider
    { 
        public abstract ServiceCommand GetCommand(string serviceId);
        ConcurrentDictionary<string,  object> scripts = new ConcurrentDictionary<string,  object>();

        public async Task<object> Run(string text, params string[] InjectionNamespaces)
        {
            object result = null;
            var scriptOptions = ScriptOptions.Default.WithImports("System.Threading.Tasks");
            if (InjectionNamespaces != null)
            {
                foreach (var injectionNamespace in InjectionNamespaces)
                {
                    scriptOptions = scriptOptions.WithReferences(injectionNamespace);
                }
            }
            if (!scripts.ContainsKey(text))
            {
                result = scripts.GetOrAdd(text, await CSharpScript.EvaluateAsync(text, scriptOptions));
            }
            return result;
        }
    }
}
