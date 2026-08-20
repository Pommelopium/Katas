
namespace PrototypePattern;

/// <summary>
///     Kata 14_04 — Prototyp (Prototype) (Design-Pattern-Kata)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangszustand nachbauen: Kopierlogik im Aufrufer, die an privaten Feldern bricht
    // [ ] IPrototype<T> bzw. Clone() auf dem Objekt selbst — Kopie kennt ihre eigenen Interna
    // [ ] Copy-Konstruktor protected, Clone() virtual — abgeleiteter Typ bleibt abgeleiteter Typ
    // [ ] flache gegen tiefe Kopie: bewusst entscheiden und im Test unterscheiden
    // [ ] zyklische Referenzen ueber eine Kopier-Map (Dictionary<object, object>) abfangen
    // [ ] PrototypeRegistry: benannte Vorlagen ausgeben, ohne die konkrete Klasse zu kennen
    // [ ] Tests: Aenderung an der Kopie laesst das Original unberuehrt
    // [ ] Abgrenzung notieren: record-with, MemberwiseClone, Serialisierungs-Roundtrip
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_04");
    }
}
