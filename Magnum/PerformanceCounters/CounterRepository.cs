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
namespace Magnum.PerformanceCounters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Reflection;


    public class CounterRepository :
        IDisposable
    {
        bool _disposed;
        readonly Dictionary<Type, CounterCategory> _counterCache = new Dictionary<Type, CounterCategory>();

        public void RegisterCategory(CategoryConfiguration categoryConfiguration)
        {
        	if (!PerformanceCounterCategory.Exists(categoryConfiguration.Name))
        	{
        		PerformanceCounterCategory.Create(
        		                                  categoryConfiguration.Name,
        		                                  categoryConfiguration.Help,
        		                                  PerformanceCounterCategoryType.MultiInstance,
        		                                  new CounterCreationDataCollection(
        		                                  	categoryConfiguration.Counters.Select(x => (CounterCreationData)x).ToArray()));
        		return;
        	}


        	CleanUpCategory(categoryConfiguration);
        }

    	static void CleanUpCategory(CategoryConfiguration categoryConfiguration)
        {
            int missing = categoryConfiguration.Counters
                    .Where(counter => !PerformanceCounterCategory.CounterExists(counter.Name, categoryConfiguration.Name))
                    .Count();

                if (missing > 0)
                {
                    PerformanceCounterCategory.Delete(categoryConfiguration.Name);

                    PerformanceCounterCategory.Create(
                                                      categoryConfiguration.Name,
                                                      categoryConfiguration.Help,
                                                      PerformanceCounterCategoryType.MultiInstance,
                                                      new CounterCreationDataCollection(
                                                          categoryConfiguration.Counters.Select(x => (CounterCreationData)x).ToArray()));
                }
        }

        public void RemoveCategory<TCategory>() where TCategory : CounterCategory
        {
            RemoveCategory(typeof(TCategory).Name);
        }

        public void RemoveCategory(string categoryName)
        {
                PerformanceCounterCategory.Delete(categoryName);
        }

    	public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Close()
        {
            foreach (var category in _counterCache.Values)
            {
                // for each property dispose of the instance counters
                foreach (var propertyInfo in category.GetType().GetProperties())
                {
                    var fp = new FastProperty(propertyInfo);
                    ((Counter)fp.Get(category)).Dispose();
                }
            }
                
            _counterCache.Clear();
        }
        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
                Close();

            _disposed = true;
        }
        ~CounterRepository()
        {
            Dispose(false);
        }


    	public TCounterCategory GetCounter<TCounterCategory>(string instance) where TCounterCategory : class, CounterCategory, new()
        {
            Type t = typeof(TCounterCategory);
            CounterCategory value;
            if (_counterCache.TryGetValue(t, out value))
            {
                return (TCounterCategory)value;
            }

            //cache miss

            var cat = new TCounterCategory();
            var categoryName = t.Name;
            var props = t.GetProperties();


            foreach (var propertyInfo in props)
            {
                var fp = new FastProperty<TCounterCategory>(propertyInfo);
                fp.Set(cat, CreateCounter(categoryName, propertyInfo.Name, instance));
            }

            _counterCache.Add(t, cat);

            return cat;
        }

        static Counter CreateCounter(string categoryName, string counterName, string instanceName)
        {
            try
            {
                
                var instancePerformanceCounter = new InstancePerformanceCounter(categoryName,
                                                                                counterName,
                                                                                instanceName);

                return instancePerformanceCounter;
            }
            catch (InvalidOperationException ex)
            {
                return new NullPerformanceCounter();
            }
        }
    }
}