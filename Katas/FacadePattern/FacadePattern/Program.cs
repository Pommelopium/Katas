
namespace FacadePattern;

/// <summary>
///     Kata 14_10 — Fassade (Facade) (Design-Pattern-Kata)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangszustand nachbauen: ein Aufrufer orchestriert sieben Subsystemtypen von Hand
    // [ ] Reihenfolge und Aufraeumen im Fehlerfall als Test festnageln, bevor etwas umzieht
    // [ ] TrainingSessionFacade mit fachlichen Methoden — kein Sammelbecken von Delegationen
    // [ ] Aufrufer haelt nur noch die Fassade: keine using-Direktive auf das Subsystem mehr
    // [ ] Subsystem bleibt oeffentlich — der direkte Weg ist fuer Sonderfaelle weiter offen
    // [ ] Grenze der Fassade schriftlich festlegen, damit sie kein God Object wird
    // [ ] Abgrenzung notieren: Adapter, Mediator, Proxy
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_10");
    }
}
