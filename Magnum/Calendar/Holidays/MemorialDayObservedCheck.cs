namespace Magnum.Calendar.Holidays
{
    using System;
    using Extensions;

	public class MemorialDayObservedCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return CheckMonth(dateToCheck, Months.May) &&
                dateToCheck.Last(DayOfWeek.Monday).Equals(dateToCheck);
        }

        public override string HolidayName
        {
            get { return "Memorial Day (Observed)"; }
        }
    }
}