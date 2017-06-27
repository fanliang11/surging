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
	using System.Collections.Generic;

	public abstract class EventActionBase<T> :
		StateEventAction<T>
		where T : StateMachine<T>
	{
		protected EventActionBase(Event definedEvent)
		{
			DefinedEvent = definedEvent;

			Actions = new EventActionList<T>();
		}

		protected EventActionList<T> Actions { get; private set; }

		public Event DefinedEvent { get; private set; }

		public IEnumerable<EventAction<T>> EventActions
		{
			get { return Actions; }
		}

		public void Execute(T instance, Event @event, object parameter)
		{
			if (ParameterMeetsCondition(parameter))
			{
				Actions.Execute(instance, @event, parameter);
			}
		}

		protected abstract bool ParameterMeetsCondition(object parameter);
	}
}