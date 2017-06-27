namespace Magnum.Calendar.Holidays
{
    using System;

    public class MartinLutherKingDayCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {
            return dateToCheck.Month == 1 &&
                   IsMonday(dateToCheck) &&
                   ((dateToCheck.DayOfYear - 1)/7) == 2;
        }

        public override string HolidayName
        {
            get { return "Martin Luther King Jr. Day (Observed)"; }
        }
    }
}