using System;

namespace Magnum.Calendar.Holidays
{
    public class FlagDayObservedCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.June) &&
                   IsMonday(dateToCheck) &&
                   dateToCheck.Day.Equals(15);
        }

        public override string HolidayName
        {
            get { return "Flag Day (Observed)"; }
        }
    }
}