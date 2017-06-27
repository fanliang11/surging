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
	using Collections;
	using Extensions;

	public class State<T> :
		State,
		IStateMachineInspectorSite
		where T : StateMachine<T>
	{
		private readonly MultiDictionary<Event, StateEventAction<T>> _actions;

		private readonly BasicEvent<T> _enter;
		private readonly BasicEvent<T> _leave;
		private readonly string _name;

		public State(string name)
		{
			_name = name;

			_enter = new BasicEvent<T>(Name + ":Enter");
			_leave = new BasicEvent<T>(Name + ":Leave");

			_actions = new MultiDictionary<Event, StateEventAction<T>>(true);
		}

		public void Inspect(IStateMachineInspector inspector)
		{
			inspector.Inspect(this, () =>
				{
					_actions.Each(item => inspector.Inspect(item.Key, () =>
						{
							item.Value.Each(x => inspector.Inspect(x, () =>
								{
									x.EventActions.Each(inspector.Inspect);
								}));
						}));
				});
		}

		public Event Enter
		{
			get { return _enter; }
		}

		public Event Leave
		{
			get { return _leave; }
		}

		public string Name
		{
			get { return _name; }
		}

		public void RaiseEvent(T instance, BasicEvent<T> eevent, object value)
		{
			if (!_actions.ContainsKey(eevent))
				return;

			foreach (var action in _actions[eevent])
			{
				action.Execute(instance, eevent, value);
			}
		}

		public override string ToString()
		{
			return string.Format("{0} (State)", _name);
		}

		public void BindEventAction(StateEventAction<T> action)
		{
			_actions.Add(action.DefinedEvent, action);
		}

		public static State<T> GetState(State input)
		{
			State<T> result = input as State<T>;
			if (result == null)
				throw new ArgumentException("The state is not valid for this state machine", "input");

			return result;
		}
	}

	public interface State
	{
		string Name { get; }

		Event Enter { get; }
		Event Leave { get; }
	}
}