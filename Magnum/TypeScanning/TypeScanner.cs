// Copyright 2007-2010 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.TypeScanning
{
    using System;
    using System.Reflection;
    using System.Linq;


    public class TypeScanner 
    {
        public static Type[] Scan(Action<TypeScanner> cfg)
        {
            var ts = new TypeScanner();
            cfg(ts);
            return ts.Execute();
        }

        readonly Scanner _scanner;

        TypeScanner()
        {
            _scanner = new Scanner();
        }

        public void Assembly(Assembly assembly)
        {
            _scanner.AddAssembly(assembly);
        }

        public void TheCallingAssembly()
        {
            _scanner.AddCallingAssembly();
        }

        public void AssemblyContainingType<T>()
        {
            _scanner.AddAssemblyContaining<T>();
        }

        public void AssemblyContainingType(Type type)
        {
            _scanner.AddAssemblyContaining(type);
        }

#if !SILVERLIGHT
        public void AssembliesFromPath(string path)
        {
            _scanner.AddAssembliesFromPath(path);
        }

        public void AssembliesFromPath(string path, Predicate<Assembly> assemblyFilter)
        {
            _scanner.AddAssembliesFromPath(path);
            _scanner.AssemblyFilters.Add(assemblyFilter);
        }
#endif


        public void AssembliesFromApplicationBaseDirectory()
        {
            _scanner.AddAssembliesFromBaseDirectory();
        }

        public void AssembliesFromApplicationBaseDirectory(Predicate<Assembly> assemblyFilter)
        {
            _scanner.AddAssembliesFromBaseDirectory();
            _scanner.AssemblyFilters.Add(assemblyFilter);
        }

        public void AddAllTypesOf<TPlugin>()
        {
            _scanner.TypeFilters.Add(t=>!t.IsConcreteAndAssignableTo(typeof(TPlugin)));
        }

        public void AddAllTypesOf(Type pluginType)
        {
            _scanner.TypeFilters.Add(t=>!t.IsConcreteAndAssignableTo(pluginType));
        }

        public void Exclude(Predicate<Type> exclude)
        {
            _scanner.TypeFilters.Add(exclude);
        }

        public void ExcludeNamespace(string nameSpace)
        {
            _scanner.TypeFilters.Add(x => x.Namespace.Contains(nameSpace));
        }

        public void ExcludeNamespaceContainingType<T>()
        {
            _scanner.TypeFilters.Add(x => x.Namespace.Contains(x.Namespace));
        }

        public void ExcludeType<T>()
        {
            _scanner.TypeFilters.Add(x => x.Equals(typeof(T)));
        }

        public Type[] Execute()
        {
            var filteredTypes = _scanner.GetMatchingTypes();

            return filteredTypes.ToArray();
        }
    }
}