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
	using System.Collections;
	using System.Collections.Generic;
	using Collections;
	using Extensions;

	public interface IEventActionExceptionHandler<T>
		where T : StateMachine
	{
		void HandleException(T stateMachine, Event @event, object parameter, Exception exception);
	}

	public class ExceptionActionDictionary<T> :
		IEnumerable<ExceptionAction<T>>, IEventActionExceptionHandler<T> where T : StateMachine<T>
	{
		private readonly Dictionary<Type, ExceptionAction<T>> _exceptionEvents = new Dictionary<Type, ExceptionAction<T>>();

		public IEnumerator<ExceptionAction<T>> GetEnumerator()
		{
			return _exceptionEvents.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(params ExceptionAction<T>[] exceptionActions)
		{
			exceptionActions.Each(exceptionAction => _exceptionEvents.Add(exceptionAction.ExceptionType, exceptionAction));
		}

		public void HandleException(T stateMachine, Event @event, object parameter, Exception exception)
		{
			Type exceptionType = exception.GetType();
			if (_exceptionEvents.ContainsKey(exceptionType))
			{
				_exceptionEvents[exceptionType].Execute(stateMachine, @event, parameter, exception);
			}
			else
			{
				string message = string.Format("Exception occurred in {0} during state {1} while handling {2}",
				                               typeof (T).FullName,
				                               stateMachine.CurrentState.Name,
				                               @event.Name);

				throw new StateMachineException(message, exception);
			}
		}
	}
}