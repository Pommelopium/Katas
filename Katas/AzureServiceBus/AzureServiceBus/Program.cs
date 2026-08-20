
namespace AzureServiceBus;

/// <summary>
///     Kata 11_05 — Azure Service Bus (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Service Bus Emulator im Container, Queue + Topic mit zwei Subscriptions
    // [ ] ServiceBusProcessor als BackgroundService, MaxConcurrentCalls begruendet
    // [ ] PeekLock, Lock-Renewal und Crash-Verhalten nachgewiesen
    // [ ] Poison Message in der DLQ, Handler zum Zurueckspielen
    // [ ] Sessions fuer strenge Reihenfolge pro KataId
    // [ ] Scheduled Message einplanen und stornieren
    // [ ] Duplicate Detection mit MessageId aus der Outbox
    // [ ] Drosselung (ServiceBusy) mit Backoff behandelt
    // [ ] Vergleichstabelle RabbitMQ vs. Kafka vs. Service Bus
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_05");
        await Task.CompletedTask;
    }
}
