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
    using Binding;
    using ValueProviders;


    public class InterfaceImplementationInitializer
    {
        public object InitializeFromDictionary(Type objectType, IDictionary<string, object> values)
        {
            ValueProvider dictionaryProvider = new DictionaryValueProvider(values);

            ModelBinder binder = new FastModelBinder();
            ModelBinderContext context = new InitializerModelBinderContext(dictionaryProvider);

            object obj = binder.Bind(objectType, context);

            return obj;
        }


        public class InitializerModelBinderContext :
            ModelBinderContext
        {
            readonly ValueProvider _provider;

            public InitializerModelBinderContext(ValueProvider provider)
            {
                _provider = provider;
            }

            public bool GetValue(string key, Func<object, bool> matchingValueAction)
            {
                return _provider.GetValue(key, matchingValueAction);
            }

            public bool GetValue(string key, Func<object, bool> matchingValueAction, Action missingValueAction)
            {
                return _provider.GetValue(key, matchingValueAction, missingValueAction);
            }

            public void GetAll(Action<string, object> valueAction)
            {
                _provider.GetAll(valueAction);
            }
        }
    }
}