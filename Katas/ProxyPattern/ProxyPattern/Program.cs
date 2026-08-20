
namespace ProxyPattern;

/// <summary>
///     Kata 14_12 — Stellvertreter (Proxy), Strukturmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode abschreiben: Archiv wird eifrig geladen, Rechtepruefung steht beim Aufrufer
    // [ ] gemeinsame Schnittstelle IAttemptArchive fuer Original und Stellvertreter
    // [ ] Virtual Proxy: teures Archiv erst beim ersten echten Zugriff erzeugen, Ladezaehler beweist es
    // [ ] Protection Proxy: unberechtigter Zugriff wird abgewiesen, ohne das Ziel zu erzeugen
    // [ ] Logging- oder Caching-Proxy als dritte Art, Stapelreihenfolge bewusst festlegen
    // [ ] Aufrufer kennt nur das Interface — dieselbe Testsuite gegen Original und Proxy
    // [ ] Gegenprobe: Lazy<T>, gRPC-Client und EF-Core-Lazy-Loading-Proxies als eingebaute Loesungen
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_12");
    }
}
