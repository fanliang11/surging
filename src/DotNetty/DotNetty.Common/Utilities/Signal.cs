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

namespace DotNetty.Common.Utilities
{
    using System;

    public sealed class Signal : Exception, IConstant, IComparable, IComparable<Signal>
    {
        static readonly SignalConstantPool Pool = new SignalConstantPool();

        sealed class SignalConstantPool : ConstantPool
        {
            protected override IConstant NewConstant<T>(int id, string name) => new Signal(id, name);
        };

        public static Signal ValueOf(string name) => (Signal)Pool.ValueOf<Signal>(name);

        public static Signal ValueOf(Type firstNameComponent, string secondNameComponent) => (Signal)Pool.ValueOf<Signal>(firstNameComponent, secondNameComponent);

        readonly SignalConstant constant;

        Signal(int id, string name)
        {
            this.constant = new SignalConstant(id, name);
        }

        public void Expect(Signal signal)
        {
            if (!ReferenceEquals(this, signal))
            {
                ThrowHelper.ThrowInvalidOperationException_Unexpected(signal);
            }
        }

        public int Id => this.constant.Id;

        public string Name => this.constant.Name;

        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public bool Equals(IConstant other) => ReferenceEquals(this, other);

        public override int GetHashCode() => this.Id;

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }
            if (obj is object && obj is Signal signal)
            {
                return this.CompareTo(signal);
            }

            return ThrowHelper.FromException_CompareSignal();
        }

        public int CompareTo(Signal other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            return this.constant.CompareTo(other.constant);
        }

        public override string ToString() => this.Name;

        sealed class SignalConstant : AbstractConstant<SignalConstant>
        {
            public SignalConstant(int id, string name) : base(id, name)
            {
            }
        }
    }
}
