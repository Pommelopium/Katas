# Kata 14_05 — Einzelstueck (Singleton)

**Erzeugungsmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/singleton)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

Singleton ist das Muster, das am haeufigsten falsch angewendet wird — es ist leicht zu
schreiben, sieht nach Ordnung aus und zieht globalen Zustand hinter sich her. Diese Kata
trainiert deshalb beides: das Muster korrekt bauen **und** erkennen, wann es der falsche
Griff ist. Der spannendste Teil ist nicht der Weg hinein, sondern der Weg **heraus** aus
einem bestehenden Singleton.

## Ziel

Das Muster in fremdem Code erkennen, es threadsicher und ohne Zufall korrekt implementieren
— und vor allem die Folgen fuer die Testbarkeit sehen: Ein Singleton ist ein Objekt, das
zwischen zwei Tests weiterlebt. Wer das gesehen hat, entscheidet danach anders.

## Woran du das Muster erkennst

- Ein privater Konstruktor und ein statisches `Instance`-Property in derselben Klasse.
- `Irgendwas.Instance.MachWas(...)` steht quer durch den Code verteilt — die Abhaengigkeit
  steht nicht im Konstruktor, sondern versteckt mitten in einer Methode.
- Tests beeinflussen sich gegenseitig: einzeln gruen, zusammen rot. Oder umgekehrt.
- Reihenfolgeabhaengigkeit: `dotnet test` ist gruen, mit anderer Sortierung oder paralleler
  Ausfuehrung rot. Irgendwo gibt es eine `Reset()`-Methode, die es nur fuer die Tests gibt.
- Der Zustand ueberlebt laenger als der Vorgang, der ihn erzeugt hat.

## Aufgabe: das Versuchsprotokoll des Kata-Trackers

Der **Kata-Tracker** braucht ein Protokoll der Trainingsversuche: jeder Versuch bekommt eine
laufende Versuchsnummer und einen Eintrag. Weil "das ja nur eine Liste ist und es die nur
einmal geben darf", wurde es als Singleton gebaut. Genau so:

```csharp
public sealed class Versuchsprotokoll
{
    private static Versuchsprotokoll? _instance;

    private readonly List<string> _eintraege = new();
    private int _naechsteNummer = 1;

    private Versuchsprotokoll() { }

    public static Versuchsprotokoll Instance
    {
        get
        {
            if (_instance is null)              // zwei Threads kommen hier gleichzeitig durch
            {
                Thread.Sleep(1);               // macht das Zeitfenster nur sichtbar, nicht schlimmer
                _instance = new Versuchsprotokoll();
            }

            return _instance;
        }
    }

    public int Protokolliere(string kataId)
    {
        int nummer = _naechsteNummer++;        // nicht atomar
        _eintraege.Add($"{nummer}: {kataId}"); // List<T> ist nicht threadsicher
        return nummer;
    }

    public int Anzahl => _eintraege.Count;
}

// Aufrufer, ueberall im Code:
public sealed class AttemptService
{
    public void Record(string kataId) => Versuchsprotokoll.Instance.Protokolliere(kataId);
}
```

Dieser Code hat zwei Fehler, die zusammen die Kata ergeben. Erstens bricht er unter
Nebenlaeufigkeit: zwei Threads erzeugen zwei Instanzen, und `_naechsteNummer++` vergibt
dieselbe Nummer doppelt. Zweitens verkoppelt er die Tests: ein Test, der einen Versuch
erfasst, hinterlaesst `Anzahl == 1` fuer den naechsten. Ein Test, der `Anzahl == 0` erwartet,
ist nur gruen, wenn er zuerst laeuft. `AttemptService` laesst sich ausserdem gar nicht
isoliert testen — die Abhaengigkeit ist nicht sichtbar und nicht ersetzbar.

## Aufgaben

1. **Ausgangszustand nachbauen** und den Schaden beweisen, bevor du etwas repariert: ein Test
   mit 100 parallelen Zugriffen auf `Instance`, der die Anzahl verschiedener Instanzen zaehlt.
   Er muss zuerst rot sein (mindestens gelegentlich). Ein Fehler, den du nicht rot gesehen
   hast, hast du nicht behoben.
2. **Zweiter roter Test, diesmal ohne Threads:** zwei Tests, die beide `Anzahl` pruefen. In
   der einen Reihenfolge gruen, in der anderen rot. Notier, welcher der beiden luegt.
3. **Variante A — statischer Initialisierer:** `private static readonly Versuchsprotokoll
   _instance = new();`. Die Laufzeit garantiert die Einmaligkeit. Halt fest, wann der
   Konstruktor laeuft (Typinitialisierung, `beforefieldinit`) und warum "lazy genug" hier
   meist reicht.
4. **Variante B — `Lazy<T>`:** `new Lazy<Versuchsprotokoll>(() => new(),
   LazyThreadSafetyMode.ExecutionAndPublication)`. Pruefe im Test, dass die Factory bei 100
   parallelen Zugriffen **genau einmal** aufgerufen wird. Probier zum Vergleich
   `PublicationOnly` und beschreibe, was sich dabei aendert.
