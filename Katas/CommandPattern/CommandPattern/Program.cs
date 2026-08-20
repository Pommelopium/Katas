
namespace CommandPattern;

/// <summary>
///     Kata 14_14 — Befehl (Command), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode hinschreiben: Event-Handler ruft die Fachlogik direkt, kein Undo moeglich
    // [ ] ICommand mit Execute und Undo, jeder Befehl haelt seine eigenen Parameter
    // [ ] Invoker mit Verlauf: Undo-Stack und Redo-Stack, Redo verfaellt bei neuer Aktion
    // [ ] MacroCommand: mehrere Befehle als einer, Undo in umgekehrter Reihenfolge
    // [ ] fehlgeschlagener Execute landet nicht im Verlauf
    // [ ] Befehl serialisieren und aus einer Warteschlange spaeter ausfuehren
    // [ ] dieselbe Aktion an Button, Tastenkuerzel und Menue haengen — ein Befehl, drei Ausloeser
    // [ ] Tests: drei Aktionen, dreimal Undo, Ausgangszustand exakt wiederhergestellt
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_14");
    }
}
