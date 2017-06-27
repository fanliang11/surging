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
    using System.Linq;
    using Extensions;
    using Reflection;
    using Segments;

    public class SubscriptionScope :
        ISubscriptionScope
    {
        private readonly Pipe _pipe;
        private bool _disposed;
        private readonly IList<Pipe> _disposables;

        public SubscriptionScope(Pipe pipe)
        {
            _pipe = pipe;
            _disposables = new List<Pipe>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SubscriptionScope()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                var remover = new SegmentUnbinder(_disposables);
                remover.RemoveFrom(_pipe);

                _disposables.Clear();
            }

            _disposed = true;
        }

        public void Subscribe<T>(MessageConsumer<T> consumer)
            where T : class
        {
            Pipe segment = PipeSegment.Consumer(consumer);

            var binder = new SubscriberBinder(segment);
            binder.Bind(_pipe);

            _disposables.Add(segment);
        }

        public void Subscribe<T>(T consumer)
            where T : class
        {
            typeof (T).GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IConsumer<>))
				.Each(type => this.FastInvoke(new[] { null, type.GetGenericArguments()[0] }, "SubscribeConsumer", consumer));
        }

		public void Subscribe<T>(Func<T> getConsumer) 
			where T : class
		{
			typeof(T).GetInterfaces()
				.Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IConsumer<>))
				.Each(type => this.FastInvoke(new[] { null, type.GetGenericArguments()[0] }, "SubscribeComponent", getConsumer));
		}

    	// ReSharper disable UnusedMember.Local
        private void SubscribeConsumer<TConsumer, TMessage>(TConsumer consumer)
            // ReSharper restore UnusedMember.Local
            where TConsumer : IConsumer<TMessage>
            where TMessage : class
        {
            Pipe segment = PipeSegment.Consumer<TConsumer,TMessage>(consumer);

            var binder = new SubscriberBinder(segment);
            binder.Bind(_pipe);

            _disposables.Add(segment);
        }

        // ReSharper disable UnusedMember.Local
        private void SubscribeComponent<TConsumer, TMessage>(Func<TConsumer> getConsumer)
            // ReSharper restore UnusedMember.Local
            where TConsumer : IConsumer<TMessage>
            where TMessage : class
        {
			Pipe segment = PipeSegment.Consumer<TConsumer, TMessage>(getConsumer);

            var binder = new SubscriberBinder(segment);
            binder.Bind(_pipe);

            _disposables.Add(segment);
        }

        public void Intercept<T>(Action<IInterceptorConfigurator<T>> configureAction)
            where T : class
        {
            var binder = new InterceptionBinder(typeof (T), x =>
                {
                    var interceptor = PipeSegment.Interceptor(x, configureAction);
                    _disposables.Add(interceptor);

                    return interceptor;
                });

            binder.Bind(_pipe);
        }
    }
}