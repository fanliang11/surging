// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal.Logging
{
    using System;

    partial class GenericLogger
    {
        private static string MessageFormatterInternal(string message, Exception error) => message;
    }
}