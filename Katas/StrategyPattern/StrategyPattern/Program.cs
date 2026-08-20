
namespace StrategyPattern;

/// <summary>
///     Kata 14_20 — Strategie (Strategy) (Verhaltensmuster aus dem Refactoring.Guru-Katalog)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangszustand: CalculateShippingCost mit switch ueber den Verfahrensnamen
    // [ ] IShippingCostStrategy als Strategie-Interface
    // [ ] drei Implementierungen: Standard, Express, Abholung
    // [ ] Auswahl per DI: Schluessel aus der Konfiguration -> Strategie
    // [ ] vierte Strategie (Spedition) ohne Aenderung bestehender Klassen einhaengen
    // [ ] unbekannter Schluessel -> definierter Fehler, kein stiller Standard
    // [ ] Variante mit Func<ShippingRequest, decimal> — Abwaegung schriftlich festhalten
    // [ ] Kontext-Test mit Fake-Strategie, ohne Fachrechnung
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_20");
    }
}
