using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TaskScheduler
{
    public class TaskScheduler
    {
        private readonly IList<Tuple<DateTime, Action>> _scheduledJobs;

        public TaskScheduler()
        {
            _scheduledJobs = new List<Tuple<DateTime, Action>>();
            new Thread(() =>
            {
                while (true)
                {
                    var now = DateTime.Now;
                    var jobsToExecute = _scheduledJobs.Where(sj => sj.Item1 < now).ToList();
                    foreach (var job in jobsToExecute)
                    {
                        _scheduledJobs.Remove(job);
                        var jobAction = job.Item2;
                        new Thread(() => jobAction()).Start();
                    }

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public void ScheduleJob(Action job, DateTime dateTime)
        {
            _scheduledJobs.Add(new Tuple<DateTime, Action>(dateTime, job));
        }

        public void ScheduleDelayedJob(Action job, TimeSpan delay)
        {
            ScheduleJob(job, DateTime.Now + delay);
        }

        private void R(Action job, IEnumerator<DateTime> dateTimes)
        {
            dateTimes.MoveNext();
            var currentDateTime = dateTimes.Current;
            ScheduleJob(() => R(job, dateTimes), currentDateTime);
            job();
        }

        public void SchedulePeriodicJob(Action job, IEnumerable<DateTime> times)
        {
            ScheduleDelayedJob(() => R(job, times.GetEnumerator()), TimeSpan.FromTicks(0));
        }

        public void SchedulePeriodicJob(Action job, TimeSpan period)
        {
            var now = DateTime.Now;
            var dateTimes =
                period.Repeat().Select((p, i) => Enumerable.Repeat(p, i).Aggregate(now, (acc, curr) => acc + curr));
            SchedulePeriodicJob(job, dateTimes);
        }

        public void SchedulePeriodicJob(Action job, string cronExpression)
        {
            var cronExpressionFormatException = new Lazy<ArgumentException>(() => new ArgumentException("invalid CronExpression format!", "cronExpression"));

            var cronExprParts = cronExpression.Split(' ').ToList();

            if (cronExprParts.Count != 5)
            {
                throw cronExpressionFormatException.Value;
            }
            var prefs = cronExprParts.Select(x =>
            {
                if (x == "*")
                {
                    return null;
                }
                int a;
                if (!Int32.TryParse(x, out a))
                {
                    throw cronExpressionFormatException.Value;
                }
                return (int?)a;
            }).ToList();

            var min = prefs[0];
            var hour = prefs[1];
            var dayOfMonth = prefs[2];
            var month = prefs[3];
            var dayOfWeek = prefs[4];

            if (min < 0 || min > 59)
            {
                throw cronExpressionFormatException.Value;
            }
            if (hour < 0 || hour > 23)
            {
                throw cronExpressionFormatException.Value;
            }
            if (dayOfMonth < 1 || dayOfMonth > 31)
            {
                throw cronExpressionFormatException.Value;
            }
            if (month < 1 || month > 12)
            {
                throw cronExpressionFormatException.Value;
            }
            if (dayOfWeek < 0 || dayOfWeek > 7)
            {
                throw cronExpressionFormatException.Value;
            }

            dayOfWeek = dayOfWeek % 7;

            var dateTimes = DateTimeSequence(min, hour, dayOfMonth, month, dayOfWeek);

            SchedulePeriodicJob(job, dateTimes);
        }
        public static IEnumerable<DateTime> DateTimeSequence(int? min, int? hour, int? dayOfMonth, int? month, int? dayOfWeek)
        {
            var last = DateTime.Now;
            while (true)
            {
                var curr = last;

                while (true)
                {
                    if (min.HasValue)
                    {
                        if (curr.Minute > min)
                        {
                            curr += TimeSpan.FromMinutes(60 - curr.Minute);
                            continue;
                        }
                        curr += TimeSpan.FromMinutes(min.Value - curr.Minute);
                    }
                    if (hour.HasValue)
                    {
                        if (curr.Hour > hour)
                        {
                            curr += TimeSpan.FromHours(24 - curr.Hour);
                            continue;
                        }
                        curr += TimeSpan.FromHours(hour.Value - curr.Hour);
                    }
                    if (dayOfMonth.HasValue)
                    {
                        var daysInMonth = DateTime.DaysInMonth(curr.Year, curr.Month);

                        if (daysInMonth < dayOfMonth || curr.Day > dayOfMonth)
                        {
                            curr += TimeSpan.FromDays(daysInMonth + 1 - curr.Day);
                            continue;
                        }
                        curr += TimeSpan.FromDays(dayOfMonth.Value - curr.Day);
                    }
                    if (month.HasValue)
                    {
                        if (curr.Month > month)
                        {
                            curr = new DateTime(curr.Year + 1, 1, curr.Day, curr.Hour, curr.Minute, curr.Second);
                            continue;
                        }
                        curr = new DateTime(curr.Year, month.Value, curr.Day, curr.Hour, curr.Minute, curr.Second);
                    }

                    if (dayOfWeek.HasValue)
                    {
                        var currDayOfWeek = (int)curr.DayOfWeek;
                        if (currDayOfWeek != dayOfWeek)
                        {
                            var x = dayOfWeek.Value - currDayOfWeek;
                            x = x < 0 ? x + 7 : x;
                            curr += TimeSpan.FromDays(x);
                            continue;
                        }
                    }
                    break;
                }

                last = curr + TimeSpan.FromMinutes(1);
                yield return curr;
            }
        }
    }

    public static class Utils
    {
        public static IEnumerable<T> Repeat<T>(this T obj)
        {
            while (true)
            {
                yield return obj;
            }
        }
    }
}
