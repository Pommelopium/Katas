
namespace RussischeBauernmultiplikation;

/// <summary>
///     Function Kata: Russische Bauernmultiplikation
///     Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Quelle: https://ccd-school.de/coding-dojo/function-katas/russische-bauernmultiplikation/
/// </summary>
public class Program
{
    public static int Mul(int a, int b)
    {
        if (a == 0 || b == 0)
        {
            return 0;
        }
        
        List<(int, int)> result = [];
        do
        {
            result.Add((a, b));
            a /= 2;
            b *= 2;
        } while (a >= 1);

        return result.Where(t => t.Item1 % 2 != 0).Select(t => t.Item2).Sum();
    }
}
