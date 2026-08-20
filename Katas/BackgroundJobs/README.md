# Kata 11_07 — Hintergrundjobs und Zeitsteuerung

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende

## Ziel

Jedes System hat irgendwann den naechtlichen Lauf, die Erinnerungsmail, den Report um 6 Uhr.
Die Aufgabe klingt trivial und ist es genau so lange, bis der Prozess zweimal laeuft, mitten
im Lauf neu startet oder die Zeitumstellung kommt.

## Domaene: Kata-Tracker

Der Trainingsplan aus dem Kata-Tracker braucht drei wiederkehrende Laeufe, und sie decken
genau die drei interessanten Muster ab:

- **`StreakRecalculation`** — jede Nacht um 03:00 Ortszeit: fuer jeden Trainierenden die
  laengste Serie aufeinanderfolgender Tage neu berechnen und im Tracker ablegen. Der Job
  arbeitet Zeile fuer Zeile ueber alle Trainierenden — der Kandidat fuer Fortschrittsmarken
  und Wiederaufsetzbarkeit.
- **`PlanReminder`** — jeden Morgen um 06:00 Ortszeit: wer laut Trainingsplan heute eine
  Kata offen hat, bekommt eine Erinnerung. Der Job, der auf keinen Fall zweimal laufen darf,
  weil jeder Lauf nach draussen sichtbar ist.
- **`WeeklyReport`** — montags um 07:00: Wochenbericht je Trainierendem (geloeste Katas,
  Gesamtdauer, Streak) erzeugen und versenden. Der Job, der laenger laeuft als sein eigenes
  Intervall und deshalb ueberlappende Laeufe provoziert.

Verschickt wird nichts wirklich: der Versand ist ein Port, der im Test das Zaehlen der
Erinnerungen erlaubt. Die Fachlichkeit ist absichtlich duenn — geuebt wird die
Zeitsteuerung, nicht die Berechnung.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Ein Job, der Zeilen in einer Tabelle oder einer
Datei verarbeitet, genuegt als Fachlichkeit.
**Empfohlen, nicht erforderlich:** Kata 11_02 (Observability) fuer die Metriken aus Punkt 8.
**Werkzeuge:** Docker Desktop (Datenbank fuer den Job-Store).

## Minimalpfad

Punkte 1, 3, 4 und 6.

## Aufgaben

1. **Erst ohne Framework:** ein `BackgroundService` mit `PeriodicTimer`, der alle N Sekunden
   laeuft. Zeig die zwei Klassiker: eine Exception im Job beendet **still** den ganzen
   Service, und ein langer Lauf verschiebt alle folgenden Termine. Behebe beides.
2. `IHostedService`-Lebenszyklus verstehen: `StartAsync` blockiert den Start der Anwendung,
   wenn du dort wartest. Zeig es, dann mach es richtig.
3. **Zeitplanung mit Cron** (Quartz.NET oder Hangfire). Drei Faelle, die man einmal gesehen
   haben muss:
   - Zeitzone: der Job soll um 06:00 **Ortszeit** laufen, der Server steht auf UTC
   - Sommerzeitumstellung: die Nacht mit 23 und die mit 25 Stunden — was passiert mit dem
     Job um 02:30?
   - Ausfallzeit: der Prozess war zum Termin aus. Nachholen oder ueberspringen? Entscheide
     begruendet und konfiguriere es explizit.
4. **Nur einmal, auch bei drei Instanzen.** Starte den Dienst dreifach und beweise, dass der
   Job nicht dreimal laeuft — ueber einen persistenten Job-Store mit Locking oder ein
   verteiltes Lease mit TTL. Ein `static bool` ist keine Loesung; zeig auch, warum nicht.
5. **Wiederaufsetzbarkeit:** kill den Prozess mitten im Lauf. Nach dem Neustart darf keine
   Zeile doppelt und keine uebersprungen werden. Das erzwingt Fortschrittsmarken statt
   "alles oder nichts".
6. **Graceful Shutdown:** `SIGTERM` waehrend eines laufenden Jobs. Der Job bekommt den
   `CancellationToken`, beendet die aktuelle Einheit und stoppt — er wird nicht mitten in
   einer Transaktion abgeschnitten. `HostOptions.ShutdownTimeout` bewusst setzen.
7. Fehlerbehandlung pro Job: Retry mit Backoff, danach Dead-Letter in eine
   `FailedJob`-Tabelle mit Grund und Nutzlast — und ein Weg, sie gezielt neu zu starten.
8. Betrieb: Metriken fuer Laufdauer, Erfolg/Fehler und **Verspaetung** (geplanter vs.
   tatsaechlicher Start). Die Verspaetung ist die Zahl, die einen ueberlasteten Scheduler
   verraet, bevor Jobs ausfallen.
9. Testbarkeit: die Zeit kommt aus `TimeProvider`, nicht aus `DateTime.Now`. Ein Test muss
   ein Jahr Zeitplan in Millisekunden durchspielen koennen.
10. Vergleich schriftlich: `BackgroundService` + Cron-Bibliothek vs. Hangfire vs.
    Quartz.NET vs. externer Trigger (Kubernetes `CronJob`, Azure Function Timer). Wann was?

## Beispiele und Testfaelle

