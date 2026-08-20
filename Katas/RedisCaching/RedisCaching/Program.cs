
namespace RedisCaching;

/// <summary>
///     Kata 11_06 — Verteilter Cache mit Redis (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Wirkung des Caches auf die teuerste Query aus Kata 10_02 beziffert
    // [ ] Cache-Aside mit begruendeten Ablaufzeiten
    // [ ] Cache Stampede provoziert und geloest, Zahlen vorher/nachher
    // [ ] Invalidierung beim Schreiben, inkl. Fehlerfall nach erfolgreichem Commit
    // [ ] Versioniertes Keyschema, Formatwechsel ohne Downtime beschrieben
    // [ ] HybridCache: L1-Problem gezeigt und ueber die Backplane geloest
    // [ ] Serialisierung gewaehlt, Kompatibilitaetstest fuer alte Eintraege
    // [ ] Rate Limiting, Idempotenz-Key-Store und verteiltes Lock je einmal
    // [ ] Cache-Hit-Rate als Metrik; Anwendung laeuft mit gestopptem Redis weiter
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_06");
        await Task.CompletedTask;
    }
}
