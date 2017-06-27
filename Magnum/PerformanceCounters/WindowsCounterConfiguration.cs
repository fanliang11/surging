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
    using System.Reflection;
    using Collections;
    using Extensions;
    using TypeScanning;


    public class WindowsCounterConfiguration :
        CounterConfiguration
    {
        //key: CategoryName
        //value: CounterCreationInfo
        readonly List<CategoryConfiguration> _categoryConfigurations =
            new List<CategoryConfiguration>();

        public void Register<TCounterCategory>() where TCounterCategory : CounterCategory
        {
            Register(typeof(TCounterCategory));
        }
        void Register(Type counterCategoryType)
        {
            var category = new CategoryConfiguration
                {
                    Name = counterCategoryType.Name
                };
            var props = counterCategoryType.GetProperties();
            foreach (var propertyInfo in props)
            {
                var counter = new PerformanceCounterConfiguration(propertyInfo.Name, "no-counter-help-yet",
                                                               PerformanceCounterType.NumberOfItems32);
                category.Counters.Add(counter);
            }
            _categoryConfigurations.Add(category);
            
        }
        public void ScanForCounters()
        {
            var types = TypeScanner.Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory();
                scan.AddAllTypesOf<CounterCategory>();
            });
            types.Each(Register);
        }


        public CounterRepository BuildRepository()
        {
            var repo = new CounterRepository();
            _categoryConfigurations.ForEach(repo.RegisterCategory);
            return repo;
        }
    }
}