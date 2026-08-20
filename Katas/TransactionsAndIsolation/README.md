# Kata 10_03 — Transaktionen, Isolationslevel, Deadlocks

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende · baut auf Kata 09_02 auf

## Ziel

Die Kata, die man nicht durch Lesen bestehen kann. Zwischen "ich kenne die vier
Isolationslevel" und "ich habe jede Anomalie selbst erzeugt" liegt der ganze
Unterschied — und der zeigt sich erst, wenn es unter Last schiefgeht.

## Domaene: Kata-Tracker

Dasselbe Schema wie in Kata 09_02, hier zusammengestrichen auf das, was zum Sperren
gebraucht wird. Das Spielfeld sind drei Tabellen und ein Zaehler:

- `Katas` — `KataId`, `Title`, `Level`, `IsActive` und `AttemptCount` als
  **denormalisierter Zaehler**. Dieser Zaehler ist das Feld, an dem Lost Update passiert;
  `RowVersion` liegt daneben fuer die Optimistic-Concurrency-Variante.
- `Attempts` — `AttemptId`, `KataId`, `SolvedOn`, `DurationMinutes`. Der Datumsbereich
  ueber `SolvedOn` ist das Spielfeld fuer Non-repeatable Read und Phantom Read.
- `TrainingPlans` / `PlanEntries` — ein Plan verweist geordnet auf Katas. Die
  Fachregel "jeder Trainingsplan enthaelt mindestens **eine** aktive Kata" ist der Fall
  fuer Write Skew: sie steht in keiner Constraint, sondern wird vor dem Schreiben geprueft.

Zwei feste Katas — "Bowling" (`KataId` A) und "Taxi" (`KataId` B) — reichen als Sperrpaar
fuer den Deadlock. Die zwei Verbindungen heissen im Folgenden **S1** und **S2**; jeder
Schritt ist nummeriert, damit die Verschraenkung im Test reproduzierbar bleibt (Signale
ueber `TaskCompletionSource`, nicht ueber `Task.Delay`).

## Aufgabe

Erzeuge jede der folgenden Anomalien **absichtlich** und reproduzierbar in einem Test, mit
zwei kontrolliert verschraenkten Verbindungen.

## Aufgaben

1. **Dirty Read** unter `READ UNCOMMITTED` — und zeig, dass er unter `READ COMMITTED`
   verschwindet. Damit ist auch `WITH (NOLOCK)` erklaert: schreib auf, warum es kein
   Performance-Trick ist.
2. **Lost Update**: zwei Leser, beide rechnen `+1`, ein Ergebnis geht verloren. Loese es
   dreimal — mit `UPDATE ... SET x = x + 1`, mit `rowversion` (Optimistic Concurrency aus
   Kata 09_02) und mit `UPDLOCK`-Hint. Vergleich die drei.
3. **Non-repeatable Read** und **Phantom Read** je einmal erzeugen; zeig, welches Level
   sie jeweils verhindert.
4. **Write Skew** unter `SNAPSHOT`: der Fall, der beweist, dass Snapshot Isolation nicht
   Serializable ist. Daran zeigt sich, ob du Snapshot Isolation wirklich verstanden hast.
5. `READ_COMMITTED_SNAPSHOT` einschalten und zeigen, wie sich das Verhalten von
   `READ COMMITTED` dadurch aendert (Row Versioning statt Shared Locks) — inklusive der
   Kosten in `tempdb`.
6. **Deadlock provozieren**: zwei Transaktionen, die zwei Zeilen in *umgekehrter*
   Reihenfolge sperren. Fang das Deadlock-Graph-Ereignis ab (Extended Events oder
   `sys.event_log`) und lies es. Dann behebe es durch **konsistente Sperrreihenfolge** —
   nicht durch Retry.
7. Retry als zweite Verteidigungslinie: Policy fuer Fehler 1205 (Deadlock) und 1222
   (Lock Timeout) mit Backoff und Jitter. Ein Retry darf nur um eine **idempotente**
   Operation liegen — begruende, welche deiner Operationen das erfuellt.
8. Der Klassiker in .NET: `TransactionScope` mit `async`. Zeig, was ohne
   `TransactionScopeAsyncFlowOption.Enabled` passiert. Und miss, wie lange eine
   Transaktion offen bleibt, wenn ein HTTP-Call in ihr steckt — daraus folgt die Regel.
