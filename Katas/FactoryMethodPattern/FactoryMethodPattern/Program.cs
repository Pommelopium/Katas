
namespace FactoryMethodPattern;

/// <summary>
///     Kata 14_01 — Fabrikmethode (Factory Method), Erzeugungsmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode mit switch-Kaskade und dreifachem Copy-Paste hinschreiben
    // [ ] gemeinsame Schnittstelle IReportWriter herausziehen
    // [ ] abstrakte Basisklasse ReportExporter mit Export() als festem Ablauf
    // [ ] abstract IReportWriter CreateWriter() — die Fabrikmethode
    // [ ] konkrete Creator: MarkdownReportExporter, CsvReportExporter
    // [ ] JsonReportExporter ergaenzen, ohne bestehende Klassen zu aendern
    // [ ] Tests: Ablauf einmal getestet, Formate je einzeln, Erweiterung als Beweis
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_01");
    }
}
