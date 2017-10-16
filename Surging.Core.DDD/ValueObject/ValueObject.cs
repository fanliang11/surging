using Surging.Core.DDD.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.DDD
{
    /// <summary>A DDD value object base class. Provide the mechanism to compare two objects by values.
    /// </summary>
    [Serializable]
    public abstract class ValueObject<T>: DomainObject where T : class
    {
        /// <summary>Returns all the atomic values of the current object.
        /// </summary>
        public abstract IEnumerable<object> GetAtomicValues();
        /// <summary>Clone a new object from the current object with the specified default values.
        /// </summary>
        /// <param name="objectContainsNewValues"></param>
        /// <returns></returns>
        public T Clone(object objectContainsNewValues = null)
        {
            var propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var newPropertyInfoArray = objectContainsNewValues != null ? objectContainsNewValues.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : null;
            var cloneObject = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null) as T;

            if (newPropertyInfoArray != null)
            {
                foreach (var propertyInfo in propertyInfos)
                {
                    var property = newPropertyInfoArray.FirstOrDefault(x => x.Name == propertyInfo.Name);
                    propertyInfo.SetValue(cloneObject, property != null ? property.GetValue(objectContainsNewValues, null) : propertyInfo.GetValue(this, null), null);
                }
            }
            else
            {
                foreach (var propertyInfo in propertyInfos)
                {
                    propertyInfo.SetValue(cloneObject, propertyInfo.GetValue(this, null), null);
                }
            }

            return cloneObject;
        }

        /// <summary>Operator overrides.
        /// </summary>
        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            return IsEqual(left, right);
        }
        /// <summary>Operator overrides.
        /// </summary>
        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !IsEqual(left, right);
        }
        /// <summary>Method overrides.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject<T>)obj;
            var enumerator1 = GetAtomicValues().GetEnumerator();
            var enumerator2 = other.GetAtomicValues().GetEnumerator();
            var enumerator1HasNextValue = enumerator1.MoveNext();
            var enumerator2HasNextValue = enumerator2.MoveNext();

            while (enumerator1HasNextValue && enumerator2HasNextValue)
            {
                if (ReferenceEquals(enumerator1.Current, null) ^ ReferenceEquals(enumerator2.Current, null))
                {
                    return false;
                }
                if (enumerator1.Current != null)
                {
                    if (enumerator1.Current is IList && enumerator2.Current is IList)
                    {
                        if (!CompareEnumerables(enumerator1.Current as IList, enumerator2.Current as IList))
                        {
                            return false;
                        }
                    }
                    else if (!enumerator1.Current.Equals(enumerator2.Current))
                    {
                        return false;
                    }
                }
                enumerator1HasNextValue = enumerator1.MoveNext();
                enumerator2HasNextValue = enumerator2.MoveNext();
            }

            return !enumerator1HasNextValue && !enumerator2HasNextValue;
        }
        /// <summary>Method overrides.
        /// </summary>
        public override int GetHashCode()
        {
            return GetAtomicValues().Select(x => x != null ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);
        }

        private static bool IsEqual(ValueObject<T> left, ValueObject<T> right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }
        private static bool CompareEnumerables(IEnumerable enumerable1, IEnumerable enumerable2)
        {
            if (enumerable1 == null) throw new ArgumentNullException("enumerable1");
            if (enumerable2 == null) throw new ArgumentNullException("enumerable2");

            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();
            var enumerator1HasNextValue = enumerator1.MoveNext();
            var enumerator2HasNextValue = enumerator2.MoveNext();

            while (enumerator1HasNextValue && enumerator2HasNextValue)
            {
                if (ReferenceEquals(enumerator1.Current, null) ^ ReferenceEquals(enumerator2.Current, null))
                {
                    return false;
                }
                if (enumerator1.Current != null && enumerator2.Current != null)
                {
                    if (enumerator1.Current is IList && enumerator2.Current is IList)
                    {
                        if (!CompareEnumerables(enumerator1.Current as IList, enumerator2.Current as IList))
                        {
                            return false;
                        }
                    }
                    else if (!enumerator1.Current.Equals(enumerator2.Current))
                    {
                        return false;
                    }
                }
                enumerator1HasNextValue = enumerator1.MoveNext();
                enumerator2HasNextValue = enumerator2.MoveNext();
            }

            return !enumerator1HasNextValue && !enumerator2HasNextValue;
        }
    }
}
