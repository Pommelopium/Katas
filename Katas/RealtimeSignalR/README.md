# Kata 09_06 — Echtzeit mit SignalR

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1 Abend

## Ziel

"Der Client soll das sofort sehen" ist eine Standardanforderung. Die Kata behandelt die
drei Dinge, die dabei regelmaessig fehlen: Was passiert bei Verbindungsabbruch, was bei
zwei Serverinstanzen, und was ist mit den Nachrichten, die waehrend der Trennung
entstanden sind.

## Domaene: Kata-Tracker — Live-Board der Trainingsgruppe

Der Kata-Tracker aus Kata 09_01 bekommt ein Live-Board: Wer in einer Trainingsgruppe einen
Versuch erfasst (`POST /api/v1/katas/{id}/attempts`), erscheint sofort auf den Boards aller
anderen Mitglieder **derselben** Gruppe — mit Kata, Dauer und aktueller Serie. Jede
Trainingsgruppe ist eine SignalR-Gruppe; ein Nutzer sieht ausschliesslich die Boards der
Gruppen, in denen er Mitglied ist. Der Client darf eine Konsolenanwendung sein, die die
eintreffenden Ereignisse einfach untereinander schreibt — entscheidend ist nicht die
Oberflaeche, sondern dass das Board nach einem Verbindungsabbruch wieder **vollstaendig**
stimmt.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Ein Server und ein Client, der etwas anzeigt,
genuegen — der Client darf eine Konsolenanwendung sein.
**Empfohlen, nicht erforderlich:** Kata 13_01 oder 13_03 (ein echtes Frontend als Gegenstelle),
Kata 11_06 (Redis fuer die Backplane in Punkt 5).

## Minimalpfad

Punkte 1, 3, 4 und 5.

## Aufgaben

1. Hub mit **stark typisiertem Client** (`Hub<TClient>`), keine magischen Methodennamen als
   String. Ein Ereignis (z. B. "Attempt erfasst") wird an interessierte Clients gesendet.
2. Ziele bewusst waehlen: `All`, `Group`, `User`, `Caller`. Baue Gruppen, die einer
   fachlichen Einheit entsprechen, und trag Clients bei Verbindungsaufbau ein — inklusive
   der Frage, wo diese Zuordnung nach einem Reconnect herkommt.
3. **Verbindungsabbruch ist der Normalfall, nicht der Fehlerfall.** Automatischer Reconnect
   mit Backoff, sichtbarer Verbindungszustand im Client, und die entscheidende Frage:
   Nachrichten, die waehrend der Trennung entstanden sind, sind **weg**. Loese es, indem
   der Client nach dem Reconnect den Zustand nachlaedt (Sequenznummer oder Zeitstempel).
   Ohne diesen Punkt ist jede Echtzeitanwendung subtil kaputt.
4. **Autorisierung am Hub**: `[Authorize]`, Claims in `Context.User`, und die Pruefung
   "darf dieser Nutzer diese Gruppe abonnieren?". Ein Hub ohne Autorisierung ist eine
   offene Datenleitung.
5. **Zwei Instanzen**: starte den Server doppelt, verbinde je einen Client an jede Instanz
   und zeig, dass eine Nachricht **nicht** ankommt. Dann Backplane (Redis) einziehen und
   zeigen, dass sie ankommt. Erklaere, was Sticky Sessions damit zu tun haben und warum
   WebSockets es leichter machen als Long Polling.
6. Transport: WebSockets, Server-Sent Events, Long Polling — erzwinge jeden einmal und
   vergleiche. Wann fallen Clients zurueck, und was kostet das?
7. Rueckwaertsweg: ein Aufruf **vom** Client an den Hub, mit Validierung und
   `CancellationToken`. Auch das ist eine API-Oberflaeche — behandle sie so.
8. Last: 500 gleichzeitige Verbindungen simulieren. Miss Speicher pro Verbindung und die
   Zeit fuer einen Broadcast. Daraus folgt, ob "an alle senden" tragfaehig ist oder ob du
   Gruppen brauchst.
9. Testbarkeit: Hub-Logik hinter einer Abstraktion, sodass sie ohne Netzwerk testbar ist;
   plus ein Integrationstest mit echtem `HubConnection` gegen
   `WebApplicationFactory`.
10. Abgrenzung schriftlich: SignalR vs. Polling vs. SSE vs. Message Broker mit
    Client-Push. Fuer welche Anforderung nimmst du was?

## Beispiele und Testfaelle

- **Zwei Clients, dieselbe Aenderung:** Client A und Client B sind mit dem Board der Gruppe
  "Dojo Montag" verbunden. A erfasst einen Versuch fuer "Bowling" mit 24 Minuten — B erhaelt
  das Ereignis `AttemptRecorded` mit Kata-Id, Dauer und Sequenznummer, ohne selbst zu fragen.
- **Gruppen-Broadcast erreicht nur die Gruppe:** Client C ist nur in "Dojo Freitag". Beim
  Versuch aus dem Fall oben bekommt C **kein** Ereignis. Der Test wartet dafuer eine
  definierte Zeit und erwartet null empfangene Nachrichten — nicht "es sah gut aus".
- **Reconnect verliert keine Nachricht:** B wird die Verbindung getrennt (Hub-Kontext
  abbrechen oder Server neu starten). Waehrend der Trennung erfasst A zwei weitere Versuche.
  Nach dem automatischen Reconnect fragt B ab der letzten bekannten Sequenznummer nach und
  hat danach **genau** dieselbe Versuchsliste wie A — keine Luecke, kein Duplikat.
- **Reconnect stellt die Gruppenzuordnung wieder her:** Nach dem Reconnect hat B eine neue
  `ConnectionId`. Ein anschliessender Broadcast an "Dojo Montag" erreicht B trotzdem — die
  Zuordnung kommt aus den Claims bzw. der Mitgliedschaft, nicht aus dem Speicher der alten
  Verbindung.
- **Nicht autorisierter Client kommt nicht in den Hub:** Ein `HubConnection` ohne Token
  scheitert bereits beim Verbindungsaufbau (401), nicht erst beim ersten Aufruf.
- **Fremde Gruppe abonnieren schlaegt fehl:** Ein authentifizierter Nutzer ruft
  `SubscribeBoard("Dojo Freitag")` auf, ohne Mitglied zu sein — der Hub lehnt ab
  (`HubException`), und der Nutzer erhaelt danach keinerlei Ereignisse dieser Gruppe.
- **Ungueltiger Aufruf vom Client:** `RecordAttempt` mit Dauer 0 oder unbekannter Kata-Id
  wird am Hub abgewiesen; es entsteht kein Broadcast, und kein anderer Client sieht etwas.
- **Zwei Instanzen ohne und mit Backplane:** Client A an Instanz 1, Client B an Instanz 2,
  beide in "Dojo Montag". Ohne Backplane empfaengt B **nichts**; mit Redis-Backplane
  empfaengt B dieselbe Nachricht. Beide Faelle sind derselbe Test, nur mit anderer
  Serverkonfiguration.

## Nachweise

Der Zwei-Instanzen-Versuch aus Punkt 5 mit beiden Ergebnissen, und ein Test, der beweist,
dass ein Client nach einem Reconnect **keine** Zustandsluecke hat.

## Skills

SignalR, Hubs, Gruppen, Reconnect-Strategien, Zustandsabgleich nach Trennung,
Hub-Autorisierung, Redis-Backplane, Transportarten, Lastverhalten

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
