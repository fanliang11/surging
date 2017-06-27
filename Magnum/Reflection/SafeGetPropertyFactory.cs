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
	using System.Linq.Expressions;
	using System.Reflection;


	public interface SafeGetPropertyFactory
	{
		Expression GetExpression { get; }
		ParameterExpression Parameter { get; }
	}


	public class SafeGetPropertyFactory<T, TProperty> :
		SafeGetPropertyFactory
		where T : class
	{
		readonly ParameterExpression _callback;
		Expression _getExpression;
		GetProperty<T, TProperty> _getter;
		ParameterExpression _parameter;


		public SafeGetPropertyFactory(Expression<Func<T, TProperty>> propertyExpression)
		{
			_callback = Expression.Parameter(typeof(Action<TProperty>), "output");

			Visit(propertyExpression.Body);
		}

		public SafeGetPropertyFactory(Expression expression)
		{
			Visit(expression);
		}

		public GetProperty<T, TProperty> GetProperty
		{
			get
			{
				if (_getter == null)
					CreateGetter();

				return _getter;
			}
		}

		public Expression GetExpression
		{
			get { return _getExpression; }
		}

		public ParameterExpression Parameter
		{
			get { return _parameter; }
		}

		void Visit(Expression expression)
		{
			if (expression.NodeType == ExpressionType.MemberAccess)
				VisitMemberAccess((MemberExpression)expression);
			else if (expression.NodeType == ExpressionType.Call)
				VisitMethodCall((MethodCallExpression)expression);
			else if (expression.NodeType == ExpressionType.ArrayIndex)
				VisitArrayIndex((BinaryExpression)expression);
			else
				throw new ArgumentException("Unknown expression type: " + expression.NodeType);
		}

		void VisitArrayIndex(BinaryExpression expression)
		{
			Expression collection = expression.Left;
			Expression index = expression.Right;

			Type objectType = collection.Type;
			Type elementType = expression.Type;

			SafeGetPropertyFactory expressionVisitor = CreateVisitor(typeof(T), objectType, collection);

			_parameter = expressionVisitor.Parameter;

			Expression instance = expressionVisitor.GetExpression;

			if (!(index is ConstantExpression))
				throw new ArgumentException("The argument must be a constant expression: " + index);

			ConstantExpression nullInstance = Expression.Constant(null, instance.Type);

			BinaryExpression instanceIsNotNull = Expression.NotEqual(nullInstance, instance);

			Expression getCount = Expression.ArrayLength(instance);

			Expression nullElement;
			if (IsNotNullable(elementType))
				nullElement = Expression.Convert(Expression.Constant(null, typeof(object)), elementType);
			else
				nullElement = Expression.Constant(null, elementType);

			Expression getItem = Expression.ArrayIndex(instance, index);

			BinaryExpression lessThan = Expression.LessThan(index, getCount);
			BinaryExpression greatherThanOrEqualTo = Expression.GreaterThanOrEqual(index, Expression.Constant(0, typeof(int)));
			BinaryExpression between = Expression.And(lessThan, greatherThanOrEqualTo);

			ConditionalExpression condition = Expression.Condition(between, getItem, nullElement);

			Expression getExpression = Expression.Condition(instanceIsNotNull, condition, nullElement);

			if (_callback != null)
				getExpression = CreateInvokeExpression(instanceIsNotNull, getExpression, _callback);

			_getExpression = getExpression;
		}

		protected Expression VisitMethodCall(MethodCallExpression expression)
		{
			Type objectType = expression.Object.Type;
			Type elementType = expression.Type;

			Type collectionType = typeof(ICollection<>).MakeGenericType(elementType);
			if (collectionType.IsAssignableFrom(objectType))
			{
				if (IsGetItemMethod(expression))
				{
					SafeGetPropertyFactory expressionVisitor = CreateVisitor(typeof(T), objectType, expression.Object);

					_parameter = expressionVisitor.Parameter;

					Expression instance = expressionVisitor.GetExpression;
					Expression index = expression.Arguments[0];

					if (!(index is ConstantExpression))
						throw new ArgumentException("The argument must be a constant expression: " + index);

					ConstantExpression nullInstance = Expression.Constant(null, instance.Type);

					BinaryExpression instanceIsNotNull = Expression.NotEqual(nullInstance, instance);

					Expression getCount = CreateCountExpression(collectionType, instance);

					Expression nullElement;
					if (IsNotNullable(elementType))
						nullElement = Expression.Convert(Expression.Constant(null, typeof(object)), elementType);
					else
						nullElement = Expression.Constant(null, elementType);

					Expression getItem = CreateGetItemExpression(expression.Method, instance, index);

					BinaryExpression lessThan = Expression.LessThan(index, getCount);
					BinaryExpression greatherThanOrEqualTo = Expression.GreaterThanOrEqual(index, Expression.Constant(0, typeof(int)));
					BinaryExpression between = Expression.And(lessThan, greatherThanOrEqualTo);

					ConditionalExpression condition = Expression.Condition(between, getItem, nullElement);

					Expression getExpression = Expression.Condition(instanceIsNotNull, condition, nullElement);

					if (_callback != null)
						getExpression = CreateInvokeExpression(instanceIsNotNull, getExpression, _callback);

					_getExpression = getExpression;

					return expression;
				}
			}


			throw new ArgumentException("Unsupported method call expression: " + expression);
		}

		static Expression CreateCountExpression(Type collectionType, Expression instance)
		{
			MethodInfo getMethod = collectionType.GetProperty("Count").GetGetMethod();

			return Expression.Call(instance, getMethod);
		}

		static Expression CreateGetItemExpression(MethodInfo getMethod, Expression instance, Expression index)
		{
			//MethodInfo getMethod = collectionType.GetProperty("Item").GetGetMethod();

			return Expression.Call(instance, getMethod, index);
		}

		static bool IsGetItemMethod(MethodCallExpression expression)
		{
			return "get_Item".Equals(expression.Method.Name)
			       && expression.Arguments.Count == 1
			       && expression.Arguments[0].Type == typeof(int);
		}

		protected Expression VisitMemberAccess(MemberExpression m)
		{
			var property = (PropertyInfo)m.Member;

			Expression instance;
			if (m.Expression.NodeType == ExpressionType.Parameter)
			{
				_parameter = (ParameterExpression)m.Expression;
				instance = _parameter;
			}
			else
			{
				SafeGetPropertyFactory expressionVisitor = CreateVisitor(m.Expression.Type, property.PropertyType, m.Expression);

				_parameter = expressionVisitor.Parameter;
				instance = expressionVisitor.GetExpression;
			}

			ConstantExpression nullInstance = Expression.Constant(null, instance.Type);
			BinaryExpression instanceIsNotNull = Expression.NotEqual(nullInstance, instance);

			Expression getExpression = CreateGetPropertyExpression(property, instance, instanceIsNotNull);

			if (_callback != null)
				getExpression = CreateInvokeExpression(instanceIsNotNull, getExpression, _callback);

			_getExpression = getExpression;

			return m;
		}

		static Expression CreateGetPropertyExpression(PropertyInfo property,
		                                              Expression instance,
		                                              BinaryExpression instanceIsNotNull)
		{
			if (IsNotNullable(property.PropertyType))
				return Expression.Call(instance, property.GetGetMethod(true));

			ConstantExpression nullProperty = Expression.Constant(null, property.PropertyType);

			MethodCallExpression getProperty = Expression.Call(instance, property.GetGetMethod());

			return Expression.Condition(instanceIsNotNull, getProperty, nullProperty);
		}

		static bool IsNotNullable(Type propertyType)
		{
			return propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null;
		}

		static Expression CreateInvokeExpression(BinaryExpression instanceIsNotNull,
		                                         Expression getExpression,
		                                         ParameterExpression callback)
		{
			Action<TProperty> noAction = x => { };

			ConstantExpression getNoAction = Expression.Constant(noAction, typeof(Action<TProperty>));

			ConstantExpression defaultTProperty = Expression.Constant(default(TProperty), typeof(TProperty));

			InvocationExpression doNothing = Expression.Invoke(getNoAction, defaultTProperty);

			InvocationExpression invokeCallback = Expression.Invoke(callback, getExpression);

			ConditionalExpression whichCallback = Expression.Condition(instanceIsNotNull, invokeCallback, doNothing);

			return whichCallback;
		}

		void CreateGetter()
		{
			Action<T, Action<TProperty>> getter =
				Expression.Lambda<Action<T, Action<TProperty>>>(_getExpression, _parameter, _callback).Compile();

			_getter = (obj, callback) => getter(obj, callback);
		}

		static SafeGetPropertyFactory CreateVisitor(Type objectType, Type propertyType, Expression expression)
		{
			Type visitorType = typeof(SafeGetPropertyFactory<,>).MakeGenericType(objectType, propertyType);

			return (SafeGetPropertyFactory)Activator.CreateInstance(visitorType, expression);
		}
	}
}