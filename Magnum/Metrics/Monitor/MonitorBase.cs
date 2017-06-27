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
namespace Magnum.Metrics.Monitor
{
	using System;

	public class MonitorBase : 
		IMonitor
	{
		private readonly string _name;
		private readonly Type _ownerType;

		public MonitorBase(Type ownerType, string name)
		{
			_ownerType = ownerType;
			_name = name;
		}

		public Type OwnerType
		{
			get { return _ownerType; }
		}

		public string Name
		{
			get { return _name; }
		}
	}
}