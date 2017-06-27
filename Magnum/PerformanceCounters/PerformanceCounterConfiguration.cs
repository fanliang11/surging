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
    using System.Diagnostics;

    /// <summary>
    /// This class encapsulates the data needed to create a counter in Windows
    /// </summary>
    public class PerformanceCounterConfiguration
    {
        readonly PerformanceCounterType _counterType;
        readonly string _help;
        readonly string _name;

        public PerformanceCounterConfiguration(string name, string help, PerformanceCounterType counterType)
        {
            _name = name;
            _help = help;
            _counterType = counterType;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Help
        {
            get { return _help; }
        }

        public static implicit operator CounterCreationData(PerformanceCounterConfiguration counterConfiguration)
        {
            return new CounterCreationData(counterConfiguration._name, counterConfiguration._help, counterConfiguration._counterType);
        }
    }
}