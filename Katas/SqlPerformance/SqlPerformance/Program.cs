
namespace SqlPerformance;

/// <summary>
///     Kata 10_02 — Ausfuehrungsplaene und Indizes (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Testdatenbank mit mindestens 2 Mio. Attempt-Zeilen
    // [ ] Ausgangsmessung per SET STATISTICS IO, TIME — Logical Reads notiert
    // [ ] Pro Plan der teuerste Operator benannt
    // [ ] Scan -> Seek durch nonclustered Index, Key Lookup durch INCLUDE beseitigt
    // [ ] (A, B) vs. (B, A): Query gezeigt, die nur einen nutzen kann
    // [ ] Drei nicht-SARGable Queries umformuliert
    // [ ] Parameter Sniffing reproduziert und begruendet geloest
    // [ ] Filtered Index und Index auf persistierte berechnete Spalte
    // [ ] Schreibkosten der Indizes vorher/nachher gemessen
    // [ ] Top-5-Queries ueber DMVs / Query Store gefunden
    // [ ] Tabelle: Query | Reads vorher | nachher | Aenderung | Begruendung
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 10_02");
        await Task.CompletedTask;
    }
}
