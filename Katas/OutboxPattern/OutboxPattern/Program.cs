
namespace OutboxPattern;

/// <summary>
///     Kata 11_01 — Transactional Outbox (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] OutboxMessage-Tabelle, geschrieben in DERSELBEN Transaktion wie die Fachdaten
    // [ ] BackgroundService pollt, publiziert nach RabbitMQ, markiert als verarbeitet
    // [ ] Consumer idempotent (Dedup ueber MessageId)
    // [ ] Nachweis: Kill zwischen Commit und Publish -> Nachricht kommt trotzdem an
    // [ ] Nachweis: doppelt publiziert -> Consumer wirkt nur einmal
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_01");
        await Task.CompletedTask;
    }
}
