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
namespace Magnum.Policies
{
	public static class PolicyBuilder
	{
		public static PolicyBuilder<ExceptionHandler> For<T>(ExceptionHandler condition)
		{
			return new PolicyBuilder<ExceptionHandler>(condition);
		}
	}

	public class PolicyBuilder<T>
	{
		public PolicyBuilder(T condition)
		{
			Condition = condition;
		}

		public T Condition { get; private set; }
	}
}