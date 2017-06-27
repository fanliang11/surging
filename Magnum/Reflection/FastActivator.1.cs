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
namespace Magnum.Reflection
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using Extensions;


	public class FastActivator<T> :
		FastActivatorBase,
		IFastActivator<T>
	{
		[ThreadStatic]
		static FastActivator<T> _current;

		readonly Dictionary<int, Func<object[], T>> _argGenerators;
		Func<T> _new;

		FastActivator()
			: base(typeof(T))
		{
			_argGenerators = new Dictionary<int, Func<object[], T>>();

			InitializeNew();
		}

		public static FastActivator<T> Current
		{
			get
			{
				if (_current == null)
					_current = new FastActivator<T>();

				return _current;
			}
		}

		object IFastActivator.Create()
		{
			return Create();
		}

		object IFastActivator.Create(object[] args)
		{
			return CreateFromArgs(args);
		}

		object IFastActivator.Create<TArg0>(TArg0 arg0)
		{
			return Create(arg0);
		}

		object IFastActivator.Create<TArg0, TArg1>(TArg0 arg0, TArg1 arg1)
		{
			return Create(arg0, arg1);
		}

		T IFastActivator<T>.Create()
		{
			return Create();
		}

		T IFastActivator<T>.Create(object[] args)
		{
			return CreateFromArgs(args);
		}

		T IFastActivator<T>.Create<TArg0>(TArg0 arg0)
		{
			return FastActivator<T, TArg0>.Create(arg0);
		}

		T IFastActivator<T>.Create<TArg0, TArg1>(TArg0 arg0, TArg1 arg1)
		{
			return FastActivator<T, TArg0, TArg1>.Create(arg0, arg1);
		}

		void InitializeNew()
		{
			_new = () =>
				{
					ConstructorInfo constructorInfo = Constructors
						.MatchingArguments()
						.SingleOrDefault();

                    if (constructorInfo == null)
                    {
                        _new = CreateUsingSerialization;
                    }
                    else
                    {
                        Func<T> lambda = Expression.Lambda<Func<T>>(Expression.New(constructorInfo)).Compile();

                        _new = lambda;
                    }

				    return _new();
				};
		}

        T CreateUsingSerialization()
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

		T CreateFromArgs(object[] args)
		{
            if (args == null || args.Length == 0)
                return _new();

			int offset = 0;
			int key = args.Aggregate(0, (x, o) => x ^ (o == null ? offset : o.GetType().GetHashCode() << offset++));

			Func<object[], T> generator = _argGenerators.Retrieve(key, () =>
				{
					ConstructorInfo constructorInfo = Constructors
						.MatchingArguments(args)
						.SingleOrDefault();

					if (constructorInfo == null)
						throw new FastActivatorException(typeof(T), "No usable constructor found");

					ParameterExpression argsParameter = Expression.Parameter(typeof(object[]), "args");

					Expression[] parameters = constructorInfo.GetParameters().ToArrayIndexParameters(argsParameter).ToArray();

					NewExpression newExpression = Expression.New(constructorInfo, parameters);

					Func<object[], T> lambda = Expression.Lambda<Func<object[], T>>(newExpression, argsParameter).Compile();

					return lambda;
				});

			return generator(args);
		}

		public static T Create()
		{
			return Current._new();
		}

		public static T Create(object[] args)
		{
			return Current.CreateFromArgs(args);
		}

		public static T Create<TArg0>(TArg0 arg0)
		{
			return FastActivator<T, TArg0>.Create(arg0);
		}

		public static T Create<TArg0, TArg1>(TArg0 arg0, TArg1 arg1)
		{
			return FastActivator<T, TArg0, TArg1>.Create(arg0, arg1);
		}
	}
}