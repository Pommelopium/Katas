using System;

namespace  FizzBuss;

class Program
{
    static void Main(string[] args)
    {
        FizzBuzz(true);
    }

    private static void FizzBuzz(bool funMode = false)
    {
        for (int i = 1; i <= 100; i++)
        {
            string output = string.Empty;
            if (i % 3 == 0 || (funMode && i.ToString().Contains('3')))
            {
                output += "Fizz";
            }
            if (i % 5 == 0 || (funMode && i.ToString().Contains('5')))
            {
                output += "Buzz";
            }
            
            Console.WriteLine(string.IsNullOrEmpty(output) ? i.ToString() : output);
        }
    }
}
