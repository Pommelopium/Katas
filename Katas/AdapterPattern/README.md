# Kata 14_06 — Adapter (Adapter)

**Strukturmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/adapter)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Adapter ist
geuebt, wenn du in fremdem Code die Naht findest, an der eine unpassende Fremdschnittstelle in
die eigene Sprache uebersetzt wird, und wenn du danach einen zweiten Anbieter anschliessen
kannst, ohne eine Zeile Domaenencode anzufassen. Wer jede Bibliothek vorsorglich hinter ein
eigenes Interface legt, hat die Kata nicht bestanden — auch wenn die Schichten hinterher
lehrbuchmaessig aussehen.

## Woran du das Muster erkennst

- Eine **fremde Bibliothek** hat die richtige Faehigkeit, aber die falsche Signatur: sie liefert
  Epoch-Millisekunden, wo du ein `DateOnly` brauchst, und `"1h 35m"`, wo du `int` brauchst.
- Der **Konvertierungscode ist ueber die halbe Codebase verstreut**: dieselbe Umrechnung steht
  an drei Stellen, leicht unterschiedlich. Eine Zeitzonenkorrektur muss dreimal gemacht werden,
  und beim dritten Mal wird sie vergessen.
- **Fremdtypen sickern in die Domaene**: Methodensignaturen und Testklassen im Kern nennen Typen
  aus dem SDK. Ein Test der Fachregel braucht plausiblerweise HTTP, Credentials oder das SDK.
- Die **Bibliothek ist nicht aenderbar** — sie kommt als NuGet-Paket, ist generiert oder wird von
  einem anderen Team gepflegt. Du kannst nur *drumherum* etwas bauen.
- Ein **zweiter Anbieter** derselben Sache steht an, und die Frage "wo muss ich dafuer suchen"
  hat keine Antwort in einer einzelnen Datei.
- Fehler kommen als **fremde Exception-Typen** hoch und werden im Domaenencode gefangen — die
  Fachlogik kennt die Fehlerformate eines Drittsystems.

## Aufgabe: Der Zeiterfassungs-Import des Kata-Trackers

Der **Kata-Tracker** soll Versuche nicht mehr nur per Hand erfassen, sondern aus einer externen
Zeiterfassung importieren. Das Unternehmen benutzt **TimeTrackr**, ein NuGet-Paket mit eigenen
Vorstellungen: Zeitstempel als Epoch-Millisekunden, Dauer als formatierter String, Kata-Kuerzel
im Feld `Tag`, Fehler als `TimeTrackrApiException`. Aendern lassen sich diese Typen nicht.
Der heutige Stand sieht so aus:

```csharp
public sealed class ImportService
{
    private readonly TimeTrackrClient _client = new("api-key-from-config");

    public IReadOnlyList<Attempt> ImportLastWeek()
    {
        var entries = _client.FetchEntries(from: 1755000000000, to: 1755600000000);
        var result = new List<Attempt>();

        foreach (var e in entries)
        {
            // Konvertierung, Vorkommen 1 von 3
            var date = DateTimeOffset.FromUnixTimeMilliseconds(e.StartedAtEpochMs).Date;
            var parts = e.Duration.Split('h', 'm');
            var minutes = int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
            result.Add(new Attempt(e.Tag, DateOnly.FromDateTime(date), minutes));
        }

        return result;
    }
}

public sealed class StatisticsService
{
    private readonly TimeTrackrClient _client = new("api-key-from-config");

    public int TotalMinutes(long fromEpochMs, long toEpochMs)
    {
        var total = 0;

        foreach (var e in _client.FetchEntries(fromEpochMs, toEpochMs))
        {
            // Konvertierung, Vorkommen 2 von 3 — hier ohne Schutz gegen "45m"
            var parts = e.Duration.Split('h', 'm');
            total += int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
        }

        return total;
    }
}

public static class TrackerCli
{
    public static void PrintToday(TimeTrackrClient client)
    {
        try
        {
            foreach (var e in client.FetchEntries(TodayStartMs(), TodayEndMs()))
            {
                // Konvertierung, Vorkommen 3 von 3 — hier mit Local statt UTC
                var date = DateTimeOffset.FromUnixTimeMilliseconds(e.StartedAtEpochMs).LocalDateTime;
                Console.WriteLine($"{e.Tag} | {date:d} | {e.Duration}");
            }
        }
        catch (TimeTrackrApiException ex)
        {
            Console.WriteLine($"Zeiterfassung nicht erreichbar: {ex.StatusCode}");
        }
    }
}
```

