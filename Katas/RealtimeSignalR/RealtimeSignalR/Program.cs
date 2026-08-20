
namespace RealtimeSignalR;

/// <summary>
///     Kata 09_06 — Echtzeit mit SignalR (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Hub<TClient> stark typisiert, keine Methodennamen als String
    // [ ] Ziele All/Group/User/Caller bewusst genutzt, Gruppenzuordnung nach Reconnect geklaert
    // [ ] Reconnect mit Backoff und Zustandsnachladen — keine Luecke nach Trennung
    // [ ] Autorisierung am Hub inkl. Pruefung der Gruppenmitgliedschaft
    // [ ] Zwei Instanzen: Nachricht fehlt, mit Redis-Backplane kommt sie an
    // [ ] Alle drei Transportarten erzwungen und verglichen
    // [ ] Client-zu-Hub-Aufruf mit Validierung und CancellationToken
    // [ ] 500 Verbindungen: Speicher pro Verbindung und Broadcast-Dauer gemessen
    // [ ] Hub-Logik ohne Netzwerk testbar, plus Integrationstest mit HubConnection
    // [ ] Abgrenzung SignalR vs. Polling vs. SSE vs. Broker
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 09_06");
        await Task.CompletedTask;
    }
}
