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
	using System.Diagnostics;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Runtime.Serialization;
	using Extensions;

	[DebuggerDisplay("Current State = {CurrentState.Name}")]
	public class StateMachine<T> :
		StateMachine,
		ISerializable,
		IStateMachineInspectorSite
		where T : StateMachine<T>
	{
		private const string CompletedStateName = "Completed";
		private const string InitialStateName = "Initial";
		private static readonly State<T> _anyState = new State<T>("Any");
		private static readonly Dictionary<string, BasicEvent<T>> _events = new Dictionary<string, BasicEvent<T>>();
		private static readonly Dictionary<string, State<T>> _states = new Dictionary<string, State<T>>();
		private static State<T> _completedState;
		private static State<T> _initialState;
		private State<T> _currentState;

		static StateMachine()
		{
			InitializeStates();
			InitializeEvents();
		}

		protected StateMachine()
		{
			VerifyStateMachineConfiguration();

			EnterState(_initialState);
		}

		public StateMachine(SerializationInfo info, StreamingContext context)
		{
			string currentStateName = info.GetString("Current");

			_currentState = GetState(currentStateName);
			if (_currentState == null)
				throw new SerializationException("The serialized state is not valid for this version of the state machine: " + currentStateName);
		}

		/// <summary>
		/// Returns the current state of the StateMachine
		/// </summary>
		public virtual State CurrentState
		{
			get { return _currentState; }
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Current", CurrentState.Name);
		}

		public virtual void Inspect(IStateMachineInspector inspector)
		{
			inspector.Inspect(this, () =>
				{
					_initialState.Inspect(inspector);

					foreach (var state in _states.Values)
					{
						if (state != _initialState && state != _completedState)
							state.Inspect(inspector);
					}

					_anyState.Inspect(inspector);

					_completedState.Inspect(inspector);
				});
		}

		/// <summary>
		/// Send an event within the current state
		/// </summary>
		/// <param name="raised">The event to raise</param>
		public virtual void RaiseEvent(Event raised)
		{
			BasicEvent<T> eevent = BasicEvent<T>.GetEvent(raised);

			_currentState.RaiseEvent(this as T, eevent, null);
			_anyState.RaiseEvent(this as T, eevent, null);
		}

		/// <summary>
		/// Send an event within the current state passing the data associated with the event
		/// </summary>
		/// <typeparam name="TData">The type of data, must match the data type expected by the event</typeparam>
		/// <param name="raised">The event to raise</param>
		/// <param name="value">The data to associate with the event</param>
		public virtual void RaiseEvent<TData>(Event raised, TData value)
		{
			DataEvent<T, TData> eevent = DataEvent<T, TData>.GetEvent(raised);

			_currentState.RaiseEvent(this as T, eevent, value);
			_anyState.RaiseEvent(this as T, eevent, value);
		}

		protected internal virtual void ChangeCurrentState(State newState)
		{
			if (_currentState == newState)
				return;

			LeaveCurrentState();

			EnterState(State<T>.GetState(newState));
		}

		private void EnterState(State<T> state)
		{
			_currentState = state;

			RaiseEvent(state.Enter);
		}

		private void LeaveCurrentState()
		{
			if (_currentState == null) return;

			RaiseEvent(_currentState.Leave);
			_currentState = null;
		}

		public static State<T> GetState(string name)
		{
			State<T> state;
			return _states.TryGetValue(name, out state) ? state : null;
		}

		/// <summary>
		/// This must be called from the static constructor to define the states, events, and transitions
		/// 
		/// This is performed as an expression because the base static class needs to perform setup
		/// before actually defining the state machine and derived classes don't call static base class 
		/// constructors
		/// </summary>
		/// <param name="definition">An expression to invoke to setup the state machine</param>
		protected static void Define(Action definition)
		{
			definition();
		}

		/// <summary>
		/// Sets the state to use for a completed state machine. By default, the state named "Completed" is used.
		/// </summary>
		/// <param name="completedState"></param>
		protected static void SetCompletedState(State completedState)
		{
			_completedState = State<T>.GetState(completedState);
		}

		/// <summary>
		/// Sets the state to use for a newly created state machine. By default, the state named "Initial" is used.
		/// </summary>
		/// <param name="initialState"></param>
		protected static void SetInitialState(State initialState)
		{
			_initialState = State<T>.GetState(initialState);
		}

		/// <summary>
		/// Defines an actions to take when an event is raised within a state
		/// </summary>
		/// <param name="raised"></param>
		/// <returns></returns>
		protected static BasicEventAction<T> When(Event raised)
		{
			BasicEvent<T> eevent = BasicEvent<T>.GetEvent(raised);

			return new BasicEventAction<T>(eevent);
		}

		/// <summary>
		/// Starts the definition of actions to take when an event occurs
		/// </summary>
		/// <param name="raised">The event that would be raised</param>
		protected static DataEventAction<T, TData> When<TData>(Event<TData> raised)
		{
			DataEvent<T, TData> eevent = DataEvent<T, TData>.GetEvent(raised);

			return new DataEventAction<T, TData>(eevent);
		}

		/// <summary>
		/// Defines an action to take if an exception occurs while the event is being handled
		/// </summary>
		/// <typeparam name="TException"></typeparam>
		/// <returns></returns>
		protected static ExceptionAction<T, TException> InCaseOf<TException>()
			where TException : Exception
		{
			return new ExceptionAction<T, TException>();
		}

		/// <summary>
		/// Opens the definition of the event actions for the initial state (shortcut for During(Initial))
		/// </summary>
		/// <param name="actions"></param>
		protected static void Initially(params StateEventAction<T>[] actions)
		{
			During(_initialState, actions);
		}

		/// <summary>
		/// Opens the definition of the event actions for the specified state
		/// </summary>
		/// <param name="inputState"></param>
		/// <param name="actions"></param>
		protected static void During(State inputState, params StateEventAction<T>[] actions)
		{
			State<T> state = State<T>.GetState(inputState);

			actions.Each(state.BindEventAction);
		}

		/// <summary>
		/// Starts the definition of actions to take when events occur during any state
		/// </summary>
		/// <param name="actions"></param>
		protected static void Anytime(params StateEventAction<T>[] actions)
		{
			actions.Each(_anyState.BindEventAction);
		}

		protected static EventCombiner<T> Combine(params Event[] events)
		{
			var combiner = new EventCombiner<T>(_anyState.BindEventAction, events);

			return combiner;
		}

		internal static State GetCompletedState()
		{
			return _completedState;
		}

		private static void InitializeEvents()
		{
			Type machineType = typeof (T);
			foreach (PropertyInfo propertyInfo in machineType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
			{
				if (IsPropertyABasicEvent(propertyInfo))
				{
					BasicEvent<T> value = SetPropertyValue(propertyInfo, x => new BasicEvent<T>(x.Name));

					_events.Add(value.Name, value);
				}
				else if (IsPropertyATypedEvent(propertyInfo))
				{
					Type eventType = typeof (DataEvent<,>).MakeGenericType(typeof (T), propertyInfo.PropertyType.GetGenericArguments()[0]);

					ConstructorInfo ctor = eventType.GetConstructors()[0];
					var name = Expression.Parameter(typeof (string), "name");
					var newExp = Expression.New(ctor, name);

					Func<string, object> creator = Expression.Lambda<Func<string, object>>(newExp, new[] {name}).Compile();

					PropertyInfo eventProperty = propertyInfo;

					SetPropertyValue(propertyInfo, x => creator(eventProperty.Name));
				}
			}
		}

		private static void InitializeStates()
		{
			Type machineType = typeof (T);
            foreach (PropertyInfo propertyInfo in machineType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
			{
				if (!IsPropertyAState(propertyInfo)) continue;

				State<T> state = SetPropertyValue(propertyInfo, x => new State<T>(x.Name));

				_states.Add(state.Name, state);

				switch (state.Name)
				{
					case InitialStateName:
						_initialState = state;
						break;
					case CompletedStateName:
						_completedState = state;
						break;
				}
			}
		}

		private static bool IsPropertyABasicEvent(PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType == typeof (BasicEvent<T>) || propertyInfo.PropertyType == typeof (Event);
		}

		private static bool IsPropertyATypedEvent(PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType.IsGenericType &&
			       propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (Event<>);
		}

		private static bool IsPropertyAState(PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType == typeof (State<T>) || propertyInfo.PropertyType == typeof (State);
		}

		private static object SetPropertyValue(PropertyInfo propertyInfo, Func<PropertyInfo, object> getValue)
		{
			var value = Expression.Parameter(typeof (object), "value");
			var valueCast = propertyInfo.PropertyType.IsValueType
			                	? Expression.TypeAs(value, propertyInfo.PropertyType)
			                	: Expression.Convert(value, propertyInfo.PropertyType);

			var action = Expression.Lambda<Action<object>>(Expression.Call(propertyInfo.GetSetMethod(), valueCast), new[] {value}).Compile();

			object propertyValue = getValue(propertyInfo);
			action(propertyValue);

			return propertyValue;
		}

		private static TValue SetPropertyValue<TValue>(PropertyInfo propertyInfo, Func<PropertyInfo, TValue> getValue)
		{
			var value = Expression.Parameter(typeof (TValue), "value");
			var action = Expression.Lambda<Action<TValue>>(Expression.Call(propertyInfo.GetSetMethod(), value), new[] {value}).Compile();

			TValue propertyValue = getValue(propertyInfo);
			action(propertyValue);

			return propertyValue;
		}

		private static void VerifyStateMachineConfiguration()
		{
			if (_states.Count == 0)
				throw new StateMachineException("A state machine must have at least one state to be valid.");

			if (_initialState == null)
				throw new StateMachineException("No initial state has been defined.");

			if (_completedState == null)
				throw new StateMachineException("No completed state has been defined.");
		}
	}

	public interface StateMachine
	{
	}

	public static class ExtensionsToStateMachine
	{
	}
}