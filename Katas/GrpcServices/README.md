# Kata 09_05 — gRPC fuer Dienst-zu-Dienst-Kommunikation

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1 Abend

## Ziel

REST nach aussen, gRPC nach innen — das ist in .NET-Landschaften die uebliche Aufteilung.
Der Wert dieser Kata liegt im Vergleich: derselbe Anwendungsfall zweimal, und danach ein
begruendetes "wann was".

## Domaene: Trainingsplan-Dienst

Der Kata-Tracker aus Kata 09_01 wird aufgeteilt: `Tracker.Api` bleibt der REST-Dienst nach
aussen, die Auswertung wandert in einen internen Dienst `Tracker.Stats`, den nur die API
aufruft — per gRPC. Der Vertrag `stats.proto` beschreibt einen Service `KataStats` mit vier
Aufrufarten, je eine pro Punkt 2:

- `GetStreak(StreakRequest) returns (StreakReply)` — unary: laengste und aktuelle Serie an
  Trainingstagen fuer eine Person.
- `StreamAttempts(AttemptFilter) returns (stream Attempt)` — Server Streaming: alle Versuche
  ab einem Datum, potenziell zehntausende, deshalb kein `repeated` in einer Antwort.
- `ImportAttempts(stream Attempt) returns (ImportSummary)` — Client Streaming: Altbestand aus
  einer CSV-Datei einspielen.
- `Coach(stream ProgressUpdate) returns (stream Recommendation)` — Bidirectional: waehrend du
  uebst, kommen Vorschlaege fuer die naechste Kata zurueck.

Derselbe Anwendungsfall — `GET /api/v1/stats/streak` — existiert damit zweimal: als REST-Route
nach aussen und als gRPC-Aufruf nach innen. Genau dieses Paar wird in Punkt 7 gemessen.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Zwei kleine Dienste, die miteinander reden,
genuegen.
**Empfohlen, nicht erforderlich:** Kata 09_01 (die REST-Variante als Vergleichsobjekt),
Kata 11_02 (Tracing ueber die Dienstgrenze).

## Minimalpfad

Punkte 1, 2, 4 und 7.

## Aufgaben

1. **Contract first**: `.proto` schreiben, nicht generieren lassen. Server und Client aus
   derselben Datei erzeugen (`Grpc.AspNetCore`, `Grpc.Net.ClientFactory`). Der Vertrag ist
   das Artefakt — behandle ihn wie eine oeffentliche API.
2. Alle vier Aufrufarten je einmal, mit einem Beispiel, das sie rechtfertigt:
   Unary, Server Streaming, Client Streaming, Bidirectional Streaming.
   Beim Server Streaming: `IAsyncEnumerable` und ein `CancellationToken`, der wirklich
   abbricht.
3. **Schema-Evolution** ohne Downtime — das ist die entscheidende Frage zu Protobuf:
   Feld hinzufuegen, Feld umbenennen, Feld entfernen. Zeig mit einem alten Client gegen
   einen neuen Server (und umgekehrt), was funktioniert und was bricht. Regeln
   aufschreiben: Feldnummern nie wiederverwenden, `reserved` nutzen, keine Typwechsel.
4. **Fehlerbehandlung**: `StatusCode` statt HTTP-Statuscodes, Fehlerdetails ueber Trailer
   oder `google.rpc.Status`. Bilde die Ergebnisse aus deinem `Result<T>` (Kata 07_02) sauber
   darauf ab. `DeadlineExceeded` gehoert dazu: setz **jeder** Anfrage eine Deadline und
   zeig, wie sie sich ueber Aufrufketten hinweg vererbt.
5. Interceptors fuer Logging, Auth und Metriken — das Gegenstueck zu Middleware, einmal
   server- und einmal clientseitig.
6. `Grpc.Net.ClientFactory` mit wiederverwendetem Kanal: zeig den Unterschied zwischen
   einem Kanal pro Aufruf und einem geteilten Kanal (HTTP/2-Multiplexing) an der
   Verbindungszahl.
7. **Messvergleich zur REST-Variante:** dieselben 10.000 Aufrufe ueber JSON/REST und ueber
   gRPC. Miss Nutzlastgroesse, Latenz und Allokationen. Notier auch, wo gRPC *verliert* —
   Browser-Unterstuetzung, Lesbarkeit im Log, Debugbarkeit mit `curl`.
