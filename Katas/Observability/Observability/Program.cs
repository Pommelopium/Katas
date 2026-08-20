
namespace Observability;

/// <summary>
///     Kata 11_02 — Observability (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Serilog, strukturiert, Correlation-ID ueber den ganzen Request
    // [ ] OpenTelemetry-Traces: API -> DB -> Broker -> Consumer, Export nach Jaeger
    // [ ] eigene Meter: Attempts/Minute, Outbox-Lag, Handler-Dauer als Histogram
    // [ ] /health/live und /health/ready getrennt
    // [ ] eingebauten Fehler im Trace finden, nicht im Debugger
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 11_02");
    }
}
