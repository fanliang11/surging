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
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;


    public class SafePropertyVisitor :
        ExpressionVisitor
    {
        ReadOnlyCollection<ParameterExpression> _parameters;
        Action<object, int, object> _setter;
        Type _type;

        public SafePropertyVisitor(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
                throw new ArgumentException("Expression must be a lambda to capture parameters: " + expression);

            _parameters = lambdaExpression.Parameters;

            Visit(lambdaExpression.Body);
        }

        public Action<object, int, object> Setter
        {
            get { return _setter; }
        }

        public Type Type
        {
            get { return _type; }
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var property = m.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("Unable to make access for non-property: " + m);

            Action<object, object> setMethod = new FastProperty(property, BindingFlags.NonPublic).SetDelegate;

            if (_setter == null)
            {
                _type = property.PropertyType;
                _setter = (o, args, v) => { setMethod(o, v); };
            }
            else
            {
                Func<object, object> getMethod = new FastProperty(property, BindingFlags.NonPublic).GetDelegate;
                Func<object> factoryMethod = CreateFactory(property.PropertyType);
                Action<object, int, object> nextSetter = _setter;

                _setter = (o, args, v) =>
                    {
                        object target = getMethod(o);
                        if (target == null)
                        {
                            target = factoryMethod();
                            setMethod(o, target);
                        }

                        nextSetter(target, args, v);
                    };
            }

            return base.VisitMemberAccess(m);
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            Func<object, int> getCount = GetCountMethod(expression);
            Func<object> objectFactory = CreateFactory(expression.Method.ReturnType);
            Action<object, object> addItem = GetAddMethod(expression);
            Func<object, int, object> getItem = GetGetItemMethod(expression);
            Func<int, int> getIndex = x => 0;

            Action<object, int, object> nextSetter = _setter;

            if (expression.Arguments[0].NodeType == ExpressionType.Constant)
            {
                var itemIndex = (int)(((ConstantExpression)expression.Arguments[0]).Value);
                getIndex = x => itemIndex;
            }
            else if (expression.Arguments[0].NodeType == ExpressionType.Parameter
                     && expression.Arguments[0].Type == typeof(int))
                getIndex = x => x;
            else
                throw new ArgumentException("Unable to use argument indexer: " + expression);

            _setter = (o, index, v) =>
                {
                    int itemIndex = getIndex(index);

                    int count = getCount(o);
                    for (; count <= itemIndex; count++)
                        addItem(o, objectFactory());

                    object item = getItem(o, itemIndex);

                    nextSetter(item, index, v);
                };

            return base.VisitMethodCall(expression);
        }

        public static Func<object> CreateFactory(Type type)
        {
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(IList<>))
                {
                    Type createType = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);

                    return () => FastActivator.Create(createType);
                }
            }

            return () => FastActivator.Create(type);
        }

        public static Func<object, int> GetCountMethod(MethodCallExpression expression)
        {
            if (expression.Arguments.Count == 1 && expression.Arguments[0].Type == typeof(int))
            {
                Type interfaceType = expression.Object.Type;
                Type argument = interfaceType.GetGenericArguments()[0];
                Type callType = typeof(ICollection<>).MakeGenericType(argument);

                MethodInfo getMethod = callType.GetProperty("Count").GetGetMethod();

                ParameterExpression input = Expression.Parameter(typeof(object), "input");
                UnaryExpression cast = Expression.TypeAs(input, expression.Object.Type);

                return Expression.Lambda<Func<object, int>>(Expression.Call(cast, getMethod), input).Compile();
            }

            throw new NotImplementedException("No idea why this won't work");
        }

        public static Action<object, object> GetAddMethod(MethodCallExpression expression)
        {
            if (expression.Arguments.Count == 1 && expression.Arguments[0].Type == typeof(int))
            {
                Type interfaceType = expression.Object.Type;
                Type argument = interfaceType.GetGenericArguments()[0];
                Type callType = typeof(ICollection<>).MakeGenericType(argument);

                MethodInfo getMethod = callType.GetMethod("Add");

                ParameterExpression input = Expression.Parameter(typeof(object), "input");
                ParameterExpression value = Expression.Parameter(typeof(object), "value");
                UnaryExpression cast = Expression.TypeAs(input, expression.Object.Type);
                UnaryExpression castValue = Expression.TypeAs(value, argument);

                return
                    Expression.Lambda<Action<object, object>>(Expression.Call(cast, getMethod, castValue), input, value)
                        .Compile();
            }

            throw new NotImplementedException("No idea why this won't work");
        }

        public static Func<object, int, object> GetGetItemMethod(MethodCallExpression expression)
        {
            if (expression.Arguments.Count == 1 && expression.Arguments[0].Type == typeof(int))
            {
                ParameterExpression input = Expression.Parameter(typeof(object), "input");
                ParameterExpression index = Expression.Parameter(typeof(int), "index");
                UnaryExpression cast = Expression.TypeAs(input, expression.Object.Type);

                return
                    Expression.Lambda<Func<object, int, object>>(Expression.Call(cast, expression.Method, index), input,
                                                                 index).Compile
                        ();
            }

            throw new NotImplementedException("No idea why this won't work");
        }
    }
}