8. `grpc-health-probe`-kompatible Health Checks und Reflection fuer Werkzeuge
   (`grpcurl`), Reflection im Produktivbetrieb bewusst an oder aus — mit Begruendung.
9. Optional: **gRPC-Web** oder JSON-Transcoding, damit derselbe Dienst auch aus einem
   Browser erreichbar ist. Erklaere, was dabei verloren geht.

## Beispiele und Testfaelle

- **Unary mit Ergebnis:** Versuche an den Tagen 2026-03-01 bis 2026-03-05 sowie 2026-03-07.
  `GetStreak({trainee_id: "lasse"})` liefert `{longest_days: 5, current_days: 1,
  last_attempt: "2026-03-07"}`.
- **Fehler als `StatusCode`, nicht als Exception:** `GetStreak({trainee_id: ""})` beantwortet
  der Server mit `INVALID_ARGUMENT`, ein unbekanntes `trainee_id` mit `NOT_FOUND` — beides aus
  demselben `Result<T>` (Kata 07_02) abgebildet. Der Test prueft die `RpcException.StatusCode`
  auf der Clientseite; im Serverlog steht dabei **kein** unbehandelter Fehler.
- **Serverstream bricht bei Deadline ab:** `StreamAttempts` liefert 500 Versuche mit je 10 ms
  Verzoegerung, der Client setzt `deadline: 200ms`. Erwartet: der Client hat weniger als 500
  Elemente empfangen und die Enumeration endet in einer `RpcException` mit
  `DEADLINE_EXCEEDED`; der serverseitige `CancellationToken` ist ausgeloest (Test prueft ein
  Flag, das der Serverhandler im `finally` setzt).
- **Deadline vererbt sich ueber die Kette:** `Tracker.Api` ruft mit 300 ms Deadline
  `GetStreak` auf, `Tracker.Stats` ruft daraufhin einen dritten Dienst. Im innersten Handler
  ist `context.Deadline` gesetzt und die Restzeit kleiner als 300 ms; bricht der aeussere
  Aufruf ab, endet auch der innerste mit `DEADLINE_EXCEEDED` statt weiterzulaufen.
- **Client Streaming mit Teilfehlern:** `ImportAttempts` mit 3 gueltigen Versuchen und einem
  mit `duration_ms: 0` endet mit Status `OK` und `ImportSummary{accepted: 3, rejected: 1}` —
  fachliche Teilfehler stehen in der Nutzlast, nicht im Statuscode.
- **Rueckwaertskompatibilitaet, Feld hinzugefuegt:** Client, der aus dem alten `stats.proto`
  ohne `Attempt.tag` (Feldnummer 5) generiert wurde, gegen den neuen Server: der Aufruf
  gelingt, das unbekannte Feld wird ignoriert. Umgekehrt liest der neue Client vom alten
  Server `tag == ""` — Standardwert, nicht Fehler. Beide Richtungen als Test, mit beiden
  generierten Clients im Testprojekt.
- **Rueckwaertskompatibilitaet, Feld entfernt und Nummer wiederverwendet:** `duration_ms`
  (Nummer 3, `int32`) entfernen und Nummer 3 fuer ein `string`-Feld erneut verwenden. Der Test
  zeigt, dass ein alter Client Muell liest oder das Parsen scheitert — und dass die Variante
  mit `reserved 3;` plus neuer Feldnummer 6 stattdessen sauber laeuft.
- **Kanal geteilt oder pro Aufruf:** 10.000 `GetStreak`-Aufrufe. Mit `Grpc.Net.ClientFactory`
  und geteiltem Kanal bleibt es bei einer TCP-Verbindung, mit einem Kanal pro Aufruf steigt
  die Verbindungszahl mit den Aufrufen. Dieselben 10.000 Aufrufe gegen
  `GET /api/v1/stats/streak` liefern die Vergleichszeile fuer die Messtabelle in Punkt 7
  (Nutzlastgroesse in Bytes, Latenz p50/p99, allozierte Bytes).

## Nachweise

Die Messtabelle aus Punkt 7 und die Evolutionsregeln aus Punkt 3. Plus ein Satz pro
Anwendungsfall in deinem System: REST oder gRPC — und warum.

## Skills

gRPC, Protobuf, Contract-first, Streaming-Arten, Schema-Evolution, Deadlines,
Interceptors, HTTP/2-Multiplexing, gRPC-Web

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
