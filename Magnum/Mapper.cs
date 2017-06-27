// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Magnum
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public class Mapper<TSource, TTarget>
        where TTarget : new()
    {
        private readonly List<IMapAction<TSource, TTarget>> _maps = new List<IMapAction<TSource, TTarget>>();

        public MapAction<TProperty> From<TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            return new MapAction<TProperty>(this, expression);
        }

        private void Add(IMapAction<TSource, TTarget> mapAction)
        {
            _maps.Add(mapAction);
        }

        public TTarget Transform(TSource source)
        {
            return Transform(source, new TTarget());
        }

        public TTarget Transform(TSource source, TTarget target)
        {
            foreach (var mapAction in _maps)
            {
                mapAction.Map(source, target);
            }

            return target;
        }

        public string WhatAmIDoing()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("<transform from=\"{0}\" to=\"{1}\">", typeof (TSource), typeof (TTarget)));
            foreach (var map in _maps)
            {
                sb.AppendFormat("    <map from=\"{0}\" to=\"{1}\" />{2}", map.SourceMember, map.TargetMember, Environment.NewLine);
            }
            sb.AppendLine("</transform>");
            return sb.ToString();
        }

        #region Nested type: IMapAction

        public interface IMapAction<TS, TT>
        {
            string TargetMember { get; }

            string TargetClass { get; }

            string SourceMember { get; }

            string SourceClass { get; }
            void Map(TS source, TT target);
        }

        #endregion

        #region Nested type: MapAction

        public class MapAction<TProperty> :
            IMapAction<TSource, TTarget>
        {
            private readonly Mapper<TSource, TTarget> _mapper;
            private readonly Func<TSource, TProperty> _property;
            private readonly string _sourceClass;
            private readonly string _sourceMember;
            private Action<TTarget, TProperty> _action;
            private string _targetClass;
            private string _targetMember;

            internal MapAction(Mapper<TSource, TTarget> mapper, Expression<Func<TSource, TProperty>> expression)
            {
                var body = expression.Body as MemberExpression;
                if (body != null)
                {
                    if (body.Member.MemberType != MemberTypes.Property)
                        throw new ArgumentException("Not a property: " + body.Member.Name);

                    Type t = body.Member.DeclaringType;
                    PropertyInfo prop = t.GetProperty(body.Member.Name,
                                                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    ParameterExpression instance = Expression.Parameter(typeof (TSource), "instance");
                    _property =
                        Expression.Lambda<Func<TSource, TProperty>>(Expression.Call(instance, prop.GetGetMethod()), instance)
                            .Compile();

                    _sourceClass = body.Member.DeclaringType.Name;
                    _sourceMember = body.Member.Name;
                }
                else
                {
                    _property = expression.Compile();
                    _sourceClass = typeof (TSource).Name;
                    _sourceMember = expression.Body.ToString();
                }


                _mapper = mapper;
            }

            #region IMapAction<TSource,TTarget> Members

            public string TargetMember
            {
                get { return _targetMember; }
            }

            public string TargetClass
            {
                get { return _targetClass; }
            }

            public string SourceMember
            {
                get { return _sourceMember; }
            }

            public string SourceClass
            {
                get { return _sourceClass; }
            }

            public void Map(TSource source, TTarget target)
            {
                _action(target, _property(source));
            }

            #endregion

            public MapAction<TProperty> To<TTargetProperty>(Expression<Func<TTarget, TTargetProperty>> expression)
            {
            	Guard.AgainstNull(expression);
            	var body = Guard.IsTypeOf<MemberExpression>(expression.Body);

                Type t = body.Member.DeclaringType;
                PropertyInfo prop = t.GetProperty(body.Member.Name,
                                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                ParameterExpression instance = Expression.Parameter(typeof (TTarget), "instance");
                ParameterExpression value = Expression.Parameter(typeof (TProperty), "value");

                // value as T is slightly faster than (T)value, so if it's not a value type, use that
                UnaryExpression instanceCast = (!prop.DeclaringType.IsValueType)
                                                   ? Expression.TypeAs(instance, prop.DeclaringType)
                                                   : Expression.Convert(instance, prop.DeclaringType);
                UnaryExpression valueCast = (!prop.PropertyType.IsValueType)
                                                ? Expression.TypeAs(value, prop.PropertyType)
                                                : Expression.Convert(value, prop.PropertyType);

                _action =
                    Expression.Lambda<Action<TTarget, TProperty>>(
                        Expression.Call(instanceCast, prop.GetSetMethod(), valueCast), new[] {instance, value}).Compile();

                _targetClass = body.Member.DeclaringType.Name;
                _targetMember = body.Member.Name;

                _mapper.Add(this);

                return this;
            }
        }

        #endregion
    }
}