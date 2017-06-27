using System;

namespace Magnum.Calendar.Holidays
{
    public class ChristmasDayObserved :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.December) &&
                   IsMonday(dateToCheck) &&
                   dateToCheck.Day.Equals(26);
        }

        public override string HolidayName
        {
            get { return "Chistmas Day (Observed)"; }
        }
    }
}