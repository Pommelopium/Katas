
namespace KafkaStreaming;

/// <summary>
///     Kata 11_04 — Event-Streaming mit Kafka (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Kafka im KRaft-Modus per Compose, Topic attempts mit 3 Partitionen
    // [ ] Producer mit Key = KataId, Acks.All, EnableIdempotence
    // [ ] Consumer als BackgroundService, EnableAutoCommit = false
    // [ ] Rebalancing mit 2, 3 und 4 Instanzen nachgewiesen
    // [ ] Poison Message landet in attempts.dlq, Consumer laeuft weiter
    // [ ] Consumer Lag als Metrik exportiert
    // [ ] Offset-Reset erzeugt identische Statistik (Replay)
    // [ ] Halbe Seite: Kafka vs. RabbitMQ
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_04");
        await Task.CompletedTask;
    }
}
