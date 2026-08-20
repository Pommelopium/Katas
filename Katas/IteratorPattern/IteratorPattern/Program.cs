
namespace IteratorPattern;

/// <summary>
///     Kata 14_15 — Iterator (Iterator), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode hinschreiben: Aufrufer greift auf die interne Liste zu und laeuft sie per Index ab
    // [ ] Iterator von Hand: IEnumerator<T> mit MoveNext / Current / Reset, Zustand im Iterator
    // [ ] derselbe Durchlauf mit yield return — Zeilen vorher und nachher zaehlen
    // [ ] mehrere benannte Traversierungen als Properties (Tiefe zuerst, Breite zuerst, nur Blaetter)
    // [ ] foreach-Faehigkeit ueber IEnumerable<T>, zwei gleichzeitige Iteratoren stoeren sich nicht
    // [ ] definiertes Verhalten bei Aenderung der Sammlung waehrend der Iteration (Versionszaehler)
    // [ ] verzoegerte Auswertung: unendlicher Iterator, Take(3) berechnet nur 3 Elemente
    // [ ] Tests: erwartete Reihenfolgen je Traversierung, Zaehler beweist die verzoegerte Auswertung
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_15");
    }
}
