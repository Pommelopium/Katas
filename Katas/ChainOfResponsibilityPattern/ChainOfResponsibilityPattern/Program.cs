
namespace ChainOfResponsibilityPattern;

/// <summary>
///     Kata 14_13 — Zustaendigkeitskette (Chain of Responsibility), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode als if/else-if-Kaskade hinschreiben, Reihenfolgeabhaengigkeit sehen
    // [ ] IApprovalHandler mit Nachfolger (SetNext) und Handle(ExpenseRequest)
    // [ ] ein Handler pro Betragsgrenze: Teamleitung, Abteilungsleitung, Geschaeftsfuehrung
    // [ ] Kette zusammenstecken, Reihenfolge als Test festnageln
    // [ ] Fall "niemand ist zustaendig" explizit modellieren, nicht mit null beantworten
    // [ ] Handler ergaenzen und Reihenfolge aendern, ohne bestehende Handler anzufassen
    // [ ] Tests: Grenzwerte, Kettenabbruch mit Zaehler, umgestellte Reihenfolge anderes Ergebnis
    // [ ] Vergleich mit der ASP.NET-Core-Middleware-Pipeline schriftlich festhalten
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_13");
    }
}
