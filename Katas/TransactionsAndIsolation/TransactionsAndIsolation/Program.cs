
namespace TransactionsAndIsolation;

/// <summary>
///     Kata 10_03 — Transaktionen, Isolationslevel, Deadlocks (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Dirty Read erzeugt und erklaert, warum NOLOCK kein Performance-Trick ist
    // [ ] Lost Update dreifach geloest (SET x = x + 1, rowversion, UPDLOCK)
    // [ ] Non-repeatable Read und Phantom Read je reproduziert
    // [ ] Write Skew unter SNAPSHOT gezeigt
    // [ ] READ_COMMITTED_SNAPSHOT samt tempdb-Kosten bewertet
    // [ ] Deadlock provoziert, Graph gelesen, per Sperrreihenfolge behoben
    // [ ] Retry-Policy fuer 1205/1222 mit Backoff und Jitter um idempotente Operationen
    // [ ] TransactionScope mit async: Verhalten ohne AsyncFlowOption gezeigt
    // [ ] LOCK_TIMEOUT und Transaktions-Timeouts bewusst gesetzt
    // [ ] Pro Anomalie ein Test: falsches Level rot, richtiges Level gruen
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 10_03");
        await Task.CompletedTask;
    }
}
