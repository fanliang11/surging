using System;

namespace Magnum.Calendar.Holidays
{
    public class IndependenceDayObservedCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.July) &&
                   IsMonday(dateToCheck) &&
                   dateToCheck.Day.Equals(5);
        }

        public override string HolidayName
        {
            get { return "Independence Day (Observed)"; }
        }
    }
}