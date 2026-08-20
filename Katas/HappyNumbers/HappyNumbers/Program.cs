
using System;
using System.Linq;

namespace HappyNumbers;

public class Program
{
    public static void Main()
    {
        for (int i = 10; i <= 200; i++)
        {
            Console.WriteLine($"{i} is Happy: {IsHappyNumber(i)}");
        }
    }
    
    private static bool IsHappyNumber(int number, int iteration = 0)
    {
        int[] digits = number.ToString().Select(c => int.Parse(c.ToString())).ToArray();
        int sum = 0;
        for (int i = 0; i < digits.Length; i++)
        {
            sum += digits[i]*digits[i];
        }

        if (sum == 1)
        {
            return true;
        }
        
        if(iteration < 100) //max num of iterations
        {
            return IsHappyNumber(sum, iteration+1);
        }
        return false;
    }
}