Die Zeit kommt in allen Faellen aus `TimeProvider`; im Test steht ein `FakeTimeProvider`
darin, dessen Uhr der Test selbst vorstellt. Kein `Task.Delay`, kein `Thread.Sleep`, kein
"warte zwei Sekunden und hoffe" — jeder Fall unten laeuft in Millisekunden und immer gleich.

1. **Termin trifft.** Uhr auf 2026-03-02 02:59:59 Ortszeit, `StreakRecalculation` als Cron
   `0 0 3 * * *` registriert. `Advance(1s)` -> der Job lief genau einmal. Weitere
   `Advance(23h)` -> ein zweiter Lauf, nicht mehr. Der Test prueft neben der Anzahl den
   *geplanten* Startzeitpunkt: `2026-03-02T03:00:00` Ortszeit, obwohl der Prozess auf UTC
   laeuft.
2. **Sommerzeit.** Fuer einen Job um 02:30 Ortszeit: in der Nacht der Vorstellung
   (2026-03-29, 02:30 existiert nicht) gilt die konfigurierte Entscheidung —
   uebersprungen *oder* einmal nachgeholt, aber nicht beides; in der Nacht der
   Rueckstellung (2026-10-25, 02:30 existiert zweimal) laeuft der Job **einmal**, nicht
   zweimal. Beide Naechte sind je ein Test, der die getroffene Entscheidung festschreibt.
3. **Verspaetung wird gemessen.** Geplant 06:00, der Scheduler kommt erst 06:00:07 dazu ->
   die Metrik fuer Verspaetung zeigt 7 Sekunden. Ein Lauf, der punktgenau startet, zeigt 0.
4. **Shutdown mitten im Job.** `WeeklyReport` verarbeitet 100 Trainierende, nach 40
   Berichten wird `StopAsync` ausgeloest. Ergebnis: die 40. Einheit ist **vollstaendig**
   abgeschlossen und festgeschrieben, die 41. wurde nicht angefangen, der Host ist innerhalb
   des `ShutdownTimeout` unten und der Job hat `OperationCanceledException` nicht nach
   draussen gegeben. Halbe Arbeit gibt es nicht: kein Bericht ist teilweise geschrieben.
5. **Wiederaufsetzen ohne Luecke und ohne Duplikat.** Derselbe Lauf wird nach dem Shutdown
   aus Fall 4 neu gestartet: am Ende sind genau 100 Berichte erzeugt — die ersten 40 nicht
   erneut, die restlichen 60 genau einmal. Der Test zaehlt die Aufrufe des Versand-Ports pro
   Trainierendem und erwartet ueberall genau 1.
6. **Retry mit Backoff, dann Fehlerzustand.** Der Versand fuer einen Trainierenden wirft
   dauerhaft. Bei `MaxRetries = 3` gilt: insgesamt 4 Versuche (Erstversuch + 3
   Wiederholungen), die Abstaende dazwischen wachsen nach der konfigurierten Backoff-Kurve
   (z. B. 1s, 2s, 4s — im Test durch Vorstellen der Uhr belegt), danach **kein** fuenfter
   Versuch. Der Job landet als Eintrag in `FailedJob` mit Grund und Nutzlast. Ein Fall, der
   beim dritten Versuch glueckt, erzeugt dagegen keinen `FailedJob`-Eintrag — und die
   uebrigen 99 Trainierenden sind trotz des Fehlers verarbeitet.
7. **Neustart aus dem Fehlerzustand.** Der `FailedJob`-Eintrag aus Fall 6 wird gezielt
   erneut angestossen; der Versand glueckt diesmal. Danach ist der Eintrag erledigt, und der
   betroffene Trainierende hat insgesamt genau einen Bericht — kein zweiter durch den
   Neustart.
8. **Nur einmal bei mehreren Instanzen.** Zwei (oder drei) Scheduler-Instanzen gegen
   denselben Job-Store, Uhr auf 05:59:59, `Advance(1s)`: `PlanReminder` lief **einmal**,
   der Versand-Port wurde je Trainierendem einmal aufgerufen. Derselbe Test mit einem
   `static bool` als "Sperre" faellt durch, sobald die Instanzen getrennte Prozesse sind —
   dieser rote Test ist Teil der Loesung von Aufgabe 4.
9. **Keine ueberlappenden Laeufe.** `WeeklyReport` braucht 90 Sekunden, ist aber alle 60
   Sekunden geplant. Nach fuenf simulierten Minuten gilt: es lief nie mehr als eine Instanz
   des Jobs gleichzeitig (ein Zaehler fuer gleichzeitige Ausfuehrungen erreicht maximal 1),
   und der uebersprungene Termin ist als solcher protokolliert statt still verschluckt.
10. **Eine Exception toetet den Service nicht.** Der erste Lauf wirft, die Uhr laeuft weiter:
    der naechste Termin wird trotzdem ausgefuehrt. Ohne die Behebung aus Aufgabe 1 bleibt
    der zweite Lauf aus — genau das ist der Test, der den Fehler sichtbar macht.

## Nachweise

- Drei laufende Instanzen, ein Joblauf — im Log belegt.
- Kill mitten im Lauf, danach nachweislich keine Duplikate und keine Luecken.

## Skills

`BackgroundService`, `PeriodicTimer`, Quartz.NET/Hangfire, Cron und Zeitzonen,
verteiltes Locking, Wiederaufsetzbarkeit, Graceful Shutdown, `TimeProvider` in Tests

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
