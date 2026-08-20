
namespace AdapterPattern;

/// <summary>
///     Kata 14_06 — Adapter (Adapter), Strukturmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode hinschreiben: Fremd-API direkt im Domaenencode, Konvertierung dreimal kopiert
    // [ ] Zielinterface aus der Sicht der Domaene formulieren (ITimeSource), nicht aus der des Anbieters
    // [ ] Objektadapter TimeTrackrAdapter: Fremdclient gekapselt, Konvertierung an genau einer Stelle
    // [ ] Fremdtypen und Fremd-Exceptions verlassen den Adapter nicht
    // [ ] zweiter Anbieter (ClockifyAdapter) hinter demselben Zielinterface
    // [ ] Klassenadapter als Gegenprobe, Entscheidung schriftlich festhalten
    // [ ] Tests: Domaene ohne Fremdtyp testbar, beide Adapter gegen dieselbe Testsuite
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_06");
    }
}
