
namespace ResultPattern;

/// <summary>
///     Kata 07_02 — Result-Pattern statt Exceptions (Stufe 1: Modernes C# und Testbarkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] readonly record struct Error(string Code, string Message)
    // [ ] Result<T> mit Success / Failure / Match / Bind
    // [ ] Result ohne Wert
    // [ ] Combine(params Result[])
    // [ ] RomanNumerals darauf portieren — kein throw mehr
    // [ ] Tests: jeder Fehlerfall hat einen eigenen Error.Code
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 07_02");
    }
}
