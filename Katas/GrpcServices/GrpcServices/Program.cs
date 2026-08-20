
namespace GrpcServices;

/// <summary>
///     Kata 09_05 — gRPC fuer Dienst-zu-Dienst-Kommunikation
///     (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] .proto contract-first, Server und Client daraus generiert
    // [ ] Unary, Server-, Client- und Bidirectional Streaming je mit passendem Beispiel
    // [ ] Schema-Evolution mit alten und neuen Gegenstellen durchgespielt, Regeln notiert
    // [ ] StatusCode-Mapping aus Result<T>, Deadline je Aufruf und deren Vererbung
    // [ ] Interceptors server- und clientseitig
    // [ ] Geteilter Kanal vs. Kanal pro Aufruf an der Verbindungszahl gezeigt
    // [ ] Messvergleich zu REST: Nutzlast, Latenz, Allokationen — inkl. Nachteilen
    // [ ] Health Checks und Reflection bewusst konfiguriert
    // [ ] Optional: gRPC-Web oder JSON-Transcoding, Verlust benannt
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 09_05");
        await Task.CompletedTask;
    }
}
