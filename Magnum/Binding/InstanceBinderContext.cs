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
namespace Magnum.Binding
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;
    using Reflection;
    using TypeBinders;


    public class InstanceBinderContext :
        BinderContext
    {
        static readonly Cache<Type, ObjectBinder> _typeBinders;

        readonly Stack<ModelBinderContext> _contextStack;
        readonly Stack<ObjectPropertyBinder> _propertyStack;

        static InstanceBinderContext()
        {
            _typeBinders = new GenericTypeCache<ObjectBinder>(typeof(ObjectBinder<>), CreateBinderFor);

            LoadBuiltInBinders();
        }

        public InstanceBinderContext(ModelBinderContext context)
        {
            _contextStack = new Stack<ModelBinderContext>();
            _contextStack.Push(context);

            _propertyStack = new Stack<ObjectPropertyBinder>();
        }

        public ModelBinderContext Context
        {
            get { return _contextStack.Count > 0 ? _contextStack.Peek() : null; }
        }

        public string PropertyKey
        {
            get
            {
                const string separator = ".";

                string value = string.Join(separator,
                                           _propertyStack
                                               .Select(x => x.Property.Name)
                                               .Reverse()
                                               .ToArray());

                return value;
            }
        }

        public object PropertyValue
        {
            get
            {
                object value = null;

                Context.GetValue(PropertyKey, x =>
                    {
                        value = x;
                        return true;
                    }, () => value = null);

                return value;
            }
        }

        public PropertyInfo Property
        {
            get { return _propertyStack.Count > 0 ? _propertyStack.Peek().Property : null; }
        }

        public object Bind(ObjectPropertyBinder property)
        {
            _propertyStack.Push(property);

            try
            {
                return Bind(Property.PropertyType);
            }
            finally
            {
                _propertyStack.Pop();
            }
        }

        public object Bind(Type type)
        {
            ObjectBinder binder = _typeBinders[type];

            return binder.Bind(this);
        }

        static void LoadBuiltInBinders()
        {
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => !x.IsGenericType)
                .Where(x => x.Namespace == typeof(StringBinder).Namespace)
                .Each(type =>
                    {
                        type.GetInterfaces().Where(x => x.IsGenericType
                                                        && x.GetGenericTypeDefinition() == typeof(ObjectBinder<>))
                            .Each(interfaceType =>
                                {
                                    Type itemType = interfaceType.GetGenericArguments().First();

                                    _typeBinders.Add(itemType, FastActivator.Create(type) as ObjectBinder);
                                });
                    });
        }

        static ObjectBinder CreateBinderFor(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                ObjectBinder underlyingBinder = _typeBinders[underlyingType];
                return (ObjectBinder)FastActivator.Create(typeof(NullableBinder<>), new[] {underlyingType},
                                                          new object[] {underlyingBinder});
            }

            if (type.IsEnum)
                return (ObjectBinder)FastActivator.Create(typeof(EnumBinder<>), new[] {type});

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                if (type.IsArray)
                    return (ObjectBinder)FastActivator.Create(typeof(ArrayBinder<>), new[] {type.GetElementType()});

                if (type.IsGenericType)
                {
                    Type genericTypeDefinition = type.GetGenericTypeDefinition();
                    Type[] arguments = type.GetGenericArguments();
                    if (genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(List<>))
                        return (ObjectBinder)FastActivator.Create(typeof(ListBinder<>), arguments);

                    // TODO handle dictionary as well
                }

                throw new NotSupportedException("Unsupported enumeration type: " + type.FullName);
            }

            if (type.IsInterface)
            {
                Type proxyType = InterfaceImplementationBuilder.GetProxyFor(type);

                return (ObjectBinder)FastActivator.Create(typeof(FastObjectBinder<>), new[] {proxyType});
            }

            return (ObjectBinder)FastActivator.Create(typeof(FastObjectBinder<>), new[] {type});
        }
    }
}