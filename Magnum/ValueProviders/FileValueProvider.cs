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
namespace Magnum.ValueProviders
{
	using System;
	using System.IO;
	using Extensions;


	/// <summary>
	/// Wraps the reading of a file containing value configuration, passing the appropriate context
	/// provider that is initialized from the stream.
	/// </summary>
	public class FileValueProvider :
		ValueProviderDecorator
	{
		readonly string _filename;

		public FileValueProvider(string filename, Func<Stream, ValueProvider> createProviderFromStream)
			: base(CreateDictionaryProvider(filename, createProviderFromStream))
		{
			_filename = filename;
		}

		protected override string ProviderName
		{
			get { return "file ({0})".FormatWith(_filename); }
		}

		static ValueProvider CreateDictionaryProvider(string filename, Func<Stream, ValueProvider> createProviderFromStream)
		{
			var fileInfo = new FileInfo(filename);
			if (!fileInfo.Exists)
				throw new ArgumentException("Unable to access file: " + fileInfo.FullName, "filename");

			using (FileStream stream = fileInfo.OpenRead())
				return createProviderFromStream(stream);
		}
	}
}