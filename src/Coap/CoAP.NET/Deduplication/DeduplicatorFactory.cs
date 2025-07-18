/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Deduplication
{
    static class DeduplicatorFactory
    { 
        public const String MarkAndSweepDeduplicator = "MarkAndSweep";
        public const String CropRotationDeduplicator = "CropRotation";
        public const String NoopDeduplicator = "Noop";

        public static IDeduplicator CreateDeduplicator(ICoapConfig config)
        { 
            String type = config.Deduplicator;
            if (String.Equals(MarkAndSweepDeduplicator, type, StringComparison.OrdinalIgnoreCase)
                || String.Equals("DEDUPLICATOR_MARK_AND_SWEEP", type, StringComparison.OrdinalIgnoreCase))
                return new SweepDeduplicator(config);
            else if (String.Equals(CropRotationDeduplicator, type, StringComparison.OrdinalIgnoreCase)
                || String.Equals("DEDUPLICATOR_CROP_ROTATIO", type, StringComparison.OrdinalIgnoreCase))
                return new CropRotation(config);
            else if (!String.Equals(NoopDeduplicator, type, StringComparison.OrdinalIgnoreCase)
                && !String.Equals("NO_DEDUPLICATOR", type, StringComparison.OrdinalIgnoreCase))
            {
            }
            return new NoopDeduplicator();
        }
    }
}
