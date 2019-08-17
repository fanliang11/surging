using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
   public static class SegmentSpanExtensions
    {
        public static void ErrorOccurred(this SegmentSpan span, Exception exception = null)
        {
            if (span == null)
            {
                return;
            }

            span.IsError = true;
            if (exception != null)
            {
                span.AddLog(LogEvent.Event("error"),
                    LogEvent.ErrorKind(exception.GetType().FullName),
                    LogEvent.Message(exception.Message),
                    LogEvent.ErrorStack(exception.StackTrace));
            }
        }
    }
}
