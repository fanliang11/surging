﻿/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */


using System.Collections;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Diagnostics
{
    public class SegmentReference
    {
        public Reference Reference { get; set; }

        public UniqueId TraceId { get; set; }

        public int ParentServiceId { get; set; }

        public UniqueId ParentSegmentId { get; set; }

        public int ParentSpanId { get; set; }

        public int ParentServiceInstanceId { get; set; }

        public int EntryServiceInstanceId { get; set; }

        public StringOrIntValue NetworkAddress { get; set; }

        public StringOrIntValue EntryEndpoint { get; set; }

        public StringOrIntValue ParentEndpoint { get; set; }
    }

    public enum Reference
    {
        CrossProcess = 0,
        CrossThread = 1
    }

    public class SegmentReferenceCollection : IEnumerable<SegmentReference>
    {
        private readonly HashSet<SegmentReference> _references = new HashSet<SegmentReference>();

        public bool Add(SegmentReference reference)
        {
            return _references.Add(reference);
        }

        public IEnumerator<SegmentReference> GetEnumerator()
        {
            return _references.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _references.GetEnumerator();
        }

        public int Count => _references.Count;
    }
}
