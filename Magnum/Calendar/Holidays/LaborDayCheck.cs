namespace Magnum.Calendar.Holidays
{
    using System;

    public class LaborDayCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return GetWeekNumberInYear(dateToCheck).Equals(2)
                   && ((dateToCheck.Day - 1)/7) == 0;
        }

        public override string HolidayName
        {
            get { return "Labor Day"; }
        }
    }
}