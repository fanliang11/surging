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
namespace Magnum.StateMachine
{
	using System;
	using System.Runtime.Serialization;
	using Extensions;


	[Serializable]
	public class StateMachineException :
		Exception
	{
		public StateMachineException(string message)
			: base(message)
		{
		}

		public StateMachineException()
		{
		}

		public StateMachineException(Type type, string message)
			: this("{0}: {1}".FormatWith(message, type.ToShortTypeName()))
		{
		}

		public StateMachineException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected StateMachineException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}