using System;
using System.Globalization;

namespace Magnum.Calendar.Holidays
{
    public abstract class BaseCheck
    {
        #region IDateSpecification Members

        public abstract bool Check(DateTime dateToCheck);
        public abstract string HolidayName { get; }

        #endregion

        public bool IsMonday(DateTime dateToCheck)
        {
            return dateToCheck.DayOfWeek.Equals(DayOfWeek.Monday);
        }

        public bool IsThursday(DateTime dateToCheck)
        {
            return dateToCheck.DayOfWeek.Equals(DayOfWeek.Thursday);
        }

        public bool IsWeekend(DateTime dateToCheck)
        {
            return dateToCheck.DayOfWeek.Equals(DayOfWeek.Saturday) || dateToCheck.DayOfWeek.Equals(DayOfWeek.Sunday);
        }

        public bool IsWeekday(DateTime dateToCheck)
        {
            return !IsWeekend(dateToCheck);
        }

        public bool CheckMonth(DateTime dateToCheck, Months month)
        {
            return dateToCheck.Month.Equals((int)month);
        }

        public int GetWeekNumberInYear(DateTime dateToCheck)
        {
            System.Globalization.Calendar cal = new GregorianCalendar();
            int weekNumber = cal.GetWeekOfYear(dateToCheck, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);
            return weekNumber;
        }
    }
}