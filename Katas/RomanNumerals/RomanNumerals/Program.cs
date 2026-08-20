using System;
using System.Collections.Generic;
using System.Linq;

namespace RomanNumerals;

class Program
{
    static void Main(string[] _)
    {
        //im Coding Kata ist eigentlich geschrieben, dass die Zahlen korrekt sind, trotzdem mal eine Prüfung mit einbauen
        Console.WriteLine("RomanNumerals: ");
        Console.WriteLine($"I :{Parse("I")}");
        Console.WriteLine($"II :{Parse("II")}");
        Console.WriteLine($"IV :{Parse("IV")}");
        Console.WriteLine($"V :{Parse("V")}");
        Console.WriteLine($"IX :{Parse("IX")}");
        Console.WriteLine($"XLII :{Parse("XLII")}");
        Console.WriteLine($"XCIX :{Parse("XCIX")}");
        Console.WriteLine($"MMXIII :{Parse("MMXIII")}");
        Console.WriteLine($"MMIXIII :{Parse("MMIXIII")}");
        Console.WriteLine($"MMIIXIII :{Parse("MMIIXIII")}");
        Console.WriteLine($"IM :{Parse("IM")}");
    }
    
    private static readonly Dictionary<char, int> RomanNumerals = new ()
    {
        { 'I', 1 },
        { 'V', 5 },
        { 'X', 10 },
        { 'L', 50 },
        { 'C', 100 },
        { 'D', 500 },
        { 'M', 1000 },
    };

    private static int Parse(string romanValue)
    {
        if(romanValue.Length == 0)
            return 0;
        
        // Gatekeep, wenn eine Zahl nicht römisch ist, return -1
        if (!ContainsOnlyDigits(romanValue))
            return -1;
        
        //GateKeep Substraktionsregel
        if (!CheckSubstractionRule(romanValue))
            return -1;

        if(romanValue.Length == 1)
            return RomanNumerals[romanValue[0]];
        
        int result = 0;

        for (int i = romanValue.Length-1; i >= 0 ; i--)
        {
            int value = RomanNumerals[romanValue[i]];
            bool substract = false;
            if (i != romanValue.Length-1)
            {
                int prevValue = RomanNumerals[romanValue[i+1]];
                if (value < prevValue)
                {
                    substract = true;
                }
            }
            
            if(substract)
                result  -= value;
            else
                result  += value;
        }
        
        return result;
    }
    
    private static bool ContainsOnlyDigits(string romanValue)
    {
        if (romanValue.Any(c => RomanNumerals.ContainsKey(c)) == false)
        {
            return false;
        }
        return true;
    }
    
    /*
     * Die Subtraktionsregel in ihrer Normalform besagt, dass die Zahlzeichen I, X und C einem ihrer beiden jeweils
     * nächstgrößeren Zahlzeichen vorangestellt werden dürfen und dann in ihrem Zahlwert von dessen Wert abzuziehen sind:
        I vor V oder X: IV (4), IX (9)
        X vor L oder C: XL (40), XC (90)
        C vor D oder M: CD (400), CM (900)
     */
    private static bool CheckSubstractionRule(string romanValue)
    {
        if(romanValue.Length <= 1)
            return true;
        
        // Geht die römische Zahl rückwärts durch und prüft, ob die nächste Zahl kleiner ist. Wenn ja, muss die darauf
        // folgende Zahl wieder größer sein, sonst liegt ein Fehler vor.
        for (int i = romanValue.Length-1; i >= 0 ; i--)
        {
            int value = RomanNumerals[romanValue[i]];
            if (i - 1 >= 0)
            {
                int nextValue = RomanNumerals[romanValue[i - 1]];
                if (nextValue < value)
                {
                    if (!IsAllowedSubstraction(romanValue[i - 1]))
                    {
                        return false;
                    }
                    if (i - 2 >= 0)
                    {
                        int secondNextValue = RomanNumerals[romanValue[i - 2]];
                        if (secondNextValue < value)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private static bool IsAllowedSubstraction(char c)
    {
        //Nur I X C erlaubt
        if (c == 'I')
        {
            return true;
        }

        if (c == 'X')
        {
            return true;
        }

        if (c == 'C')
        {
            return true;
        }

        return false;
    }
}