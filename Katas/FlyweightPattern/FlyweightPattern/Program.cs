
namespace FlyweightPattern;

/// <summary>
///     Kata 14_11 — Fliegengewicht (Flyweight) (Design-Pattern-Kata)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] zuerst messen: Bytes pro Objekt und Gesamtbedarf fuer eine Million Zeilen
    // [ ] intrinsischen Zustand (geteilt) von extrinsischem Zustand (je Zeile) trennen
    // [ ] Flyweight-Typ unveraenderlich machen — geteilter Zustand darf niemand aendern
    // [ ] Factory mit Cache: gleicher Schluessel liefert referenzgleiche Instanz
    // [ ] Cache thread-sicher (ConcurrentDictionary, GetOrAdd) und ohne Leck
    // [ ] extrinsischen Zustand von aussen hereingeben statt zu speichern
    // [ ] erneut messen und die beiden Zahlen gegenueberstellen — Faktor notieren
    // [ ] Tests: identisches Verhalten vor und nach der Optimierung, derselbe Testlauf
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_11");
    }
}
