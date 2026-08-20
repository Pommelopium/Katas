
namespace BulkAndPaging;

/// <summary>
///     Kata 10_04 — Massendaten: Bulk-Import und Keyset-Pagination
///     (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Baseline gemessen: 100.000 Zeilen mit SaveChangesAsync pro Entitaet
    // [ ] Jede Optimierungsstufe einzeln gemessen bis SqlBulkCopy
    // [ ] SqlBulkCopy streamend ueber IDataReader, Speicher bleibt konstant
    // [ ] Table-Valued Parameter gegen Einzelaufrufe und IN (...) verglichen
    // [ ] Upsert per MERGE ueber Staging, Race Conditions benannt
    // [ ] Import nach Abbruch ohne Duplikate fortsetzbar
    // [ ] OFFSET-Pagination: Wachstum der Laufzeit im Plan gezeigt
    // [ ] Keyset-Pagination mit stabilem Tiebreaker, Seite 1 == Seite 5000
    // [ ] Opaker Cursor nach aussen, Instabilitaet von Offset erklaert
    // [ ] Entscheidung zur Gesamtzahl begruendet
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 10_04");
        await Task.CompletedTask;
    }
}
