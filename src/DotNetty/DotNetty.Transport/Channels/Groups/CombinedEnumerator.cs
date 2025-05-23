/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels.Groups
{
    using System.Collections;
    using System.Collections.Generic;

    public sealed class CombinedEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _e1;
        private readonly IEnumerator<T> _e2;
        private IEnumerator<T> _currentEnumerator;

        public CombinedEnumerator(IEnumerator<T> e1, IEnumerator<T> e2)
        {
            if (e1 is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.e1); }
            if (e2 is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.e2); }
            _e1 = e1;
            _e2 = e2;
            _currentEnumerator = e1;
        }

        public T Current => _currentEnumerator.Current;

        public void Dispose() => _currentEnumerator.Dispose();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            while (true)
            {
                if (_currentEnumerator.MoveNext())
                {
                    return true;
                }
                if (_currentEnumerator == _e1)
                {
                    _currentEnumerator = _e2;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Reset() => _currentEnumerator.Reset();
    }
}