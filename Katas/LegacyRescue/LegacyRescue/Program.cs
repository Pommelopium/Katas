
namespace LegacyRescue;

/// <summary>
///     Kata 07_04 — Legacy-Code unter Test bringen (Stufe 1: Modernes C# und Testbarkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Charakterisierungstests bilden das heutige Verhalten ab, Bugs inklusive
    // [ ] Approval Tests mit sichtbarem Diff bei Verhaltenswechsel
    // [ ] Coverage als Landkarte genutzt, unerreichbare Zweige notiert
    // [ ] Seams eingezogen: Parameter, TimeProvider/Interface, Extract Method/Class
    // [ ] Bugfix aendert genau ein Approval-File
    // [ ] Strangler Fig mit Parallel Run: alt und neu liefern dasselbe
    // [ ] Aenderungsprotokoll gefuehrt und ausgewertet
    // [ ] Optional: Klasse mit System.Web/ConfigurationManager nach .NET 10 gebracht
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 07_04");
        await Task.CompletedTask;
    }
}
