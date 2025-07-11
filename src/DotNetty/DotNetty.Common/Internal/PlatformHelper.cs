using System;

namespace DotNetty.Common.Internal
{
	/// <summary>A helper class to get the number of processors, it updates the numbers of processors every sampling interval.</summary>
	public static class PlatformHelper
	{
		private const Int32 PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000; // How often to refresh the count, in milliseconds.
		private static volatile Int32 s_processorCount; // The last count seen.
		private static volatile Int32 s_lastProcessorCountRefreshTicks; // The last time we refreshed.

		/// <summary>Gets the number of available processors</summary>
		//[SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
		public static Int32 ProcessorCount
		{
			get
			{
				Int32 now = Environment.TickCount;
				Int32 procCount = s_processorCount;
				if (procCount == 0 || (now - s_lastProcessorCountRefreshTicks) >= PROCESSOR_COUNT_REFRESH_INTERVAL_MS)
				{
					s_processorCount = procCount = Environment.ProcessorCount;
					s_lastProcessorCountRefreshTicks = now;
				}

				//Contract.Assert(procCount > 0 && procCount <= 64,
				//		"Processor count not within the expected range (1 - 64).");

				return procCount;
			}
		}

		/// <summary>Gets whether the current machine has only a single processor.</summary>
		public static Boolean IsSingleProcessor
		{
			get { return ProcessorCount == 1; }
		}
	}
}
