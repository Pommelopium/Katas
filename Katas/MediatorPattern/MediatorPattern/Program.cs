
namespace MediatorPattern;

/// <summary>
///     Kata 14_16 — Vermittler (Mediator), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode bauen: vier Dialogkomponenten, die sich direkt kennen und anstossen
    // [ ] Verbindungen zaehlen und die n-zu-n-Verdrahtung als Zahl festhalten
    // [ ] ISessionDialogMediator mit Notify(sender, ereignis) einziehen
    // [ ] Komponenten kennen nur noch den Mediator, keine Geschwisterkomponente mehr
    // [ ] gesamte Verdrahtung in genau einer Klasse: StartSessionDialog
    // [ ] neue Komponente ergaenzen, ohne bestehende Komponenten anzufassen
    // [ ] Rueckkopplung entschaerfen: Reentranz-Schutz statt Endlosschleife
    // [ ] Gegenprobe: eine Komponente mit Fake-Mediator isoliert testen
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_16");
    }
}