9. `SET LOCK_TIMEOUT` und Transaktions-Timeouts bewusst setzen: eine haengende Transaktion
   darf nicht die ganze API blockieren.

## Beispiele und Testfaelle

Jeder Fall unten ist ein Test mit zwei Verbindungen gegen den Testcontainers-SQL-Server.
Erwartet wird immer ein Paar: derselbe Ablauf ist unter dem falschen Level **rot** und
unter dem richtigen **gruen**. Wo von Logs die Rede ist, wird der Log-Text im Test
geprueft, nicht per Augenschein.

1. **Dirty Read unter `READ UNCOMMITTED`.** `AttemptCount` von A steht auf `5`.
   S1: `BEGIN TRAN`, `UPDATE Katas SET AttemptCount = 99 WHERE KataId = A` — kein Commit.
   S2 (`READ UNCOMMITTED`): liest `99`. S1: `ROLLBACK`. S2 liest jetzt wieder `5` — S2 hat
   einen Wert gesehen, der nie existiert hat. Derselbe Ablauf mit S2 unter
   `READ COMMITTED`: das `SELECT` blockiert bis zum `ROLLBACK` und liefert dann `5`.
   `WITH (NOLOCK)` in S2 statt des Levels ergibt dasselbe falsche `99` — der Test ist der
   Beleg, dass der Hint kein Performance-Schalter ist, sondern ein Korrektheitsverzicht.
2. **Lost Update ohne Sperre.** `AttemptCount` von A steht auf `5`. Beide Sessions lesen
   `5` (Schritt 1 und 2), rechnen in C# `+1` und schreiben `6` (Schritt 3 und 4). Ergebnis
   nach beiden Commits: `6`, obwohl zwei Versuche erfasst wurden — erwartet waere `7`.
   Der Test behauptet genau das: `AttemptCount == 6` ist der reproduzierte Fehler.
3. **Dieselbe Verschraenkung, dreimal korrekt.** Der Ablauf aus Fall 2 endet auf `7`:
   - `UPDATE Katas SET AttemptCount = AttemptCount + 1` — beide Commits gehen durch.
   - `rowversion`: der zweite Schreibvorgang wirft `DbUpdateConcurrencyException`, der
     Aufrufer laedt neu und rechnet nochmal; nach dem Retry steht `7`.
   - `SELECT ... WITH (UPDLOCK)` beim Lesen: S2 blockiert in Schritt 2 bis S1 committet
     und liest dann `6`. Notiere pro Variante, wer wartet und wer scheitert.
4. **Non-repeatable Read und Phantom Read.** Fuer A existieren drei Attempts im Maerz 2026.
   - *Non-repeatable Read*: S1 (`READ COMMITTED`) liest `DurationMinutes` eines Attempts
     (`42`), S2 aendert ihn auf `50` und committet, S1 liest in derselben Transaktion `50`.
     Unter `REPEATABLE READ` liest S1 beide Male `42`, und S2s `UPDATE` blockiert.
   - *Phantom Read*: S1 (`REPEATABLE READ`) zaehlt
     `WHERE SolvedOn BETWEEN '2026-03-01' AND '2026-03-31'` -> `3`. S2 fuegt einen Attempt
     am 2026-03-15 ein und committet — S2 wird **nicht** blockiert, weil eine Zeile
     gesperrt wird, die es noch nicht gibt. S1 zaehlt erneut und erhaelt `4`.
     Unter `SERIALIZABLE` blockiert S2s `INSERT` (Range Lock) und S1 zaehlt beide Male `3`.
5. **Write Skew unter `SNAPSHOT`.** Ein Trainingsplan enthaelt genau zwei aktive Katas,
   A und B. Beide Sessions pruefen unter `SNAPSHOT` "es bleibt mindestens eine aktive
   Kata uebrig" (`COUNT(*) WHERE IsActive = 1` ergibt in beiden Snapshots `2`), dann
   deaktiviert S1 die Kata A und S2 die Kata B. Beide committen ohne Konflikt — sie haben
   verschiedene Zeilen geschrieben. Ergebnis: **null** aktive Katas, die Fachregel ist
   verletzt, ohne dass irgendeine Transaktion fehlgeschlagen ist. Unter `SERIALIZABLE`
   scheitert die zweite Session; alternativ macht ein Schreibzugriff auf eine gemeinsame
   Zeile (Plan-Kopfsatz) den Konflikt fuer Snapshot Isolation sichtbar (Fehler 3960).
