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
namespace Magnum.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class FastCollection 
    {
        private Type CollectionType { get; set; }
        private Action<object, object> AddDelegate { get; set; }
        private Action<object, object> RemoveDelegate { get; set; }
        private readonly Type listType = typeof(IList);
        private readonly Type objectType = typeof(object);

        public FastCollection(Type typeToManipulate)
        {
            CollectionType = typeToManipulate;


            if (!listType.IsAssignableFrom(CollectionType))
                throw new Exception("alice");

            InitializeAdd();
            InitializeRemove();
        }

        private void InitializeAdd()
        {
            var addMethod = listType.GetMethod("Add");

            var instance = Expression.Parameter(CollectionType, "instance");
            var value = Expression.Parameter(objectType, "item");
            AddDelegate = Expression.Lambda<Action<object, object>>(Expression.Call(instance, addMethod, value), new[] { instance, value }).Compile();

        }
        private void InitializeRemove()
        {
            var removeMethod = listType.GetMethod("Remove");

            var instance = Expression.Parameter(CollectionType, "instance");
            var value = Expression.Parameter(objectType, "item");
            RemoveDelegate = Expression.Lambda<Action<object, object>>(Expression.Call(instance, removeMethod, value), new[] { instance, value }).Compile();
        }

        public void Add(object instance, object value)
        {
            AddDelegate(instance, value);
        }

        public void Remove(object instance, object value)
        {
            RemoveDelegate(instance, value);
        } 
    }

    public class FastCollection<TCollection> where TCollection : IList
    {
        private Type CollectionType { get; set; }
        private Action<TCollection, object> AddDelegate { get; set; }
        private Action<TCollection, object> RemoveDelegate { get; set; }
        private readonly Type listType = typeof (IList);
        private readonly Type objectType = typeof (object);

        public FastCollection()
        {
            CollectionType = typeof(TCollection);


            if (!listType.IsAssignableFrom(CollectionType))
                throw new Exception("alice");

            InitializeAdd();
            InitializeRemove();
        }

        private void InitializeAdd()
        {
            var addMethod = listType.GetMethod("Add");

            var instance = Expression.Parameter(listType, "instance");
            var value = Expression.Parameter(objectType, "item");
            AddDelegate = Expression.Lambda<Action<TCollection, object>>(Expression.Call(instance, addMethod, value), new[] { instance, value }).Compile();

        }
        private void InitializeRemove()
        {
            var removeMethod = listType.GetMethod("Remove");

            var instance = Expression.Parameter(listType, "instance");
            var value = Expression.Parameter(objectType, "item");
            RemoveDelegate = Expression.Lambda<Action<TCollection, object>>(Expression.Call(instance, removeMethod, value), new[] { instance, value }).Compile();
        }

        public void Add(TCollection instance, object value)
        {
            AddDelegate(instance, value);
        }

        public void Remove(TCollection instance, object value)
        {
            RemoveDelegate(instance, value);
        }        
    }

    public class FastCollection<TCollection, TElement> where TCollection : ICollection<TElement>
    {
        private Type CollectionType { get; set; }
        private Action<TCollection, TElement> AddDelegate { get; set;}
        private Action<TCollection, TElement> RemoveDelegate { get; set; }

        public FastCollection()
        {
            CollectionType = typeof (TCollection);


            if(!typeof(ICollection<TElement>).IsAssignableFrom(CollectionType))
                throw new Exception("alice");

            InitializeAdd();
            InitializeRemove();
        }

        private void InitializeAdd()
        {
            var addMethod = typeof(ICollection<TElement>).GetMethod("Add");

            var instance = Expression.Parameter(typeof(ICollection<TElement>), "instance");
            var value = Expression.Parameter(typeof(TElement), "item");
            AddDelegate = Expression.Lambda<Action<TCollection, TElement>>(Expression.Call(instance, addMethod, value), new[] { instance, value }).Compile();

        }
        private void InitializeRemove()
        {
            var removeMethod = typeof(ICollection<TElement>).GetMethod("Remove");

            var instance = Expression.Parameter(typeof(ICollection<TElement>), "instance");
            var value = Expression.Parameter(typeof(TElement), "item");
            RemoveDelegate = Expression.Lambda<Action<TCollection, TElement>>(Expression.Call(instance, removeMethod, value), new[] { instance, value }).Compile();
        }

        public void Add(TCollection instance, TElement value)
        {
            AddDelegate(instance, value);
        }

        public void Remove(TCollection instance, TElement value)
        {
            RemoveDelegate(instance, value);
        }
    }
}