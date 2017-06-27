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
namespace Magnum.StateMachine
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using Extensions;
	using Reflection;

	/// <summary>
	/// A combined event is raised automatically after all of the combined events have been raised
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EventCombiner<T>
		where T : StateMachine<T>
	{
		private readonly Action<BasicEventAction<T>> _bindEventAction;
		private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private Event _target;
		private readonly List<Event> _sources = new List<Event>();

		public EventCombiner(Action<BasicEventAction<T>> bindEventAction, params Event[] events)
		{
			_bindEventAction = bindEventAction;
			_sources.AddRange(events);
		}

		public void Into(Event target, Expression<Func<T, int>> propertyExpression)
		{
			if(_sources.Count > 31)
				throw new InvalidOperationException("More more than 31 events can be combined into an event with an integer property");

			_target = target;

			FastProperty<T, int> property = GetPropertyAccessor(propertyExpression);

			int all = 0;
			Enumerable.Range(0, _sources.Count).Each(x => all = all | (1 << x));

			for (int i = 0; i < _sources.Count; i++)
			{
				BindEventAction(all, i, property);
			}
		}

		private void BindEventAction(int all, int i, FastProperty<T, int> property)
		{
			var e = _sources[i];
			var eevent = BasicEvent<T>.GetEvent(e);
			var eventAction = new BasicEventAction<T>(eevent);

			int flag = 1 << i;

			eventAction.Then(x =>
				{
					int value = property.Get(x) | flag;
					property.Set(x, value);

					if(value == all)
						x.RaiseEvent(_target);
				});

			_bindEventAction(eventAction);
		}

		private static FastProperty<T, int> GetPropertyAccessor(Expression<Func<T, int>> propertyExpression)
		{
			Expression expression = propertyExpression;
			if (expression.NodeType == ExpressionType.Lambda)
				expression = ((LambdaExpression) expression).Body;

			var memberExpression = expression as MemberExpression;
			if(memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property)
				throw new ArgumentException("A valid property expression must be specified", "propertyExpression");

			PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;

			return new FastProperty<T, int>(propertyInfo, _bindingFlags);
		}
	}
}