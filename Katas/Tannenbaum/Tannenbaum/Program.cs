
namespace Tannenbaum;

/// <summary>
///     Function Kata: Tannenbaum
///     Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Quelle: https://ccd-school.de/coding-dojo/function-katas/tannenbaum/
/// </summary>
public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(string.Join("\n\r", Tannenbaum(5)));
    }

    public static string[] Tannenbaum(int length)
    {
        string[] result = new string[length+1];

        for (int i = 1; i <= length; i++)
        {
            string line = string.Empty;
            for (int j = 1; j <= (i+i-1); j++)
            {
                //Fand die Idee witzig, dass Random Christbaumkugeln auftauchen
                int randomInt = Random.Shared.Next(1, 100);
                if(randomInt > 95 || i==1)
                    line += "*";
                else
                    line += "X";
            }
            result[i-1] = line;
        }
        
        result[length] = "I";
        
        //Length -1 da der Stamm ja eine andere Einrückung erhält.
        for (int i = 0; i < result.Length-1; i++)
        {
            result[i] = result[i].PadLeft(length+i, '\u00A0');
        }
        result[length] = result[length].PadLeft(length, '\u00A0');
        
        return result;
    }
}
