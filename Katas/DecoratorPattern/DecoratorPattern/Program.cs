
namespace DecoratorPattern;

/// <summary>
///     Kata 14_09 — Dekorator (Decorator), Strukturmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode mit Flags fuer Logging, Caching und Retry hinschreiben und den Schmerz sehen
    // [ ] gemeinsame Schnittstelle IStreakSource herausziehen, Kern ohne Belange (StreakCalculator)
    // [ ] ein Dekorator pro Belang: Logging, Caching, Retry — je eine Klasse, je ein Grund zur Aenderung
    // [ ] Stapel in definierter Reihenfolge zusammensetzen, Reihenfolge als Test festnageln
    // [ ] Registrierung im DI-Container, sodass der Aufrufer nur IStreakSource kennt
    // [ ] neuen Belang (Timing/Metrik) ergaenzen, ohne bestehende Klassen anzufassen
    // [ ] Tests: Kern nackt gruen, Cache-vor-Retry gegen Retry-vor-Cache messbar unterschiedlich
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_09");
    }
}
