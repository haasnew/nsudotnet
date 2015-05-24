using System;

namespace TaskScheduler
{
    class Program
    {
        static void Main()
        {
            var ts = new TaskScheduler();
            Console.WriteLine(DateTime.Now);

            try
            {
                ts.SchedulePeriodicJob(() => Console.WriteLine("{0}, Выполнение началось!", DateTime.Now), "* * * * *");
                //				ts.SchedulePeriodicJob(() => Console.WriteLine("{0}, execution started!", DateTime.Now), TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
