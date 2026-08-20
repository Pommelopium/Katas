
namespace AbstractFactoryPattern;

/// <summary>
///     Kata 14_02 — Abstrakte Fabrik (Abstract Factory), Erzeugungsmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode mit drei if/switch-Kaskaden pro Land hinschreiben und den Schmerz sehen
    // [ ] Produkt-Schnittstellen trennen: ITaxRule, IAmountFormatter, IInvoiceLayout
    // [ ] IInvoiceKitFactory mit je einer Create-Methode pro Produkt
    // [ ] konkrete Fabriken: GermanInvoiceKitFactory, FrenchInvoiceKitFactory
    // [ ] Aufrufer nimmt nur noch die Fabrik, kein Landeskennzeichen mehr im Ablauf
    // [ ] SwissInvoiceKitFactory ergaenzen, ohne bestehende Klassen zu aendern
    // [ ] Tests: Familien einzeln gruen, Mischung nicht mehr konstruierbar, Erweiterung als Beweis
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_02");
    }
}
