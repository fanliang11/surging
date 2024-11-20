using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Extensions.Utils
{
    public class DateUtils
    {
        private readonly TimeSpan _OneMinute = new TimeSpan(0, 1, 0);
        private readonly TimeSpan _TwoMinutes = new TimeSpan(0, 2, 0);
        private readonly TimeSpan _OneHour = new TimeSpan(1, 0, 0);
        private readonly TimeSpan _TwoHours = new TimeSpan(2, 0, 0);
        private readonly TimeSpan _OneDay = new TimeSpan(1, 0, 0, 0);
        private readonly TimeSpan _TwoDays = new TimeSpan(2, 0, 0, 0);
        private readonly TimeSpan _OneWeek = new TimeSpan(7, 0, 0, 0);
        private readonly TimeSpan _TwoWeeks = new TimeSpan(14, 0, 0, 0);
        private readonly TimeSpan _OneMonth = new TimeSpan(31, 0, 0, 0);
        private readonly TimeSpan _TwoMonths = new TimeSpan(62, 0, 0, 0);
        private readonly TimeSpan _OneYear = new TimeSpan(365, 0, 0, 0);
        private readonly TimeSpan _TwoYears = new TimeSpan(730, 0, 0, 0);
        public TimeSpan GetTimeSpan(DateTime startTime, DateTime endTime)
        {
            return endTime - startTime;
        }
        public string ToDateTime(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public string ToDateTimeF(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss:fffffff");
        }
        public int CalculateAge(DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth, DateTime.Today);
        }
        public int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
        {
            int years = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.Month < dateOfBirth.Month || (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day)) --years;
            return years;
        }
        public int GetCountDaysOfMonth(DateTime date)
        {
            var nextMonth = date.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).Day;
        }
        public DateTime GetFirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
        public DateTime GetFirstDayOfMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = GetFirstDayOfMonth(date);
            while (dt.DayOfWeek != dayOfWeek)
            {
                dt = dt.AddDays(1);
            }
            return dt;
        }
        public DateTime GetLastDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, GetCountDaysOfMonth(date));
        }
        public DateTime GetLastDayOfMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = GetLastDayOfMonth(date);
            while (dt.DayOfWeek != dayOfWeek)
            {
                dt = dt.AddDays(-1);
            }
            return dt;
        }
        public bool IsToday(DateTime dt)
        {
            return (dt.Date == DateTime.Today);
        }
        public bool IsToday(DateTimeOffset dto)
        {
            return IsToday(dto.Date);
        }
        public DateTime SetTime(DateTime date, int hours, int minutes, int seconds)
        {
            return SetTime(date, new TimeSpan(hours, minutes, seconds));
        }
        public DateTime SetTime(DateTime date, TimeSpan time)
        {
            return date.Date.Add(time);
        }
        public DateTimeOffset ToDateTimeOffset(DateTime localDateTime)
        {
            return ToDateTimeOffset(localDateTime, null);
        }
        public DateTimeOffset ToDateTimeOffset(DateTime localDateTime, TimeZoneInfo localTimeZone)
        {
            localTimeZone = (localTimeZone ?? TimeZoneInfo.Local);

            if (localDateTime.Kind != DateTimeKind.Unspecified)
            {
                localDateTime = new DateTime(localDateTime.Ticks, DateTimeKind.Unspecified);
            }

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);
        }
        public DateTime GetFirstDayOfWeek(DateTime date)
        {
            return GetFirstDayOfWeek(date, null);
        }
        public DateTime GetFirstDayOfWeek(DateTime date, CultureInfo cultureInfo)
        {
            cultureInfo = (cultureInfo ?? CultureInfo.CurrentCulture);

            var firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            while (date.DayOfWeek != firstDayOfWeek) date = date.AddDays(-1);

            return date;
        }
        public DateTime GetLastDayOfWeek(DateTime date)
        {
            return GetLastDayOfWeek(date, null);
        }
        public DateTime GetLastDayOfWeek(DateTime date, CultureInfo cultureInfo)
        {
            return GetFirstDayOfWeek(date, cultureInfo).AddDays(6);
        }
        public DateTime GetWeekday(DateTime date, DayOfWeek weekday)
        {
            return GetWeekday(date, weekday, null);
        }
        public DateTime GetWeekday(DateTime date, DayOfWeek weekday, CultureInfo cultureInfo)
        {
            var firstDayOfWeek = GetFirstDayOfWeek(date, cultureInfo);
            return GetNextWeekday(firstDayOfWeek, weekday);
        }
        public static DateTime GetNextWeekday(DateTime date, DayOfWeek weekday)
        {
            while (date.DayOfWeek != weekday) date = date.AddDays(1);
            return date;
        }
        public DateTime GetPreviousWeekday(DateTime date, DayOfWeek weekday)
        {
            while (date.DayOfWeek != weekday) date = date.AddDays(-1);
            return date;
        }

        public DateTimeOffset SetTime(DateTimeOffset date, int hours, int minutes, int seconds)
        {
            return SetTime(date, new TimeSpan(hours, minutes, seconds));
        }
        public DateTimeOffset SetTime(DateTimeOffset date, TimeSpan time)
        {
            return SetTime(date, time, null);
        }
        public DateTimeOffset SetTime(DateTimeOffset date, TimeSpan time, TimeZoneInfo localTimeZone)
        {
            var localDate = ToLocalDateTime(date, localTimeZone);
            SetTime(localDate, time);
            return ToDateTimeOffset(localDate, localTimeZone);
        }
        public DateTime ToLocalDateTime(DateTimeOffset dateTimeUtc)
        {
            return ToLocalDateTime(dateTimeUtc, null);
        }


        public DateTime ToLocalDateTime(DateTimeOffset dateTimeUtc, TimeZoneInfo localTimeZone)
        {
            localTimeZone = (localTimeZone ?? TimeZoneInfo.Local);
            return TimeZoneInfo.ConvertTime(dateTimeUtc, localTimeZone).DateTime;
        }



        public static int WeekOfYear(DateTime datetime)
        {
            System.Globalization.DateTimeFormatInfo dateinf = new System.Globalization.DateTimeFormatInfo();
            System.Globalization.CalendarWeekRule weekrule = dateinf.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = dateinf.FirstDayOfWeek;
            System.Globalization.CultureInfo ciCurr = System.Globalization.CultureInfo.CurrentCulture;
            return ciCurr.Calendar.GetWeekOfYear(datetime, weekrule, firstDayOfWeek);
        }

        public bool IsWeekDay(DateTime date)
        {
            return !IsWeekend(date);
        }
        public bool IsWeekend(DateTime value)
        {
            return value.DayOfWeek == DayOfWeek.Sunday || value.DayOfWeek == DayOfWeek.Saturday;
        }

    }
}
