using System;

namespace Magnum.Calendar.Holidays
{
    public class FlagDayCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.June) &&
                   dateToCheck.Day.Equals(14);
        }

        public override string HolidayName
        {
            get { return "Flag Day"; }
        }
    }
}