
namespace TestableLinesOfCode;

/// <summary>
///     Kata 07_01 — LinesOfCode testbar machen (Stufe 1: Modernes C# und Testbarkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] CodeStatistics als record statt (int, int, int)
    // [ ] Analyse arbeitet auf TextReader/string, nie auf einem Pfad
    // [ ] ISourceReader fuer den Dateizugriff
    // [ ] xUnit-Projekt, Tests ZUERST
    // [ ] Bug: Leerzeile wird doppelt gezaehlt
    // [ ] Bug: // im String-Literal
    // [ ] Bug: /* ... */ in einer Zeile
    // [ ] Bug: Kommentar am Zeilenende
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 07_01");
    }
}
