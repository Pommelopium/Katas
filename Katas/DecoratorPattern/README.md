# Kata 14_09 — Dekorator (Decorator)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/decorator)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Decorator ist
die Antwort auf genau eine Frage: ein Verhalten soll **ergaenzt** werden, ohne die Klasse zu
aendern und ohne dass der Aufrufer etwas anderes sieht als vorher. Wer damit jede Klasse in
drei Huellen wickelt, hat sich einen Stacktrace gebaut, den niemand mehr liest. Am Ende dieser
Kata sollst du im eigenen Code die Stelle benennen koennen, an der ein Querschnittsbelang in
der Fachlogik klebt — und die Stelle, an der ein Dekorator uebertrieben waere.

## Woran du das Muster erkennst

- **Querschnittsbelange stehen in der Fachklasse:** Logging, Caching, Retry, Validierung oder
  Messung sind zwischen die Fachlogik gestreut, und die eigentliche Regel ist zwischen den
  `try`-Bloecken kaum noch zu finden.
- **Boolesche Flags im Konstruktor** (`bool useCache`, `bool withRetry`) oder in der Signatur —
  jede neue Kombination verdoppelt die Zahl der Pfade, die nie alle getestet werden.
- Die Klasse hat **fuenf Gruende zur Aenderung**: neue Fachregel, anderes Log-Format, andere
  Ablaufzeit im Cache, mehr Wiederholungen, neue Metrik. Fuenf Tickets fassen dieselbe Datei an.
- **Kombinationen sollen zur Laufzeit stapelbar sein:** in der Entwicklung ohne Cache, im Test
  ohne Retry, in Produktion alles — heute geregelt ueber `if (config.X)` mitten im Ablauf.
- Es gibt schon eine **Schnittstelle**, und du willst etwas hinzufuegen, das aus Sicht des
  Aufrufers **dieselbe** Schnittstelle bleibt. Aendert sich die Schnittstelle, ist es Adapter.

## Aufgabe: die Serien-Auskunft des Kata-Trackers

Der **Kata-Tracker** zeigt auf der Startseite die aktuelle **Serie** (Streak) an: wie viele
Tage in Folge geuebt wurde. Die Berechnung liest die Versuche eines Nutzers aus einem langsamen
Backend und faltet sie zu einer Zahl. Weil das Backend gelegentlich mit `503` antwortet und die
Startseite oft aufgerufen wird, sind im Laufe der Zeit Retry, Cache und Logging *in* die
Berechnung gewandert — gesteuert ueber Flags:

```csharp
public sealed class StreakService
{
    private readonly bool _useCache;
    private readonly bool _withRetry;
    private readonly Dictionary<string, int> _cache = new();

    public StreakService(bool useCache, bool withRetry) { _useCache = useCache; _withRetry = withRetry; }

    public int GetStreak(string userId)
    {
        Console.WriteLine($"[LOG] GetStreak({userId}) start");

        if (_useCache && _cache.TryGetValue(userId, out int cached))
        {
            Console.WriteLine("[LOG] cache hit");
            return cached;
        }

        int attempt = 0;
        while (true)
        {
            try
            {
                attempt++;
                List<Attempt> attempts = _backend.LoadAttempts(userId);   // langsam, faellt aus
                int streak = CountConsecutiveDays(attempts);              // die eigentliche Fachregel
                if (_useCache) _cache[userId] = streak;
                Console.WriteLine($"[LOG] GetStreak({userId}) = {streak} after {attempt} attempt(s)");
                return streak;
            }
            catch (HttpRequestException) when (_withRetry && attempt < 3)
            {
                Thread.Sleep(100 * attempt);
            }
        }
    }
}
```

Der Schmerz: drei Belange und eine Fachregel in einer Methode, zwei Flags mit vier
Kombinationen, `Thread.Sleep` im Testpfad — und die Frage "wird ein fehlgeschlagener Versuch
gecacht?" ist nur durch Lesen der Verschachtelung zu beantworten. Genau dieses Bild ist das,
was du als Decorator erkennen sollst.

## Aufgaben

1. Schreibe den Ausgangscode oben ab (oder nimm eine vergleichbare Methode aus einem eigenen
   Projekt) und markiere jede Zeile, die **nicht** zur Fachregel gehoert. Halte die Zahl der
   Belange und der Flag-Kombinationen fest — das ist deine Messgroesse.
