using System;

namespace JsonSerializer
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var testObj = new Test();

            Console.WriteLine(Serializer.Serialize(testObj));

            Console.WriteLine(typeof(int).IsPrimitive);
        }
    }
}