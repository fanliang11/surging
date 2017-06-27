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
namespace Magnum.Reflection
{
    using System;
    using System.Collections.Generic;


    public static class InterfaceImplementationExtensions
    {
        static readonly AnonymousObjectDictionaryConverter _converter = new AnonymousObjectDictionaryConverter();
        static readonly InterfaceImplementationInitializer _initializer = new InterfaceImplementationInitializer();

        public static object InitializeProxy(this Type interfaceType, object initializer)
        {
            IDictionary<string, object> values = _converter.Convert(initializer);

            return InitializeProxy(interfaceType, values);
        }

        public static object InitializeProxy(this Type interfaceType, IDictionary<string, object> values)
        {
            Type proxyType = InterfaceImplementationBuilder.GetProxyFor(interfaceType);

            return _initializer.InitializeFromDictionary(proxyType, values);
        }

        public static T InitializeProxy<T>(object initializer)
            where T : class
        {
            return (T)InitializeProxy(typeof(T), initializer);
        }

        public static T InitializeProxy<T>(IDictionary<string, object> values)
            where T : class
        {
            return (T)InitializeProxy(typeof(T), values);
        }
    }
}