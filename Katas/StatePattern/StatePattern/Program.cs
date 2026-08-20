
namespace StatePattern;

/// <summary>
///     Kata 14_19 — Zustand (State), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode hinschreiben: zwei Methoden mit demselben switch ueber das Status-Enum
    // [ ] Uebergangstabelle aufschreiben und als parametrisierten Test absichern
    // [ ] ein Typ pro Zustand, gemeinsames ISessionState — kein switch mehr im Kontext
    // [ ] Uebergaenge als Rueckgabewert des Zustands, nicht als Zuweisung von aussen
    // [ ] unerlaubter Uebergang scheitert an genau einer Stelle mit definiertem Fehler
    // [ ] neuer Zustand Abgelaufen: eine neue Klasse, genau eine bestehende angefasst
    // [ ] Persistenz klaeren: welcher Zustand steht in der Datenbank, wie wird er rekonstruiert
    // [ ] Tests: Happy Path als Zustandsfolge, jede verbotene Kombination einzeln geprueft
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_19");
    }
}