Der Schmerz ist bewusst klein und typisch: dieselbe Umrechnung steht dreimal da, jedes Mal ein
bisschen anders (einmal UTC, einmal Local, einmal ohne Sonderfall), der Fremdtyp
`TimeTrackrEntry` und die Epoch-Millisekunden stehen mitten in der Fachlogik, und
`TimeTrackrApiException` wird an der Konsole gefangen. Ein zweiter Anbieter waere ein
Suchlauf durch die ganze Codebase. Genau dieser Codeblock ist das, was du in fremdem Code
erkennen sollst.

## Aufgaben

1. Schreib den Ausgangscode oben ab (den Fremdclient als kleine Fake-Bibliothek mit
   `TimeTrackrEntry`, `FetchEntries` und `TimeTrackrApiException`) und sichere das heutige
   Verhalten mit Tests ab. Ohne gruenes Netz ist alles Folgende Umbau auf Verdacht.
2. Formuliere das **Zielinterface aus der Sicht der Domaene**, nicht aus der des Anbieters:
   `ITimeSource` mit `IReadOnlyList<TrackedSpan> Fetch(DateOnly from, DateOnly to)` und
   `TrackedSpan(string KataId, DateOnly Date, int Minutes)`. Kein `long`, kein `string`-Duration,
   kein Fremdtyp in der Signatur. Wenn du das Interface aus der SDK-Doku ableitest, hast du
   diesen Schritt falsch gemacht.
3. Bau den **Objektadapter** `TimeTrackrTimeSource : ITimeSource`: der Fremdclient wird per
   Konstruktor hereingegeben, die Konvertierung existiert danach genau **einmal**. Die drei
   Kopien werden geloescht, nicht "vorerst behalten".
4. Klaere die Grenze: **welcher Typ darf den Adapter verlassen?** `TimeTrackrEntry` und
   `TimeTrackrApiException` duerfen nach diesem Schritt nirgends ausserhalb des Adapters und
   seiner Tests vorkommen — pruef es mit einer Volltextsuche, nicht mit dem Gefuehl.
5. Beantworte: **wer uebersetzt die Fehler des Fremdsystems in die eigene Fehlerform?** Der
   Adapter, nicht der Aufrufer. Leg die Zielform fest (`TimeSourceUnavailableException` oder ein
   `Result<T>` aus Kata 07_02) und entscheide bewusst, was **nicht** uebersetzt wird: ein
   Programmierfehler wie `ArgumentNullException` gehoert nicht in die Fachfehlerform.
6. **Der Beweis:** ergaenze einen **zweiten Anbieter** `ClockifyTimeSource` hinter demselben
   Zielinterface — anderes Datumsformat (ISO-8601-String), Dauer in Sekunden, Kuerzel im Feld
   `Description`, eigene Exception. Domaene und bestehende Tests bleiben unangetastet; der Diff
   dieses Schritts darf ausser der Registrierung nur neue Dateien enthalten.
7. Gegenprobe **Objektadapter gegen Klassenadapter**: bau die Variante, die vom Fremdtyp erbt
   (`class TimeTrackrTimeSource : TimeTrackrClient, ITimeSource`). Halt in drei Saetzen fest,
   warum in C# ohne Mehrfachvererbung, mit `sealed`-Fremdklassen und aus Testbarkeitsgruenden
   praktisch immer der Objektadapter gewinnt — und wann nicht.
8. Optional, als Ausbaustufe: ein **Two-Way-Adapter**, der Versuche auch *zurueck* in die
   Zeiterfassung schreibt. Prueft, ob dein Zielinterface dafuer wirklich taugt oder ob du zwei
   getrennte Schnittstellen brauchst.

## Beispiele und Testfaelle

