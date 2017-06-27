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
	using Magnum.Extensions;


	public class DataEventBinder<T, TKey, TEvent> :
		EventBinder<T, TKey, TEvent>
		where T : StateMachine<T>
	{
		readonly Func<TEvent, TKey> _keyAccessor;

		public DataEventBinder(Func<TEvent, TKey> keyAccessor)
		{
			_keyAccessor = keyAccessor;
		}

		public Func<TEventType, TKey> GetBinder<TEventType>()
		{
			return _keyAccessor.CastAs<Func<TEventType, TKey>>();
		}
	}
}