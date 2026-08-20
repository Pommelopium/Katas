
namespace ChannelPipeline;

/// <summary>
///     Kata 08_02 — Producer/Consumer-Pipeline mit Channels (Stufe 2: Async und Nebenlaeufigkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Reader (1) -> Channel<RawLine> -> Parser (N) -> Channel<Record> -> Writer (1)
    // [ ] BoundedChannelOptions mit Backpressure
    // [ ] Writer.Complete() mit Exception-Propagation
    // [ ] sauberes Shutdown via CancellationToken
    // [ ] IProgress<T>-Reporting
    // [ ] Fehler in einer Stufe darf keinen Task deadlocken
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 08_02");
        await Task.CompletedTask;
    }
}
