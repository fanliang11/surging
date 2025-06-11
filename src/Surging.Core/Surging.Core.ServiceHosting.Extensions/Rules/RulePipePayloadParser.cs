using Jint;
using Surging.Core.ServiceHosting.Extensions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Extensions.Rules
{
    public class RulePipePayloadParser
    {
        private DateTime _currentTime = DateTime.Now;
        private ISubject<bool> _subject = new ReplaySubject<bool>();
        private LocalTimeZoneEnum _zoneEnum = LocalTimeZoneEnum.Local;
        private readonly List<Func<DateTime, bool>> _pipe = new List<Func<DateTime, bool>>();
        /// <summary>
        /// 立即执行
        /// </summary>
        public RulePipePayloadParser Immediately()
        {
            _pipe.Add(_lastExecTime =>
            {
                return true;
            });
            return this;
        }
        /// <summary>
        /// 相隔几秒执行一次
        /// </summary>
        public RulePipePayloadParser SecondAt(int value)
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Now.AddSeconds(value);
                return (DateTime.Now - lastExecTime).TotalSeconds >= value;
            });
            return this;
        }
        /// <summary>
        /// 每分钟执行一次任务
        /// </summary>
        public RulePipePayloadParser EveryMinute()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Now.AddMinutes(1);
                return (DateTime.Now - lastExecTime).TotalMinutes >= 1;
            });
            return this;
        }
        /// <summary>
        /// 每五分钟执行一次任务
        /// </summary>
        public RulePipePayloadParser EveryFiveMinutes()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Now.AddMinutes(5);
                return (DateTime.Now - lastExecTime).TotalMinutes >= 5;
            });
            return this;
        }
        /// <summary>
        /// 每十分钟执行一次任务
        /// </summary>
        public RulePipePayloadParser EveryTenMinutes()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Now.AddMinutes(10);
                return (DateTime.Now - lastExecTime).TotalMinutes >= 10;
            });
            return this;
        }
        /// <summary> 
        /// 每半小时执行一次任务
        /// </summary>
        public RulePipePayloadParser EveryThirtyMinutes()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Now.AddMinutes(30);
                return (DateTime.Now - lastExecTime).TotalMinutes >= 30;
            });
            return this;
        }
        /// <summary>
        /// 每小时执行一次任务
        /// </summary>
        public RulePipePayloadParser Hourly()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Now.AddHours(60);
                return (DateTime.Now - lastExecTime).TotalMinutes >= 60;
            });
            return this;
        }
        /// <summary>
        ///  	每一个小时的第 17 分钟运行一次
        /// </summary>
        public RulePipePayloadParser HourlyAt(int value)
        {
            _pipe.Add(lastExecTime =>
            {
                var timeSpan = value + 60;
                _currentTime = DateTime.Parse($"{DateTime.Now.AddHours(1).ToString("yyyy-MM-dd mm:00:00")}").AddMinutes(value);
                return (DateTime.Now - lastExecTime).TotalMinutes >= timeSpan;
            });
            return this;
        }
        /// <summary>
        /// 每到午夜执行一次任务
        /// </summary>
        public RulePipePayloadParser Daily()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd 23:59:59")}").AddSeconds(1);
                return (DateTime.Now - DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd 23:59:59")}")).TotalSeconds >= 1;
            });
            return this;
        }
        /// <summary>
        /// 每天的 13:00 执行一次任务
        /// </summary>
        public RulePipePayloadParser DailyAt(string value)
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {value}");
                return (DateTime.Now - DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {value}")).TotalSeconds >= 0;
            });
            return this;
        }

        /// <summary>
        /// 每天的 1:00 和 13:00 分别执行一次任务
        /// </summary>
        public RulePipePayloadParser TwiceDaily(string value1, string value2)
        {
            _pipe.Add(lastExecTime =>
            {

                if ((DateTime.Now - DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {value1}")).TotalSeconds >= 0)
                {
                    _currentTime = DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {value1}");
                    return true;
                }
                else if ((DateTime.Now - DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {value2}")).TotalSeconds <= 0)
                {
                    _currentTime = DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {value2}");
                    return true;
                }
                return false;

            });
            return this;
        }
        /// <summary>
        /// 每周执行一次任务
        /// </summary>
        public RulePipePayloadParser Weekly()
        {
            _pipe.Add(lastExecTime =>
            {
                var timeSpan = Convert.ToDouble((6 - Convert.ToInt16(DateTime.Now.DayOfWeek)));
                _currentTime = DateTime.Parse($"{DateTime.Now.AddDays(timeSpan).ToString("yyyy-MM-dd")}");
                return DateTime.Now >= _currentTime;
            });
            return this;
        }
        /// <summary>
        /// 每月执行一次任务
        /// </summary>
        public RulePipePayloadParser Monthly()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1);
                return DateTime.Now >= _currentTime;
            });
            return this;
        }
        /// <summary>
        /// 在每个月的第四天的 15:00 执行一次任务
        /// </summary> 
        public RulePipePayloadParser MonthlyOn(int value1, string value2)
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Parse(DateTime.Now.ToString($"yyyy-MM-0{value1} {value2}")).AddMonths(1);
                return DateTime.Now >= _currentTime;
            });
            return this;
        }
        /// <summary>
        /// 每季度执行一次任务
        /// </summary>
        public RulePipePayloadParser Quarterly()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Parse(DateTime.Now.AddMonths(3 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01"));
                return DateTime.Now >= _currentTime;
            });
            return this;
        }
        /// <summary>
        /// 每年执行一次任务
        /// </summary>
        public RulePipePayloadParser Yearly()
        {
            _pipe.Add(lastExecTime =>
            {
                _currentTime = DateTime.Parse(DateTime.Now.ToString("yyyy-01-01")).AddYears(1);
                return DateTime.Now >= _currentTime;
            });
            return this;
        }

        public RulePipePayloadParser Weekdays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week >= 1 && week <= 5;
            });
            return this;
        }

        public RulePipePayloadParser Sundays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 0;
            });
            return this;
        }

        public RulePipePayloadParser Mondays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 1;
            });
            return this;
        }

        public RulePipePayloadParser Tuesdays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 2;
            });
            return this;
        }

        public RulePipePayloadParser Wednesdays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 3;
            });
            return this;
        }

        public RulePipePayloadParser Thursdays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 4;
            });
            return this;
        }

        public RulePipePayloadParser Fridays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 5;
            });
            return this;
        }

        public RulePipePayloadParser Saturdays()
        {
            _pipe.Add(lastExecTime =>
            {
                var week = (int)ToLocalTimeZone(_currentTime).DayOfWeek;
                return week == 6;
            });
            return this;
        }

        public RulePipePayloadParser Between(string value1, string value2)
        {

            _pipe.Add(lastExecTime =>
            {
                TimeSpan currentDt = ToLocalTimeZone(_currentTime).TimeOfDay;
                TimeSpan workStartDT = ToLocalTimeZone(DateTime.Parse(value1)).TimeOfDay;
                TimeSpan workEndDT = ToLocalTimeZone(DateTime.Parse(value2)).TimeOfDay;
                return currentDt >= workStartDT && currentDt <= workEndDT;
            });
            return this;
        }

        public RulePipePayloadParser UnlessBetweenBetween(string value1, string value2)
        {
            _pipe.Add(lastExecTime =>
            {
                TimeSpan currentDt = ToLocalTimeZone(_currentTime).TimeOfDay;
                TimeSpan workStartDT = ToLocalTimeZone(DateTime.Parse(value1)).TimeOfDay;
                TimeSpan workEndDT = ToLocalTimeZone(DateTime.Parse(value2)).TimeOfDay;
                return currentDt < workStartDT || currentDt > workEndDT;
            });
            return this;
        }

        public void Build(DateTime dateTime)
        {
            if (_pipe.Any())
            {
                var pipeResult = true;
                foreach (var pipe in _pipe)
                {
                    try
                    {
                        pipeResult = pipe.Invoke(dateTime);
                        if (!pipeResult) break;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                _subject.OnNext(pipeResult);
            }
        }

        public ISubject<bool> HandlePayload()
        {
            return _subject;
        }

        public RulePipePayloadParser When(string script)
        {
            var propertyName = "script";
            script = $"var {propertyName} ={script}";
            var engine = new Engine()
       .SetValue("DateUtils", new DateUtils()).Execute(script);
            _pipe.Add(lastExecTime =>
            {
                try
                {
                    var result = engine.Invoke(propertyName, ToLocalTimeZone(lastExecTime));
                    return result.AsBoolean();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
            return this;
        }

        public RulePipePayloadParser TimeZone(string text)
        {
            _zoneEnum = Enum.Parse<LocalTimeZoneEnum>(text, true);
            return this;
        }

        private DateTime ToLocalTimeZone(DateTime time)
        {
            DateTime easternTime = time;
            if (_zoneEnum == LocalTimeZoneEnum.Utc)
            {
                easternTime = easternTime.ToUniversalTime();
            }
            else if (_zoneEnum != LocalTimeZoneEnum.Local)
            {
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(LocalTimeZoneEnumLong(_zoneEnum));
                easternTime = TimeZoneInfo.ConvertTimeFromUtc(time, easternZone);
            }
            return easternTime;
        }

        private string LocalTimeZoneEnumLong(LocalTimeZoneEnum time) => time switch
        {
            LocalTimeZoneEnum.China => "China Standard Time",
            LocalTimeZoneEnum.Italy => "W. Europe Standard Time",
            LocalTimeZoneEnum.US => "Pacific Standard Time",
            LocalTimeZoneEnum.GB => "GMT Standard Time",
            LocalTimeZoneEnum.DE => "W. Europe Standard Time",
            LocalTimeZoneEnum.FR => "Romance Standard Time",
            LocalTimeZoneEnum.JP => "Tokyo Standard Time",
            LocalTimeZoneEnum.ES => "Romance Standard Time",
            LocalTimeZoneEnum.CA => "Pacific Standard Time",
            LocalTimeZoneEnum.MX => "Central Standard Time (Mexico)",
            LocalTimeZoneEnum.AU => "E. Australia Standard Time",
            LocalTimeZoneEnum.Utc => "utc",
            _ => ""
        };

        public RulePipePayloadParser Skip(string script)
        {
            var propertyName = "script";
            script = $"var {propertyName} ={script}";
            var engine = new Engine()
          .SetValue("DateUtils", new DateUtils()).Execute(script);
            _pipe.Add(lastExecTime =>
            {
                try
                {
                    var result = engine.Invoke(propertyName, ToLocalTimeZone(lastExecTime));
                    return !result.AsBoolean();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
            return this;
        }
    }
}
