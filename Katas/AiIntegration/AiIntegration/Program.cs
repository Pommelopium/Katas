
namespace AiIntegration;

/// <summary>
///     Kata 13_02 — AI-Integration (Stufe 5: Differenzierung)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Chat-Endpoint ueber Semantic Kernel oder LLM-SDK
    // [ ] Tool Calling auf echte Domaenen-Services (GetAttempts, GetStreak, SuggestNextKata)
    // [ ] Streaming per Server-Sent Events ins Blazor-Frontend
    // [ ] Embeddings + Vektorsuche ueber die Kata-Beschreibungen
    // [ ] Token- und Kostenmetriken ueber die Meter aus Kata 11_02
    // [ ] LLM hinter einem Interface — kein Test darf echtes Geld kosten
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 13_02");
        await Task.CompletedTask;
    }
}
