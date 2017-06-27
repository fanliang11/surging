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
namespace Magnum.Collections
{
	/// <summary>
	/// A holder class for localizable strings that are used. Currently, these are not loaded from resources, but 
	/// just coded into this class. To make this library localizable, simply change this class to load the
	/// given strings from resources.
	/// </summary>
	internal static class Strings
	{
		public static readonly string ArgMustNotBeNegative = "The argument may not be less than zero.";
		public static readonly string ArrayTooSmall = "The array is too small to hold all of the items.";
		public static readonly string BadComparandType = "Comparand is not of the correct type.";
		public static readonly string CannotModifyCollection = "The \"{0}\" collection is read-only and cannot be modified.";
		public static readonly string CapacityLessThanCount = "The capacity may not be less than Count.";
		public static readonly string ChangeDuringEnumeration = "Collection was modified during an enumeration.";
		public static readonly string CollectionIsEmpty = "The collection is empty.";
		public static readonly string CollectionIsReadOnly = "The collection may not be read only.";
		public static readonly string CollectionTooLarge = "The collection has become too large.";
		public static readonly string IdentityComparerNoCompare = "The Compare method is not supported on an identity comparer.";
		public static readonly string InconsistentComparisons = "The two collections cannot be combined because they use different comparison operations.";
		public static readonly string InvalidLoadFactor = "The load factor must be between 0.25 and 0.95.";
		public static readonly string KeyAlreadyPresent = "The key was already present in the dictionary.";
		public static readonly string KeyNotFound = "The key was not found in the collection.";
		public static readonly string ListIsReadOnly = "The list may not be read only.";
		public static readonly string MustOverrideIndexerGet = "The get accessor of the indexer must be overridden.";
		public static readonly string MustOverrideIndexerSet = "The set accessor of the indexer must be overridden.";
		public static readonly string MustOverrideOrReimplement = "This method must be overridden or re-implemented in the derived class.";
		public static readonly string OutOfViewRange = "The argument is outside the range of this View.";
		public static readonly string ResetNotSupported = "Reset is not supported on this enumerator.";
		public static readonly string TypeNotCloneable = "Type \"{0}\" does not implement ICloneable.";
		public static readonly string UncomparableType = "Type \"{0}\" does not implement IComparable<{0}> or IComparable.";
		public static readonly string WrongType = "The value \"{0}\" isn't of type \"{1}\" and can't be used in this generic collection.";
	}
}