2. Ziehe die gemeinsame Schnittstelle heraus: `IStreakSource` mit `int GetStreak(string userId)`.
   Der **Kern** `StreakCalculator` implementiert sie und enthaelt danach ausschliesslich das
   Laden und das Zaehlen — kein Log, kein Cache, kein Retry, keine Flags im Konstruktor.
3. Baue **einen Dekorator pro Belang**, jeder implementiert `IStreakSource` und nimmt ein
   `IStreakSource` im Konstruktor: `LoggingStreakSource`, `CachingStreakSource`,
   `RetryingStreakSource`. Jeder darf genau eine Sache tun und die Anfrage weiterreichen.
4. Halte die Dekoratoren testbar: der Retry bekommt seine Wartezeit als Abstraktion
   (`TimeProvider`, `Func<TimeSpan, Task>` o.ae.), der Cache eine Ablaufzeit von aussen. Kein
   `Thread.Sleep` und kein `DateTime.Now` in einem Dekorator.
5. Setze den Stapel in **definierter Reihenfolge** zusammen — von aussen nach innen Logging,
   Caching, Retry, Kern — und schreibe in einem Satz auf, warum der Cache aussen und der Retry
   innen sitzt (ein wiederholter Aufruf soll gar nicht erst passieren, wenn der Wert vorliegt).
6. Registriere den Stapel im **DI-Container**, sodass der Aufrufer nur `IStreakSource` auflaest
   und die Reihenfolge an genau einer Stelle steht (Dekorator-Registrierung von Hand oder per
   `Scrutor`-`Decorate`). Der Aufrufer darf danach keinen Dekoratornamen mehr kennen.
7. Ergaenze einen **neuen Belang**: `TimingStreakSource`, das die Dauer je Aufruf misst und
   meldet. **Keine bestehende Klasse und keine bestehende Datei wird angefasst**; der Diff
   besteht aus einer neuen Datei und einer Zeile in der Registrierung.
8. Optional: mache die Reihenfolge konfigurierbar (Liste von Belangen aus der Konfiguration)
   und notiere in drei Saetzen, was du dabei verlierst — naemlich die Garantie, dass die
   Reihenfolge stimmt, die vorher im Code stand.

## Beispiele und Testfaelle

Alle Faelle mit einem Fake-Backend, das mitzaehlt, wie oft es aufgerufen wurde, und das sich
scharfstellen laesst ("die ersten n Aufrufe werfen `HttpRequestException`").

| Fall | Erwartetes Ergebnis |
|---|---|
| **nackter Kern** ohne jeden Dekorator, Backend liefert 5 Tage in Folge | `5`, genau **1** Backend-Aufruf, keine Ausgabe irgendwo |
| Kern + Caching, zweimal `GetStreak("u1")` | beide Male `5`, aber genau **1** Backend-Aufruf |
| Kern + Caching, `GetStreak("u1")` und `GetStreak("u2")` | **2** Backend-Aufrufe — der Cache-Schluessel enthaelt den Nutzer |
| Kern + Retrying, die ersten 2 Aufrufe werfen | `5`, genau **3** Backend-Aufrufe, keine Exception nach aussen |
| Kern + Retrying, alle Aufrufe werfen | die Exception kommt nach **3** Versuchen durch, nicht endlos |
| voller Stapel, Backend liefert 0 Tage | `0` — und der Wert `0` ist gecacht, nicht als "kein Wert" behandelt |
| voller Stapel, Logging aussen | Log enthaelt **einen** Eintrag pro Aufruf des Aufrufers, nicht einen pro Retry-Versuch |

Dazu drei Faelle, die keine Tabellenzeile sind:

- **Die Reihenfolge ist beobachtbar.** Szenario: die ersten 2 Backend-Aufrufe werfen, der
  dritte liefert `5`; danach wird ein zweites Mal gefragt. Stapel **Cache vor Retry**
  (`Caching(Retrying(Kern))`): 1 Cache-Lesevorgang, 3 Backend-Aufrufe, der zweite Aufruf
  kommt aus dem Cache -> insgesamt **3** Backend-Aufrufe. Stapel **Retry vor Cache**
  (`Retrying(Caching(Kern))`): der Cache wird in **jedem** der 3 Versuche befragt -> **3**
  Cache-Lesevorgaenge bei denselben 3 Backend-Aufrufen; stellt der Cache zusaetzlich
  Fehlschlaege zu (negatives Caching), liefert dieser Stapel beim zweiten Aufruf eine
  Exception statt `5`. Beide Zahlen werden im Test mit Zaehlern belegt und muessen sich
  unterscheiden — ein Test, in dem beide Reihenfolgen dasselbe ergeben, prueft die Reihenfolge
  nicht.
