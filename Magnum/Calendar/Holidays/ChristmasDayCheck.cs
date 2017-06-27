using System;

namespace Magnum.Calendar.Holidays
{
    public class ChristmasDayCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.December) &&
                   dateToCheck.Day.Equals(25) &&
                   IsWeekday(dateToCheck);
        }

        public override string HolidayName
        {
            get { return "Chistmas Day"; }
        }
    }
}