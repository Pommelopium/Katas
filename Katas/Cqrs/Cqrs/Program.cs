
namespace Cqrs;

/// <summary>
///     Kata 09_03 — CQRS ohne MediatR, dann mit (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Baut auf Kata 09_01 und 09_02 auf.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] ICommand<TResult>, IQuery<TResult> + Handler-Interfaces
    // [ ] eigener Dispatcher, Handler ueber DI mit offenen Generics aufgeloest
    // [ ] Pipeline: Logging -> Validation -> Transaction -> Handler
    // [ ] danach dasselbe mit MediatR, Vergleich schriftlich festhalten
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 09_03");
    }
}