- **Der Kern laeuft ohne jeden Dekorator korrekt.** Alle Fachtests der Serienberechnung
  (Serie ueber Monatsgrenze, Luecke von einem Tag beendet die Serie, zwei Versuche am selben
  Tag zaehlen einmal, leere Historie -> `0`) laufen gegen `new StreakCalculator(backend)`
  **ohne** einen einzigen Dekorator gruen. Wenn ein Fachtest einen Dekorator braucht, ist der
  Belang noch nicht draussen.
- **Der Kern weiss nichts von den Dekoratoren.** Nachweis in zwei Schritten: (a) ein Test
  reflektiert ueber `StreakCalculator` und stellt sicher, dass weder Konstruktor noch Felder
  ein `IStreakSource`, einen Logger oder einen Cache enthalten; (b) derselbe Kern wird in einem
  Test in beliebiger Tiefe gestapelt (`Timing(Logging(Caching(Retrying(Kern))))`) und liefert
  denselben Wert wie nackt. Gegenprobe: nach Aufgabe 7 sind **alle** bestehenden Tests
  unveraendert gruen, und `git diff` zeigt ausser der Registrierungszeile nur eine neue Datei.

## Abgrenzung

- **Adapter** aendert die **Schnittstelle** (fremdes `LegacyStreakApi` -> `IStreakSource`),
  Decorator behaelt sie und ergaenzt **Verhalten**. Faustregel: unterscheiden sich die
  Signaturen von Huelle und Kern, ist es ein Adapter — sind sie identisch, ein Dekorator.
- **Proxy** hat dieselbe Schnittstelle wie der Dekorator, aber ein anderes Motiv: er
  **kontrolliert den Zugriff** (Rechte, Lazy Loading, Remote-Aufruf) und darf den Aufruf
  verweigern oder ganz verschlucken. Ein Dekorator ergaenzt und reicht durch; ein Proxy
  entscheidet, ob ueberhaupt durchgereicht wird.
- **Chain of Responsibility** sucht den **einen** Zustaendigen — die Kette endet, sobald einer
  den Fall erledigt. Beim Dekorator wirken **alle** Glieder mit, jedes Mal, in fester Ordnung.
- **Composite** stapelt nicht, sondern **verzweigt**: ein Knoten haelt *viele* Kinder und
  aggregiert deren Ergebnisse (Kata 14_11). Decorator ist der Sonderfall mit genau einem Kind
  — gleiche Struktur, anderes Ziel.

## Wann nicht

- **Es gibt nur eine Kombination, und sie aendert sich nicht.** Dann sind vier Klassen und eine
  Registrierung teurer als drei Zeilen in der Methode. Warte, bis die zweite Kombination
  wirklich gebraucht wird.
- **.NET-Alternativen, die es schon gibt:** fuer HTTP-Belange `DelegatingHandler` in der
  `HttpClient`-Pipeline, fuer Anfragen am Rand die **ASP.NET-Core-Middleware-Pipeline**, fuer
  Resilienz Polly-Strategien, fuer querschnittliche Handler eine MediatR-Pipeline oder
  **Interceptoren** (Castle DynamicProxy, Source Generators). Von Hand dekorieren lohnt vor
  allem an eigenen Fachschnittstellen.
- **Der Preis:** tiefe Stapel sind im Stacktrace schwer zu lesen — sechs Huellen liefern sechs
  fast identische Frames, und "wer hat den Wert veraendert?" ist nur durch Kenntnis der
  Reihenfolge zu beantworten. Ab etwa drei Dekoratoren gehoert die Reihenfolge dokumentiert und
  getestet, sonst ist sie Zufall.

## Skills

Decorator, Komposition statt Vererbung, Single Responsibility, Open-Closed,
Querschnittsbelange trennen, Dependency Injection mit Dekoratoren, Testen mit Zaehlern und Fakes

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