Fremdeintraege `E1 = { Tag = "07_02", StartedAtEpochMs = 1755561600000, Duration = "1h 35m" }`
und `E2 = { Tag = "14_06", StartedAtEpochMs = 1755648000000, Duration = "45m" }`.

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `adapter.Fetch(2026-08-19, 2026-08-20)` mit `E1`, `E2` | zwei `TrackedSpan`: `("07_02", 2026-08-19, 95)` und `("14_06", 2026-08-20, 45)` |
| Eintrag mit `Duration = "45m"` (ohne Stundenanteil) | 45 Minuten — der Fall, der in Kopie 2 des Ausgangscodes geworfen hat |
| Eintrag mit `Duration = "2h"` und `Duration = "0m"` | 120 bzw. 0 Minuten, keine Exception, kein `null` |
| **Domaene ohne Fremdtyp:** Test der Importregel gegen ein `FakeTimeSource : ITimeSource` | gruen **ohne** `TimeTrackrClient`, ohne HTTP, ohne API-Key im Test |
| **Fremd-Exception:** Client wirft `TimeTrackrApiException(503)` | Aufrufer erhaelt `TimeSourceUnavailableException` (bzw. `Failure("timesource.unavailable")`); die Fremd-Exception haengt als `InnerException` dran und tritt in keiner Aufrufersignatur auf |
| **Zwei Anbieter, eine Testsuite:** dieselbe parametrisierte Suite gegen `TimeTrackrTimeSource` und `ClockifyTimeSource` | beide identisch gruen; die Suite kennt **keinen** der beiden Anbietertypen, nur `ITimeSource` |
| Zeitzone: Eintrag um `2026-08-19T23:30Z` bei Testkultur `Europe/Berlin` | genau ein festgelegtes Datum in **allen** Anbietern — dieser Test entlarvt die UTC/Local-Abweichung aus dem Ausgangscode |
| Leere Antwort des Fremdsystems | leere Liste, nicht `null`, keine Exception |
| Volltextsuche nach `TimeTrackr` im Projekt (Aufgabe 4) | Treffer nur im Adapter, in der Fake-Bibliothek und in den Adaptertests — nicht in Domaene oder CLI |

## Abgrenzung

- **Decorator:** behaelt die Schnittstelle und **aendert das Verhalten** (Caching, Retry, Logging
  um denselben Vertrag). Adapter behaelt das Verhalten und **aendert die Schnittstelle**. Frag:
  sieht der Aufrufer nach dem Umbau dieselbe Signatur wie vorher? Dann ist es Decorator. Beides
  zugleich in einer Klasse ist der haeufigste Fehler dieser Kata.
- **Facade:** vereinfacht ein **ganzes Subsystem** — viele Klassen, viele Aufrufe, eine bequeme
  Einstiegsmethode. Adapter uebersetzt **eine** Schnittstelle in **eine** andere, ohne den
  Umfang zu reduzieren. Merkmal: eine Facade hat kein vorgegebenes Zielinterface, ein Adapter
  hat immer eines, das schon existiert.
- **Proxy:** hat **dieselbe** Schnittstelle wie das Original und kontrolliert den **Zugriff**
  (Lazy Loading, Rechte, Remote-Aufruf). Kein Uebersetzen, kein neuer Vertrag.
- **Bridge:** wird **im Entwurf geplant**, um Abstraktion und Implementierung von Anfang an
  getrennt zu entwickeln. Adapter kommt **nachtraeglich** und versoehnt zwei Dinge, die es
  schon gibt und die nicht fuereinander gebaut wurden.

## Wann nicht

- **Die Fremdschnittstelle passt schon.** Ein Interface, das dieselbe Signatur nur noch einmal
  hinschreibt, ist kein Adapter, sondern eine Datei mehr. Erst der zweite Anbieter oder ein
  nicht testbarer Kern rechtfertigt die Naht.
- **Es ist nur eine Formatfrage.** Fuer reine Typkonvertierung reicht die Sprache: eine
  **Extension Method** (`this TimeTrackrEntry e => e.ToTrackedSpan()`), eine **implizite
  Konvertierung** (`public static implicit operator TrackedSpan(TimeTrackrEntry e)`) oder ein
  `record` mit statischer `From(...)`-Methode. Kein Interface, keine Klasse, kein Muster.
- **Die Bibliothek ist deine eigene.** Dann korrigier die Schnittstelle an der Quelle, statt die
  Fehlentscheidung mit einer Uebersetzungsschicht dauerhaft festzuschreiben.

## Skills

Adapter, Objektadapter vs. Klassenadapter, Ports and Adapters, Uebersetzung von Fremdfehlern,
Erkennen verstreuter Konvertierungslogik, Testbarkeit ohne Fremdtypen, Abgrenzung der
Strukturmuster

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
