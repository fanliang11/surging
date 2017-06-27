using System;

namespace Magnum.Calendar.Holidays
{
    public class EasterCheck :
        BaseCheck
    {
        public override bool Check(DateTime dateToCheck)
        {

            DateTime easter = getEasterWestern(dateToCheck.Year);
            return dateToCheck.Month.Equals(easter.Month) && dateToCheck.Day.Equals(easter.Day);
        }

        //http://www.codeproject.com/KB/datetime/christianholidays.aspx?fid=194652&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=1162854#xx1162854xx
        public DateTime getEasterWestern(int year)
        {
            int g = year % 19;
            int c = intDiv(year, 100);
            int h = (c - intDiv(c, 4) - intDiv(8 * c + 13, 25) + 19 * g + 15) % 30;
            int i = h - intDiv(h, 28) * (1 - intDiv(h, 28)
                                             * intDiv(29, h + 1) * intDiv(21 - g, 11));
            int j = (year + intDiv(year, 4) + i + 2 - c + intDiv(c, 4)) % 7;
            int p = i - j + 28;

            int day = p;
            int month = 4;
            if (p > 31)
                day = p - 31;
            else
                month = 3;

            return new DateTime(year, month, day);
        }

        private int intDiv(int num, int dvsr)
        {
            bool negate = false;
            int result;

            if (dvsr == 0)
            {
                return -99999;
            }
            
            
            
            if (num * dvsr < 0)
            {
                negate = true;
            }
            if (num < 0)
            {
                num = -num;
            }
            if (dvsr < 0)
            {
                dvsr = -dvsr;
            }
            result = ((num - (num % dvsr)) / dvsr);
            
            if (negate)
            {
                return -result;
            }
            
            return result;
        }

        public override string HolidayName
        {
            get { return "Easter"; }
        }
    }
}