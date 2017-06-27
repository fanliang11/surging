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
namespace Magnum.Threading
{
	using System;
	using System.Collections.Generic;

	public interface IAsyncExecutor
	{
		/// <summary>
		/// Executes a method containing yields, but waits for said method to complete
		/// </summary>
		/// <param name="enumerator">The method to execute</param>
		void Execute(IEnumerator<int> enumerator);

		/// <summary>
		/// Asynchronously executes a method containing yields
		/// </summary>
		/// <param name="enumerator">The method to execute</param>
		/// <param name="callback">The asynchronous method to call back</param>
		/// <param name="state">A state object passed to the asynchronous callback</param>
		/// <returns></returns>
		IAsyncResult BeginExecute(IEnumerator<int> enumerator, AsyncCallback callback, object state);

		/// <summary>
		/// Completes the execution of an asynchronous methods
		/// </summary>
		/// <param name="asyncResult"></param>
		void EndExecute(IAsyncResult asyncResult);

		/// <summary>
		/// Cancel the execution of an asynchronous method
		/// </summary>
		void Cancel();

		/// <summary>
		/// Returns a callback for the BeginXXX method being yielded
		/// </summary>
		/// <returns></returns>
		AsyncCallback End();

		/// <summary>
		/// Returns the next async result in the queue of completed asynchronous methods
		/// </summary>
		/// <returns></returns>
		IAsyncResult Result();
	}
}