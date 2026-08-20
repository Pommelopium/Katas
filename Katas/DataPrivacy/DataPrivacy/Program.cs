
namespace DataPrivacy;

/// <summary>
///     Kata 12_02 — Personenbezogene Daten und Loeschbarkeit
///     (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Inventar aller Fundorte: Tabellen, Logs, Traces, Metriken, Messages, Cache, Backups
    // [ ] [PersonalData]-Kennzeichnung plus Test, der das Inventar generiert
    // [ ] Loeschen und Anonymisieren beide gebaut, Wahl je Datensatz begruendet
    // [ ] Loeschauftrag laeuft durch alle Fundorte, nicht nur ueber die Datenbank
    // [ ] Log-Scrubbing mit Test: Exception-Payload erscheint nicht im Log
    // [ ] Auskunftsersuchen als maschinenlesbarer Export
    // [ ] Aufbewahrungsfristen als Job mit Loeschprotokoll
    // [ ] Ein Feld verschluesselt abgelegt, Suchbarkeit und Kosten bewertet
    // [ ] Pseudonymisierter Testdatensatz mit erhaltenen Verteilungen
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 12_02");
        await Task.CompletedTask;
    }
}
