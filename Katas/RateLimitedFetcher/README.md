# Kata 08_01 — Rate-limited Fetcher mit Retry

**Stufe 2: Async und Nebenlaeufigkeit** · Zeitrahmen: 3–4 h

## Ziel

Der Async-Teil, an dem die meisten stolpern. Nicht `await` an sich,
sondern: Parallelitaet begrenzen, Abbruch sauber durchreichen, Zeit testbar machen.

## Aufgabe: der Katalog-Sync

Der Kata-Tracker (ab Kata 09_01) braucht seinen Katalog: Fuer die 81 CCD-Katas liegen die
Aufgabenstellungen als URLs vor, die abgerufen und zu `Document`-Eintraegen werden. Die
Gegenseite ist ein fremder Webserver — also darf der Sync ihn nicht mit 81 gleichzeitigen
Requests ueberfahren, muss einzelne Ausfaelle wegstecken und sich jederzeit abbrechen
lassen, ohne den Rest des Laufs zu verlieren.

## Aufgaben

1. Service, der eine Liste von URLs abruft mit **maximal N parallelen Requests**
   (`SemaphoreSlim` oder `Parallel.ForEachAsync` mit `MaxDegreeOfParallelism`).
2. Retry mit exponentiellem Backoff **plus Jitter** — erst von Hand implementieren, dann
   durch **Polly** ersetzen und vergleichen.
3. `CancellationToken` als Parameter bis in **jeden** Aufruf. Abbruch muss innerhalb von
   Millisekunden greifen, auch mitten im Backoff-Delay.
4. `IHttpClientFactory` statt `new HttpClient()` — Socket Exhaustion verstehen.
5. Rueckgabe als `IAsyncEnumerable<Result<Document>>` (nutzt `Result<T>` aus Kata 07_02):
   Ergebnisse werden geliefert **sobald sie da sind**, nicht erst am Ende.

## Tests

- `HttpMessageHandler` mocken (eigene Subklasse reicht, kein Framework noetig)
- Timeout, Cancellation und Retry-Timing deterministisch pruefen — **ohne** `Thread.Sleep`
  und ohne echtes `Task.Delay` in der Testmethode
- Zeit ueber `TimeProvider` abstrahieren, in Tests `FakeTimeProvider` aus
  `Microsoft.Extensions.TimeProvider.Testing`
- Nachweisen, dass nie mehr als N Requests gleichzeitig laufen

## Beispiele und Testfaelle

- **Parallelitaet begrenzt:** 81 URLs, Limit 5. Ein `Interlocked`-Zaehler im Fake-Handler
  haelt die Hochwassermarke fest: sie ist genau 5, nie 6. Alle 81 Ergebnisse kommen an.
- **Happy Path:** 20 URLs, alle antworten `200 OK` — 20 Erfolgs-Results, kein Fehler,
  und der Handler wurde genau 20-mal aufgerufen (kein ueberfluessiger Retry).
- **Retry mit Erfolg:** Eine URL antwortet zweimal `500`, dann `200`. Ergebnis: Erfolg nach
  3 Aufrufen, mit Backoff-Delays von 200 ms und 400 ms — im Test durch
  `FakeTimeProvider.Advance` uebersprungen.
- **Retry ausgeschoepft:** Eine URL antwortet dauerhaft `503`. Nach 1 Versuch + 3 Retries
  gibt es ein Fehler-Result (`Result<Document>` aus Kata 07_02) mit dem letzten Statuscode —
  **keine** Exception nach aussen. Die uebrigen 80 URLs sind davon unberuehrt.
- **Jitter:** 100 Backoff-Berechnungen fuer Versuch 3 (Basis 800 ms) liegen alle im Fenster
  800–1200 ms und sind nicht alle identisch.
- **Abbruch im Delay:** Backoff von 1000 ms laeuft, nach 100 ms wird der `CancellationToken`
  ausgeloest. Der Aufruf endet mit `OperationCanceledException` (bzw. abgebrochenem Lauf),
  ohne den Delay auszusitzen, und der Handler wird danach nicht mehr aufgerufen.
- **Timeout pro Request:** Request-Timeout 2 s, der Handler antwortet nie. Nach
  `Advance(2 s)` liefert diese URL ein Fehler-Result "Timeout"; der Gesamtlauf laeuft weiter.
- **Streaming statt Sammeln:** 3 URLs, die zweite antwortet spaet. Das erste Ergebnis wird
  aus dem `IAsyncEnumerable` konsumiert, **bevor** die zweite fertig ist — die
  Ergebnisreihenfolge ist ausdruecklich nicht die Eingabereihenfolge.

## Fertig, wenn

Die komplette Testsuite in unter einer Sekunde durchlaeuft, obwohl sie Backoff-Delays von
mehreren Sekunden abdeckt.

## Skills

`async`/`await`, `SemaphoreSlim`, `CancellationToken`, Polly, `TimeProvider`, `IHttpClientFactory`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
