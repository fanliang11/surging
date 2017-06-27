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
namespace Magnum.Reflection
{
	public interface IFastActivator
	{
		object Create();
		object Create(object[] args);
		object Create<TArg0>(TArg0 arg0);
		object Create<TArg0, TArg1>(TArg0 arg0, TArg1 arg1);
	}

	public interface IFastActivator<T> :
		IFastActivator
	{
		new T Create();
		new T Create(object[] args);
		new T Create<TArg0>(TArg0 arg0);
		new T Create<TArg0, TArg1>(TArg0 arg0, TArg1 arg1);
	}
}