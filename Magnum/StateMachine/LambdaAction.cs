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

	public class LambdaAction<T> :
		EventAction<T>
		where T : StateMachine<T>
	{
		private readonly Action<T> _action;

		public LambdaAction(Action<T> action)
		{
			_action = action;
		}

		public void Execute(T instance, Event @event, object parameter)
		{
			_action(instance);
		}
	}

	public class LambdaAction<T, TData> :
		EventAction<T>
		where T : StateMachine<T>
	{
		private readonly Action<T, DataEvent<T, TData>, TData> _action;

		public LambdaAction(Action<T, DataEvent<T, TData>, TData> action)
		{
			_action = action;
		}

		public void Execute(T instance, Event @event, object parameter)
		{
			_action(instance, @event as DataEvent<T, TData>, (parameter is TData) ? ((TData) parameter) : default(TData));
		}
	}
}