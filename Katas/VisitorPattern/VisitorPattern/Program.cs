
namespace VisitorPattern;

/// <summary>
///     Kata 14_22 — Besucher (Visitor), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode bauen: is-Kaskade im Aufrufer und Exportmethoden in den Domaenentypen
    // [ ] IFormelBesucher<T> mit genau einer Methode je Knotentyp
    // [ ] Accept in der Hierarchie — Double Dispatch, kein is und kein switch im Besucher
    // [ ] zwei Besucher: Auswerten und Ausgeben, gleicher Baum, zwei Ergebnisse
    // [ ] dritter Besucher (Vereinfachen) ohne eine Zeile Aenderung an der Hierarchie
    // [ ] Gegenprobe: neuer Knotentyp Minimum — alle Besucher werden zum Compilezeitfehler
    // [ ] Alternative gegenrechnen: switch-Ausdruck ueber die Hierarchie, Verlust benennen
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_22");
    }
}
