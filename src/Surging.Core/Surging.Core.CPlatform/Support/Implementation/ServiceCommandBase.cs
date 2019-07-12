using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceCommandBase" />
    /// </summary>
    public abstract class ServiceCommandBase : IServiceCommandProvider
    {
        #region 字段

        /// <summary>
        /// Defines the scripts
        /// </summary>
        internal ConcurrentDictionary<string, object> scripts = new ConcurrentDictionary<string, object>();

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GetCommand
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{ServiceCommand}"/></returns>
        public abstract ValueTask<ServiceCommand> GetCommand(string serviceId);

        /// <summary>
        /// The Run
        /// </summary>
        /// <param name="text">The text<see cref="string"/></param>
        /// <param name="InjectionNamespaces">The InjectionNamespaces<see cref="string[]"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        public async Task<object> Run(string text, params string[] InjectionNamespaces)
        {
            object result = scripts;
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
            else
            {
                scripts.TryGetValue(text, out result);
            }
            return result;
        }

        #endregion 方法
    }
}