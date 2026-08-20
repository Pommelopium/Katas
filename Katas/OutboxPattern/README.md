# Kata 11_01 — Transactional Outbox

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende

## Ziel

Konsistente Zustandsaenderung ueber zwei Systeme hinweg, ohne verteilte Transaktion.

## Domaene: Kata-Tracker

Dieselbe Codebase wie ab Kata 09_01. Der Kata-Tracker nimmt ueber
`POST /api/v1/katas/{id}/attempts` einen geloesten Versuch auf und schreibt ihn in seine
Datenbank. Neu ist ein **zweiter Dienst**, der Statistik: er hoert auf `AttemptRecorded` und
pflegt daraus seine eigene Sicht — laengste Serie an Uebungstagen, Anzahl der Versuche und
Summe der geuebten Zeit pro Kata. Die beiden Dienste teilen keine Datenbank, sie kennen sich
nur ueber die Nachricht.

Damit hat der Tracker genau das Problem, das diese Kata behandelt: Der Versuch ist fachlich
erst dann erfasst, wenn er in seiner Datenbank steht **und** die Statistik davon erfaehrt.
Faellt das eine ohne das andere aus, zeigt die App `GET /api/v1/stats/streak` eine Serie von 4
und die Statistik eine von 3 — und niemand merkt, welche der beiden Zahlen luegt.

## Das Problem

Wenn ein `Attempt` erfasst wird, soll die Nachricht `AttemptRecorded` publiziert werden.
Naiv:

```csharp
await dbContext.SaveChangesAsync();   // Datenbank
await bus.PublishAsync(evt);          // Broker
```

Zwei Systeme, eine Transaktion fehlt. Crasht der Prozess dazwischen, ist die Nachricht
verloren. Schlaegt der Publish fehl und du wiederholst, kommt sie doppelt.

## Aufgaben

1. Tabelle `OutboxMessage` (`Id`, `Type`, `Payload`, `OccurredAt`, `ProcessedAt`,
   `AttemptCount`, `Error`). Wird in **derselben** Transaktion wie die Fachdaten geschrieben.
2. `BackgroundService`, der unverarbeitete Nachrichten pollt, nach **RabbitMQ** (via Docker)
   publiziert und danach als verarbeitet markiert.
3. Nebenlaeufigkeitsschutz: zwei laufende Instanzen duerfen dieselbe Nachricht nicht
   gleichzeitig greifen (`UPDATE ... OUTPUT` oder Row Locking).
4. Retry mit Backoff pro Nachricht, Dead-Letter nach N Versuchen.
5. Der **Consumer muss idempotent** sein: Dedup ueber `MessageId` in einer
   `ProcessedMessage`-Tabelle.

## Nachweise (das ist die eigentliche Kata)

- Kill den Prozess **zwischen** Commit und Publish. Die Nachricht muss nach dem Neustart
  trotzdem ankommen.
- Publiziere dieselbe Nachricht zweimal. Der Consumer darf nur einmal wirken.
- Schreib auf, warum das **at-least-once** ist und nicht exactly-once.

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test gegen echte Container (Testcontainers oder
Compose). Der Ausgangspunkt ist immer derselbe: ein `POST` auf
`/api/v1/katas/{id}/attempts` mit `{ "solvedOn": "2026-03-01", "duration": "00:42:00" }`.

1. **Absturz nach Commit, vor dem Publish.** Der Publisher wird nach dem Commit der
   Fachtransaktion hart gestoppt, bevor er in RabbitMQ schreibt. Zustand danach: Der
   `Attempt` steht in der Datenbank, die `OutboxMessage` mit `ProcessedAt = null` daneben.
   Nach dem Neustart des `BackgroundService` liegt die Nachricht **ohne** erneuten Request
   in der Queue, und die Statistik zaehlt den Versuch. Kein Aufrufer hat etwas wiederholt.
2. **Fachtransaktion schlaegt fehl.** Der Commit wird abgebrochen (Constraint-Verletzung
   oder erzwungenes Rollback). Danach existieren **weder** `Attempt` **noch** eine Zeile in
   `OutboxMessage` — die Nachricht kann nicht ueber eine Aenderung reden, die es nie gab.
3. **Broker weg.** RabbitMQ wird gestoppt, dann werden drei Versuche erfasst. Alle drei
   `POST` antworten weiter mit `201` — die Fachfunktion haengt nicht am Broker. Die drei
   Nachrichten stehen unverarbeitet in der Outbox, `AttemptCount` steigt mit den Retries.
   Nach dem Start des Brokers werden genau diese drei geliefert, keine mehr und keine
   weniger.
4. **Doppelte Zustellung.** Dieselbe Nachricht (gleiche `MessageId`) wird zweimal
   publiziert. Nach der zweiten Zustellung ist der Zustand der Statistik **bitgleich** zum
   Zustand nach der ersten: eine Zeile in `ProcessedMessage`, ein gezaehlter Versuch,
   `42` Minuten Uebungszeit statt `84`, Streak `1` statt `2`. Der Test vergleicht den
   Zustand vorher und nachher, nicht bloss den Rueckgabewert des Handlers.
5. **Reihenfolge pro Aggregat.** Fuer **eine** Kata werden schnell hintereinander drei
   Versuche erfasst. Der Consumer sieht sie in der Erfassungsreihenfolge (`OccurredAt`
   aufsteigend). Ueber verschiedene Katas hinweg darf die Reihenfolge beliebig sein — halte
   in einem Test fest, wodurch die Ordnung pro Aggregat entsteht (`ORDER BY` beim Abholen,
   Partitionierung ueber die Aggregat-Id, Prefetch von 1 pro Partition) und nicht durch
   Glueck.
6. **Zwei Instanzen, keine Doppelarbeit.** Zwei Publisher laufen parallel gegen dieselbe
   Outbox mit 100 unverarbeiteten Nachrichten. In der Queue landen 100 Nachrichten, jede
   `OutboxMessage` hat genau einen `ProcessedAt`-Wert, und keine wurde von beiden Instanzen
   gegriffen.
7. **Dauerhafter Fehlschlag.** Eine Nachricht mit kaputtem `Payload` (nicht
   deserialisierbar) laeuft in ihre N Versuche. Danach ist sie als Dead Letter markiert,
   `Error` enthaelt die letzte Ausnahme, und sie wird **nicht** weiter gepollt. Der Test
   belegt zusaetzlich, dass die nachfolgenden, gesunden Nachrichten trotzdem durchgehen —
   eine Giftnachricht darf die Outbox nicht blockieren.
8. **Backoff greift.** Bei dauerhaftem Publish-Fehler liegen die Zustellversuche einer
   Nachricht nicht in einer engen Schleife, sondern mit wachsendem Abstand. Der Test prueft
   die berechneten Wartezeiten als Funktion von `AttemptCount`, nicht die Wanduhr.

## Voraussetzung

Docker Desktop (RabbitMQ, SQL Server).

## Skills

Transactional Outbox, At-least-once-Semantik, Idempotenz, `BackgroundService`, RabbitMQ

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
