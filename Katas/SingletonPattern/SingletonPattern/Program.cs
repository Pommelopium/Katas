
namespace SingletonPattern;

/// <summary>
///     Kata 14_05 — Einzelstueck (Singleton) (Design-Pattern-Kata)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangszustand nachbauen: naives Instance-Property, das unter Last zwei Instanzen liefert
    // [ ] Nebenlaeufigkeitstest: 100 parallele Zugriffe, genau eine Instanz — muss zuerst rot sein
    // [ ] Variante A: statischer Initialisierer (beforefieldinit, Laufzeit garantiert die Einmaligkeit)
    // [ ] Variante B: Lazy<T> mit LazyThreadSafetyMode.ExecutionAndPublication
    // [ ] Variante C: doppelt gepruefte Sperre (double-checked locking) mit volatile — nur zum Vergleich
    // [ ] Test-Verkopplung zeigen: zwei Tests, deren Ergebnis von der Ausfuehrungsreihenfolge abhaengt
    // [ ] Ausweg: Interface extrahieren, Aufrufer per Konstruktor versorgen, DI-Lifetime Singleton
    // [ ] Tests: pro Test eine frische Instanz, beide Reihenfolgen gruen, kein Reset-Hintertuerchen
    // [ ] Abgrenzung notieren: statische Klasse, DI-Singleton, Monostate, Flyweight
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_05");
    }
}
