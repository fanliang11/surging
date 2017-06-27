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
    using System.Linq;
    using System.Reflection;
    using Caching;


    public class FastPropertyCache<T> :
        AbstractCacheDecorator<string, FastProperty<T>>
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public FastPropertyCache()
            : base(CreatePropertyCache())
        {
        }

        static ConcurrentCache<string, FastProperty<T>> CreatePropertyCache()
        {
            return new ConcurrentCache<string, FastProperty<T>>(typeof(T).GetProperties(Flags)
                                                                    .Select(x => new FastProperty<T>(x, Flags))
                                                                    .ToDictionary(x => x.Property.Name));
        }

        public void Each(T instance, Action<object> action)
        {
            Each(property => action(property.Get(instance)));
        }
    }
}