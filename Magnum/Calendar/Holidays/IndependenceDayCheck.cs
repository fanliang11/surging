using System;

namespace Magnum.Calendar.Holidays
{
    public class IndependenceDayCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.July) &&
                   dateToCheck.Day.Equals(4);
        }

        public override string HolidayName
        {
            get { return "Independence Day"; }
        }
    }
}