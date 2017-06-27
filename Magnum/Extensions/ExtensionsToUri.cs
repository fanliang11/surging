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
namespace Magnum.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Net;


	public static class ExtensionsToUri
	{
		/// <summary>
		///   Appends a path to an existing Uri
		/// </summary>
		/// <param name = "uri"></param>
		/// <param name = "path"></param>
		/// <returns></returns>
		public static Uri AppendPath(this Uri uri, string path)
		{
			string absolutePath = uri.AbsolutePath.TrimEnd('/') + "/" + path;
			return new UriBuilder(uri.Scheme, uri.Host, uri.Port, absolutePath, uri.Query).Uri;
		}

		public static IEnumerable<IPEndPoint> ResolveHostName(this Uri uri)
		{
			if (uri.HostNameType == UriHostNameType.Dns)
			{
				IPAddress[] addresses = Dns.GetHostAddresses(uri.DnsSafeHost);
				if (addresses.Length == 0)
					throw new ArgumentException("The host could not be resolved: " + uri.DnsSafeHost, "uri");

				foreach (IPAddress address in addresses)
				{
					var endpoint = new IPEndPoint(address, uri.Port);
					yield return endpoint;
				}
			}
			else if (uri.HostNameType == UriHostNameType.IPv4)
			{
				IPAddress address = IPAddress.Parse(uri.Host);
				if (address == null)
					throw new ArgumentException("The IP address is invalid: " + uri.Host, "uri");

				var endpoint = new IPEndPoint(address, uri.Port);
				yield return endpoint;
			}
			else
				throw new ArgumentException("Could not determine host name type: " + uri.Host, "uri");
		}
	}
}