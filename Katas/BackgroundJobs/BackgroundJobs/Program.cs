
namespace BackgroundJobs;

/// <summary>
///     Kata 11_07 — Hintergrundjobs und Zeitsteuerung (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] BackgroundService mit PeriodicTimer; stiller Tod und Terminverschiebung behoben
    // [ ] IHostedService-Lebenszyklus: StartAsync blockiert nicht mehr
    // [ ] Cron mit Zeitzone, Sommerzeitumstellung und Ausfallzeit-Strategie
    // [ ] Drei Instanzen, ein Joblauf — per Job-Store oder Lease nachgewiesen
    // [ ] Kill mitten im Lauf: keine Duplikate, keine Luecken
    // [ ] Graceful Shutdown ueber CancellationToken, ShutdownTimeout gesetzt
    // [ ] Retry mit Backoff, FailedJob-Tabelle mit Neustartweg
    // [ ] Metriken fuer Dauer, Erfolg und Verspaetung
    // [ ] Zeit ueber TimeProvider; ein Jahr Zeitplan im Test in Millisekunden
    // [ ] Vergleich BackgroundService vs. Hangfire vs. Quartz vs. externer Trigger
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_07");
        await Task.CompletedTask;
    }
}
