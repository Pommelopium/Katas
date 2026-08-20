
namespace RateLimitedFetcher;

/// <summary>
///     Kata 08_01 — Rate-limited Fetcher mit Retry (Stufe 2: Async und Nebenlaeufigkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] max. N parallele Requests (SemaphoreSlim oder Parallel.ForEachAsync)
    // [ ] Retry mit exponentiellem Backoff + Jitter — erst selbst, dann mit Polly
    // [ ] CancellationToken bis in jeden Aufruf durchgereicht
    // [ ] IHttpClientFactory statt new HttpClient()
    // [ ] IAsyncEnumerable<Result<Document>>, liefert sobald fertig
    // [ ] Tests ohne Thread.Sleep — FakeTimeProvider + gemockter HttpMessageHandler
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 08_01");
        await Task.CompletedTask;
    }
}