5. **Variante C — doppelt gepruefte Sperre** (double-checked locking) mit `volatile` und
   eigenem Lock-Objekt. Schreib sie einmal von Hand, damit du sie lesen kannst, und notier
   danach, warum du sie in C# nicht mehr brauchst. Vergleiche die drei Varianten in einer
   kleinen Tabelle: Threadsicherheit, Zeitpunkt der Erzeugung, Lesbarkeit, Kosten pro Zugriff.
6. **Den Zustand selbst absichern:** auch mit einer Instanz bleibt `_naechsteNummer++` falsch.
   Ersetz es durch `Interlocked.Increment` und die Liste durch eine threadsichere Sammlung
   oder ein `lock`. Test: 1000 parallele `Protokolliere`-Aufrufe ergeben 1000 Eintraege mit
   1000 verschiedenen Nummern.
7. **Der Ausweg — dasselbe Problem ohne Singleton:** extrahiere `IVersuchsprotokoll`, mach den
   Konstruktor oeffentlich, entfern `Instance` restlos und lass `AttemptService` die
   Abhaengigkeit im Konstruktor annehmen. Registrier die Implementierung im DI-Container mit
   Lifetime `Singleton`: eine Instanz pro Anwendung, aber die Klasse weiss nichts davon.
8. **Tests entkoppeln:** jeder Test erzeugt sein eigenes `Versuchsprotokoll` oder ein
   Test-Double. Beide Reihenfolgen aus Aufgabe 2 sind jetzt gruen, die Tests laufen parallel,
   und es gibt **keine** `Reset()`-Methode mehr. Loesch sie und schau, ob noch etwas bricht.

## Beispiele und Testfaelle

| Fall | Erwartetes Ergebnis |
|---|---|
| 100 parallele Zugriffe auf `Instance`, Instanzen ueber `ReferenceEquals` gezaehlt | genau **eine** Instanz; mit dem Ausgangscode gelegentlich zwei oder mehr |
| Factory-Aufrufe bei `Lazy<T>` unter denselben 100 Zugriffen | Zaehler steht auf **1**, nicht auf 2 |
| 1000 parallele `Protokolliere("14_05")` | 1000 Eintraege, 1000 **verschiedene** Nummern, keine Luecke |
| Konstruktor-Aufrufe bei 0 Zugriffen (Variante B) | **0** — die Instanz entsteht erst beim ersten Zugriff |
| Test X (`Anzahl == 0`) und Test Y (erfasst einen Versuch) in beiden Reihenfolgen | **beide gruen** — vorher war eine der beiden Reihenfolgen rot |
| Zwei Tests erzeugen je ein eigenes Protokoll und erfassen je einen Versuch | jeder sieht `Anzahl == 1`; der Zustand ist **pro Test isoliert** |
| `AttemptService` mit einem Fake-`IVersuchsprotokoll` | der Fake sieht genau einen Aufruf mit `"14_05"`; kein globaler Zustand beteiligt |
| Zwei Aufloesungen von `IVersuchsprotokoll` aus demselben DI-Container | dieselbe Referenz; aus zwei getrennten Containern **verschiedene** Referenzen |

## Abgrenzung

- **Statische Klasse:** kann keine Schnittstelle implementieren, nicht vererbt und nicht als
  Parameter uebergeben werden. Ein Singleton ist ein Objekt und damit ersetzbar — das ist der
  einzige echte Vorteil gegenueber `static class`, und er verpufft, sobald der Code
  `Instance` direkt aufruft.
- **DI-Lifetime `Singleton`:** dieselbe Garantie (eine Instanz pro Container), aber die
  Klasse selbst bleibt eine normale Klasse mit oeffentlichem Konstruktor. Die Einmaligkeit ist
  Konfiguration, nicht Bauart. Deshalb sind Tests frei, mehrere Instanzen zu erzeugen.
- **Monostate:** viele Instanzen, aber alle Felder `static` — verhaelt sich wie ein Singleton,
  sieht aber wie ein harmloser Typ aus. Dieselben Testprobleme, schlechter zu erkennen.
- **Flyweight:** teilt Objekte ebenfalls, aber aus Speichergruenden und in vielen Exemplaren
  (eines pro Wert). Ziel ist Wiederverwendung unveraenderlicher Daten, nicht Einmaligkeit.

## Wann nicht

- **In einer Anwendung mit DI-Container fast immer.** Wenn ein Container vorhanden ist, ist
  das klassische Singleton die schlechtere Wahl: `services.AddSingleton<IFoo, Foo>()` liefert
  dieselbe Garantie, ohne die Abhaengigkeit zu verstecken und ohne die Tests zu verkoppeln.
- **Nie fuer veraenderlichen Zustand**, der pro Anfrage, pro Mandant oder pro Vorgang
  unterschiedlich ist. Ein Singleton mit veraenderlichem Zustand ist eine globale Variable mit
  Konstruktor — im Webumfeld zusaetzlich ein Nebenlaeufigkeitsproblem und der klassische
  Weg zu mandantenuebergreifenden Datenlecks.
- **Nicht als Abkuerzung zum Durchreichen.** Wenn `Instance` nur benutzt wird, weil das
  Durchreichen durch fuenf Ebenen unbequem waere, ist die Schichtung das Problem, nicht die
  Ergonomie.

## Skills

Singleton, Threadsicherheit (`Lazy<T>`, statische Initialisierung, double-checked locking,
`Interlocked`, `volatile`), Dependency Injection und Lifetimes, Testisolation, Erkennen und
Entfernen von globalem Zustand

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
