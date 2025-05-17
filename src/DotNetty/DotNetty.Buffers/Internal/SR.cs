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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.Resources;

namespace DotNetty.Buffers.Internal
{
  internal sealed partial class SR : Strings
  {
    // Needed for debugger integration
    internal static string GetResourceString(string resourceKey)
    {
      return GetResourceString(resourceKey, String.Empty);
    }

    internal static string GetResourceString(string resourceKey, string defaultString)
    {
      string resourceString = null;
      try { resourceString = ResourceManager.GetString(resourceKey, null); }
      catch (MissingManifestResourceException) { }

      if (defaultString is object && resourceKey.Equals(resourceString, StringComparison.Ordinal))
      {
        return defaultString;
      }

      return resourceString;
    }

    internal static string Format(string resourceFormat, params object[] args)
    {
      if (args is object)
      {
        return String.Format(resourceFormat, args);
      }

      return resourceFormat;
    }

    internal static string Format(string resourceFormat, object p1)
    {
      return String.Format(resourceFormat, p1);
    }

    internal static string Format(string resourceFormat, object p1, object p2)
    {
      return String.Format(resourceFormat, p1, p2);
    }

    internal static string Format(string resourceFormat, object p1, object p2, object p3)
    {
      return String.Format(resourceFormat, p1, p2, p3);
    }
  }
}
