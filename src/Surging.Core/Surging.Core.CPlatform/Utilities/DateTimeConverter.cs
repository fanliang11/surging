using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Utilities
{
   public class DateTimeConverter
    {
        public static long DateTimeToUnixTimestamp( DateTime dateTime)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            return Convert.ToInt64((dateTime - start).TotalSeconds);
        }
         
        public static DateTime UnixTimestampToDateTime( long timestamp, DateTime? time=null)
        {
            var start = time ==null?new DateTime(1970, 1, 1, 0, 0, 0): new DateTime(1970, 1, 1, 0, 0, 0,time.Value.Kind);
            return start.AddSeconds(timestamp);
        }
    }
}
