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
namespace Magnum.Pipeline
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Provides an interface for adding consumers and interceptors to the pipeline
	/// </summary>
	public interface ISubscriptionScope :
		IDisposable
	{
		/// <summary>
		/// Subscribes the message consumer to the pipeline. Can accept any method that matches the delegate
		/// syntax, including anonymous/lambda methods
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="consumer">The method that will consume the message</param>
		void Subscribe<T>(MessageConsumer<T> consumer)
			where T : class;

		/// <summary>
		/// Subscribes an instance of a class to the pipeline. Consumer interfaces that are implemented by the 
		/// object will be wired up to the pipeline with the appropriate message type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="consumer"></param>
		void Subscribe<T>(T consumer)
			where T : class;

		/// <summary>
		/// Subscribes a type to the pipeline. When a message is delivered to the pipeline, the getConsumer function
		/// is called to get an instance of the consumer to which the message is delivered.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="getConsumer"></param>
		void Subscribe<T>(Func<T> getConsumer)
			where T : class;

		/// <summary>
		/// Registers an interceptor on the pipeline, allowing actions to be performed before and after messages
		/// are delivered on the pipeline.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="configureAction">The block containing the configuration calls for the interceptor</param>
		void Intercept<T>(Action<IInterceptorConfigurator<T>> configureAction)
			where T : class;
	}
}