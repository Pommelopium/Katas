
namespace CompositePattern;

/// <summary>
///     Kata 14_08 — Kompositum (Composite), Strukturmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode hinschreiben: Typpruefung und Handrekursion im Aufrufer
    // [ ] gemeinsame Schnittstelle ICurriculumNode fuer Blatt und Gruppe
    // [ ] KataItem als Blatt, CurriculumFolder als Kompositum — Rekursion wandert in die Gruppe
    // [ ] Add/Remove verorten: Transparenz gegen Sicherheit, Entscheidung schriftlich begruenden
    // [ ] neuer Knotentyp (PruefungsBlock) ohne Aenderung am Aufrufer
    // [ ] Grenzfaelle: leere Gruppe, tiefe Verschachtelung, Selbsteinfuegung und Zyklus
    // [ ] Tests: Summen ueber den Baum, kein einziges "is"/"as" mehr im Aufrufer
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_08");
    }
}
