using System;
using System.Collections.Generic;

namespace NumberGuesser
{
    class Program
    {
        static void Main()
        {
            Console.Write("Enter your name, please: ");
            var userName = Console.ReadLine();
            Console.WriteLine("Hi, {0}!", userName);
            var random = new Random();
            var randomNumber = random.Next(0, 100);
            Console.WriteLine("Can you guess what number I put forth?");
            var startTime = DateTime.Now;
            var tryCount = 0;
            var attempts = new List<KeyValuePair<string, CheckResult>>();

            var affrontions = new List<string>
                            {
                                    "stupid {0}",
                                    "jackass{0}",
                                    "fool {0}",
                                    "jester {0}",
                            };

            while (true)
            {
                Console.Write("[{0}] : ", ++tryCount);
                var answer = Console.ReadLine();
                var result = Check(randomNumber, answer);
                attempts.Add(new KeyValuePair<string, CheckResult>(answer, result));
                Console.WriteLine("{0}!", result);
                if (result == CheckResult.Quit)
                {
                    Console.WriteLine("\tSorry, I must exit now..");
                    return;
                }
                if (result == CheckResult.Equal)
                {
                    for (var i = 0; i < attempts.Count; i++)
                    {
                        var attempt = attempts[i];
                        Console.WriteLine("\t[{0}], answer: [{1}], result: [{2}]", i + 1, attempt.Key, attempt.Value);
                    }
                    Console.WriteLine("You have finished in {0} seconds!", (DateTime.Now - startTime).TotalSeconds);
                    return;
                }
                if (tryCount % 4 == 0)
                {
                    Console.WriteLine(affrontions[random.Next(0, affrontions.Count)], userName);
                }
            }
        }

        private enum CheckResult
        {
            Less,
            Equal,
            Greater,
            NotValid,
            Quit
        }

        private static CheckResult Check(int desired, string answer)
        {
            if (answer == "q")
            {
                return CheckResult.Quit;
            }
            int intAnswer;
            if (!Int32.TryParse(answer, out intAnswer))
            {
                return CheckResult.NotValid;
            }
            if (desired > intAnswer)
            {
                return CheckResult.Greater;
            }
            if (desired == intAnswer)
            {
                return CheckResult.Equal;
            }
            if (desired < intAnswer)
            {
                return CheckResult.Less;
            }
            return CheckResult.NotValid;
        }
    }
}