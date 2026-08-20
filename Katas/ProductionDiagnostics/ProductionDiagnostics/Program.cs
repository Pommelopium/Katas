
namespace ProductionDiagnostics;

/// <summary>
///     Kata 11_08 — Diagnose im laufenden Betrieb (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Speicherleck ueber zwei gcdump-Aufnahmen gefunden, Haltepfad benannt
    // [ ] Thread-Pool-Starvation an den Counters gezeigt und behoben
    // [ ] CPU-Hotspot per dotnet-trace als Flame Graph gefunden
    // [ ] Deadlock im Dump ueber clrstack und syncblk nachgewiesen
    // [ ] GC-Druck gemessen, Allokationen gesenkt, Differenz belegt
    // [ ] Dieselbe Analyse an einem Prozess im Container (dotnet-monitor)
    // [ ] Runbook: fuenf Symptome, je Kennzahl, Werkzeug und Verdacht
    // [ ] Instrumentierung nachgezogen, damit der Fehler kuenftig ohne Dump auffaellt
    // [ ] GCHeapHardLimitPercent und Container-Memory-Limit erklaert
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_08");
        await Task.CompletedTask;
    }
}
