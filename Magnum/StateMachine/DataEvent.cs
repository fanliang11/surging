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

	/// <summary>
	/// An event that has a typed data structure associated with it
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="V"></typeparam>
	public class DataEvent<T, V> :
		BasicEvent<T>,
		Event<V>
		where T : StateMachine<T>
	{
		public DataEvent(string name)
			: base(name)
		{
		}

		public new static DataEvent<T, V> GetEvent(Event input)
		{
			DataEvent<T, V> result = input as DataEvent<T, V>;
			if (result == null)
				throw new ArgumentException("The event is not valid for this state machine " + input.Name);

			return result;
		}
	}
}