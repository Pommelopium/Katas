
namespace ObserverPattern;

/// <summary>
///     Kata 14_18 — Beobachter (Observer), Verhaltensmuster
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangscode: eine Quelle ruft drei Empfaenger namentlich auf
    // [ ] eigenes ISubject / IObserver von Hand: Subscribe, Unsubscribe, Notify
    // [ ] dasselbe mit event / EventHandler<AttemptRecordedEventArgs>
    // [ ] dasselbe mit IObservable<T> / IObserver<T> inklusive OnCompleted und OnError
    // [ ] ein werfender Observer darf die Zustellung an die anderen nicht verhindern
    // [ ] An-/Abmeldung mitten in einer laufenden Benachrichtigung: Kopie der Liste
    // [ ] Reihenfolge und Nebenlaeufigkeit bewusst entscheiden und festnageln
    // [ ] Speicherleck nachweisen: ohne Unsubscribe bleibt der Observer per WeakReference am Leben
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_18");
    }
}
