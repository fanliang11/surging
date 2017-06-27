using System;
using System.Collections.Generic;

namespace Magnum.Calendar
{
    using System.Collections;
    using Extensions;

	public class DefaultCalendar
    {
        private static readonly IList<IDateSpecification> _dateSpecifications;

        static DefaultCalendar()
        {
            _dateSpecifications = new List<IDateSpecification>();
        }

        public static void Define(Action<IHolidayConfigurator> configuration)
        {
            HolidayConfigurator cfg = new HolidayConfigurator();
            configuration(cfg);
            foreach (var spec in cfg.DateSpecifications)
            {
                _dateSpecifications.Add(spec);
            }
        }

        public static CheckResult Check(DateTime dateToCheck)
        {
            var result = new CheckResult();

            foreach (var check in _dateSpecifications)
            {
                if(check.Check(dateToCheck))
                {
                    result = new CheckResult(true, check.Description);
                    break;
                }
                    
            }

            return result;
        }
    }

    public class HolidayConfigurator :
        IHolidayConfigurator
    {
        public HolidayConfigurator()
        {
            DateSpecifications= new List<IDateSpecification>();
        }

        public void AddHolidays(IList<IDateSpecification> holidays)
        {
            foreach (var holidayCheck in holidays)
            {
                DateSpecifications.Add(holidayCheck);
            }
        }

        public void AddHoliday(string name, DateSpecification check)
        {
            DateSpecifications.Add(check);
        }

        public IList<IDateSpecification> DateSpecifications
        {
            get; private set;
        }
    }

    public static class Month
    {
        public static DateSpecification IsJanuary(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is January", d=>d.Month == (int)Months.January, daySpecs);
        }
        public static DateSpecification IsFebruary(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is Feburary", d => d.Month == (int)Months.Feburary, daySpecs);
        }

        public static DateSpecification IsMay(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is May", d => d.Month == (int)Months.May, daySpecs);
        }
        public static DateSpecification IsJune(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is June", d => d.Month == (int)Months.June, daySpecs);
        }
        public static DateSpecification IsJuly(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is July", d => d.Month == (int) Months.July, daySpecs);
        }
        public static DateSpecification IsAugust(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is August", d => d.Month == (int) Months.August, daySpecs);
        }
        public static DateSpecification IsSeptember(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is September", d => d.Month == (int)Months.September, daySpecs);
        }
        public static DateSpecification IsOctober(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is October", d => d.Month == (int)Months.October, daySpecs);
        }
        public static DateSpecification IsNovember(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is November", d => d.Month == (int)Months.November, daySpecs);
        }
        public static DateSpecification IsDecember(Action<DaySpecs> daySpecs)
        {
            return IsMonth("Is December", d=> d.Month == (int)Months.December, daySpecs);
        }

        private static DateSpecification IsMonth(string description, Func<DateTime, bool> monthSpec, Action<DaySpecs> daySpecs)
        {
            DateSpecification dc = new DateSpecification(description, monthSpec);
            var spec = new DaySpecs();
            daySpecs(spec);

            spec.Configure(dc);

            return dc;
        }

        
    }
    public class DaySpecs
    {
        private IList<Func<DateTime, bool>>  _checks = new List<Func<DateTime, bool>>();

        public void IsMonday()
        {
            _checks.Add(d => d.DayOfWeek == DayOfWeek.Monday);
        }
        public void IsTuesday()
        {
            _checks.Add(d => d.DayOfWeek == DayOfWeek.Tuesday);
        }
        public void IsWednesday()
        {
            _checks.Add(d => d.DayOfWeek == DayOfWeek.Wednesday);
        }
        public void IsThursday()
        {
            _checks.Add(d => d.DayOfWeek == DayOfWeek.Thursday);
        }

        public void DayIs(int dayNumber)
        {
            _checks.Add(d=>d.Day == dayNumber);
        }

        public void Configure(DateSpecification check)
        {
            check.AddChecks(_checks);
        }

        //http://michaelthompson.org/technikos/holidays.php
        public void NthDayOfMonth(int nTh, DayOfWeek day)
        {
            _checks.Add(d=>d.DayOfWeek == day);
            _checks.Add(d=> d.Day >= (1+7*(nTh-1)));
        }
        public void IsWeekday()
        {
            _checks.Add(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);
        }
        public void IsWeekend()
        {
            _checks.Add(d => d.DayOfWeek == DayOfWeek.Saturday && d.DayOfWeek == DayOfWeek.Sunday);
        }

        public void InRange(int start, int end)
        {
            _checks.Add(d=> start < d.Day && d.Day < end);
        }

        public void Custom(Func<DateTime, bool> func)
        {
            _checks.Add(func);
        }

        public void LastDayOfMonth(DayOfWeek dayOfWeek)
        {
            //last {day} of the given month
            _checks.Add(d=>d.DayOfWeek == dayOfWeek);
            _checks.Add(d=>d.Last(dayOfWeek).Equals(d));
        }
    }



    public interface IHolidayConfigurator
    {
        void AddHoliday(string name, DateSpecification check);
    }

    public interface IDateSpecification
    {
        bool Check(DateTime dateToCheck);
        string Description { get; }

    }
    public class DateSpecification :
        IDateSpecification
    {
        private IList<Func<DateTime, bool>> _checks = new List<Func<DateTime, bool>>();
        string _description; 

        public DateSpecification(string description, Func<DateTime, bool> monthCheck)
        {
            _checks.Add(monthCheck);
            _description = description;
        }

        public void AddChecks(IList<Func<DateTime, bool>> checks)
        {
            foreach (var check in checks)
            {
                _checks.Add(check);
            }
        }

        

        public bool Check(DateTime dateToCheck)
        {
            var result = false;

            foreach (var check in _checks)
            {
                if(check(dateToCheck))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public string Description
        {
            get { return _description; }
        }
    }
}