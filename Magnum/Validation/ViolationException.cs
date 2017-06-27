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
namespace Magnum.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;


	[Serializable]
	public class ViolationException :
		ValidationException
	{
		public ViolationException()
		{
			Violations = new Violation[] {};
		}

		public ViolationException(IEnumerable<Violation> violations)
		{
			Violations = violations.ToArray();
		}

		public ViolationException(IEnumerable<Violation> violations, string message)
			: base(message)
		{
			Violations = violations.ToArray();
		}

		public ViolationException(string message, Exception innerException)
			: base(message, innerException)
		{
			Violations = new Violation[] {};
		}

		protected ViolationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Violations = new Violation[] {};
		}

		public IEnumerable<Violation> Violations { get; private set; }
	}
}