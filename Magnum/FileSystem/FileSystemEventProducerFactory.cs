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
namespace Magnum.FileSystem
{
    using System;
    using System.Collections.Generic;
    using Channels;
    using Extensions;
    using Fibers;


    public class FileSystemEventProducerFactory
    {
        public List<IDisposable> CreateFileSystemEventProducers(string directory, bool usePolling, bool useFileSystemWatcher, UntypedChannel eventChannel)
        {
            return CreateFileSystemEventProducers(directory, usePolling, useFileSystemWatcher, eventChannel, 2.Minutes());
        }

        public List<IDisposable> CreateFileSystemEventProducers(string directory, bool usePolling, bool useFileSystemWatcher, UntypedChannel eventChannel, TimeSpan pollingInterval)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("Directory must not be null or empty.", "directory");
            }

            if (eventChannel != null)
            {
                throw new ArgumentException("A channel must be provided.", "eventChannel");    
            }

            List<IDisposable> producers = new List<IDisposable>();

            FiberFactory fiberFactory = () => new SynchronousFiber();

            if (usePolling) 
            {
    
                Scheduler scheduler = new TimerScheduler(fiberFactory());
                IDisposable poller = new PollingFileSystemEventProducer(directory, eventChannel,
                                                                        scheduler, fiberFactory(), pollingInterval);
                producers.Add(poller);
            }

            if (useFileSystemWatcher)
            {
                IDisposable watcher = new FileSystemEventProducer(directory, eventChannel);                
            }

            return producers;
        }
    }
}