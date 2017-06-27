using System;

namespace Magnum.Calendar.Holidays
{
    public class ColumbusDayObserved :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.October) &&
                   GetWeekNumberInYear(dateToCheck).Equals(2) &&
                   IsMonday(dateToCheck) &&
                   WhatIsThisChecking(dateToCheck);
        }

        private bool WhatIsThisChecking(DateTime dateToCheck)
        {
            return ((dateToCheck.Day - 1)/7) == 1;
        }
        public override string HolidayName
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}