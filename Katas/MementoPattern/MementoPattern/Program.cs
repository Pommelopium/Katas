
namespace MementoPattern;

/// <summary>
///     Kata 14_17 — Memento (Memento), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode bauen: der Aufrufer sichert Feld fuer Feld von aussen und vergisst eines
    // [ ] Felder wieder privat machen — kein Zustand nur zum Sichern oeffentlich
    // [ ] IPlanSnapshot als undurchsichtiger Schnappschuss, Caretaker sieht nicht hinein
    // [ ] Originator: Save() erzeugt den Schnappschuss, Restore(snapshot) stellt ihn her
    // [ ] Caretaker mit Verlauf: Undo/Redo ueber einen Stapel von Schnappschuessen
    // [ ] tiefe Objektgraphen: Listen und Kinder tief kopieren, nicht die Referenz teilen
    // [ ] fremdes Memento wird bei Restore abgewiesen
    // [ ] Kombination mit Command (Kata 14_14): Undo ueber Schnappschuss statt Gegenaktion
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_17");
    }
}
