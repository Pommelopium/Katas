
namespace ResilienceAndChaos;

/// <summary>
///     Kata 11_09 — Resilienz und Chaos (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Retry Storm gemessen: Requests am Ziel ohne Begrenzung
    // [ ] Timeout je Versuch und gesamt, Verhaeltnis begruendet
    // [ ] Circuit Breaker: alle drei Zustandswechsel im Log
    // [ ] Bulkhead: haengende Abhaengigkeit blockiert die zweite nicht
    // [ ] Fallback pro Aufruf definiert; nicht degradierbare Faelle benannt
    // [ ] Resilience Pipeline in bewusster Reihenfolge, Vertauschung gezeigt
    // [ ] Jitter: Verteilung der Retry-Zeitpunkte mit und ohne gemessen
    // [ ] Chaos-Injektion als Test je Muster
    // [ ] Jede Operation als idempotent oder nicht markiert
    // [ ] Metriken: offene Kreise, abgelehnte Bulkhead-Anfragen, Retries pro Aufruf
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_09");
        await Task.CompletedTask;
    }
}
