
namespace ArchitectureFitness;

/// <summary>
///     Kata 07_05 — Architektur- und Testqualitaet automatisch pruefen
///     (Stufe 1: Modernes C# und Testbarkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Schichtregeln als Tests: Domain referenzfrei, keine Zyklen, Sichtbarkeiten
    // [ ] Jede Regel einmal absichtlich verletzt und rot gesehen
    // [ ] Konventionen festgeschrieben statt im Review verhandelt
    // [ ] TreatWarningsAsErrors, dotnet format im Build, BannedApiAnalyzers
    // [ ] Stryker.NET: Mutation Score erhoben, Datei mit hoher Coverage / schlechtem Score
    // [ ] Drei ueberlebende Mutanten behoben, fehlende Testart benannt
    // [ ] Assertion-freier Test mit 100 % Coverage demonstriert
    // [ ] Coverage-Gate mit begruendeten Ausschluessen
    // [ ] Pipeline-Stufe, die tatsaechlich rot wird
    // [ ] Pro Regel ein Satz: was verhindert sie?
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 07_05");
        await Task.CompletedTask;
    }
}