6. **`READ_COMMITTED_SNAPSHOT` aendert das Verhalten, nicht das Level.** Der Ablauf aus
   Fall 1 mit S2 unter `READ COMMITTED`: mit `READ_COMMITTED_SNAPSHOT OFF` **blockiert**
   S2s `SELECT` bis zum Ende von S1 (messbar: Wartezeit > 0, `sys.dm_exec_requests` zeigt
   `LCK_M_S`); mit `ON` kehrt es sofort mit dem alten Wert `5` zurueck. Dazu ein Blick auf
   die Version-Store-Groesse in `sys.dm_tran_version_store_space_usage` vor und nach dem
   Lauf — die Kosten stehen in `tempdb`.
7. **Deadlock absichtlich erzeugt und im Log nachgewiesen.** S1: `UPDATE` auf A, dann
   `UPDATE` auf B. S2: `UPDATE` auf B, dann `UPDATE` auf A. Genau eine der beiden
   Sessions endet mit `SqlException` und **Fehler 1205**, die andere committet. Der Test
   prueft zusaetzlich den abgefangenen **Deadlock-Graph** (Extended Events bzw.
   `system_health`): er enthaelt beide Prozesse, die zwei betroffenen Ressourcen und das
   gewaehlte Opfer. Danach greifen beide Sessions in derselben Reihenfolge (A vor B) zu:
   derselbe Test laeuft 100 Mal ohne ein einziges 1205 durch — behoben durch
   Sperrreihenfolge, mit ausgeschaltetem Retry.
8. **Retry und Lock-Timeout begrenzen den Schaden.** Mit Retry-Policy auf 1205 laeuft der
   Deadlock-Ablauf aus Fall 7 gruen durch, und der Zaehler wird genau einmal erhoeht — der
   Beweis, dass die wiederholte Operation idempotent ist. Fuer 1222: S1 haelt eine Sperre
   auf A, S2 setzt `SET LOCK_TIMEOUT 200` und scheitert nach ~200 ms mit Fehler 1222
   statt unbegrenzt zu warten. Der Test misst die Wartezeit.
9. **Die Matrix als Test, nicht als Tabelle.** Ein parametrisierter Test ueber
   (Anomalie x Isolationslevel) haelt fest, wer wen verhindert: Dirty Read faellt ab
   `READ COMMITTED` weg, Non-repeatable Read ab `REPEATABLE READ`, Phantom Read erst ab
   `SERIALIZABLE`, Lost Update bei keinem Level ohne passendes Schreibmuster, Write Skew
   unter `SNAPSHOT` trotz Konsistenz der Leseansicht. Jede Zelle ist ein gruener Test —
   entweder "Anomalie tritt auf" oder "Anomalie tritt nicht auf".
10. **`TransactionScope` mit `async`.** Ein `await` innerhalb eines `TransactionScope`
    ohne `TransactionScopeAsyncFlowOption.Enabled`: der Test faengt die
    `InvalidOperationException` bzw. weist nach, dass die Arbeit nach dem `await`
    **ausserhalb** der Transaktion landet (sie ueberlebt ein `Dispose` ohne `Complete`).
    Mit gesetzter Option verschwindet beides. Dazu ein Fall mit einem simulierten
    HTTP-Call (500 ms) in der Transaktion: gemessene Offenhaltedauer > 500 ms, danach
    derselbe Ablauf mit dem Aufruf **vor** `BEGIN` — Offenhaltedauer im Millisekunden-
    bereich. Daraus folgt die Regel, und die Messung ist ihre Begruendung.

## Nachweise

Pro Anomalie ein **laufender Test**, der sie unter dem falschen Level zeigt und unter dem
richtigen nicht mehr. Plus eine Seite: welches Level nutzt dein System als Default und
warum.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine Tabelle mit einer Zahlenspalte reicht als
Spielfeld fuer alle Anomalien.
**Empfohlen, nicht erforderlich:** Kata 09_02 (`rowversion` fuer Punkt 2).
**Werkzeuge:** Docker Desktop (SQL Server via Testcontainers).

## Skills

ACID in der Praxis, Isolationslevel, Snapshot Isolation, Write Skew, Deadlock-Analyse,
Sperrreihenfolge, Retry-Policies, `TransactionScope` mit `async`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
