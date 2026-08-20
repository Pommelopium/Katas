# Kata 11_04 — Event-Streaming mit Kafka

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende · baut auf Kata 11_01 auf

## Ziel

Kafka ist kein "RabbitMQ mit anderem Namen". Der Unterschied — Log statt Queue, Offsets
statt Acks, Partitionen statt Consumer-Wettlauf — ist genau das, worauf es beim Entwurf
ankommt.

## Domaene: Kata-Tracker

Dieselbe Codebase wie ab Kata 09_01. Der Kata-Tracker nimmt Versuche auf, die Statistik aus
Kata 11_01 hoert auf `AttemptRecorded`. Bisher lief die Nachricht ueber eine Queue: gelesen
ist gelesen, weg ist weg. Jetzt liegt derselbe Ereignisstrom in einem Kafka-Topic `attempts`
— und damit aendert sich, was du damit tun kannst.

Die Statistik wird zum **Stream-Consumer**: sie liest den Strom als Log und schreibt daraus
ihre Sicht fort — laengste Serie an Uebungstagen, Anzahl der Versuche und geuebte Zeit pro
Kata. Weil das Log bleibt, ist diese Sicht keine Wahrheit mehr, sondern eine **Projektion**:
loeschbar und aus dem Topic jederzeit neu berechenbar. Genau daran haengen die interessanten
Fragen dieser Kata. Reihenfolge gilt nur je Kata — deshalb ist `KataId` der Key. Zwei
Instanzen der Statistik teilen sich die Arbeit ueber Partitionen, nicht ueber einen Wettlauf
um Nachrichten. Und wenn die Streak-Berechnung falsch war, wirfst du die Projektion weg und
baust sie aus dem Log erneut auf, statt die Historie zu korrigieren.

## Aufgabe

Publiziere die `AttemptRecorded`-Events aus Kata 11_01 zusaetzlich nach Kafka und baue einen
Consumer, der daraus die Statistik-Projektion des Kata-Trackers fortschreibt — mit `KataId`
als Key, eigenem Offset-Management und der Faehigkeit, dieselbe Projektion aus dem Log
reproduzierbar neu zu erzeugen.

## Aufgaben

1. Kafka via Docker Compose (KRaft-Modus, kein ZooKeeper). Topic `attempts` mit
   **3 Partitionen** anlegen.
2. Producer mit `Confluent.Kafka`: Key = `KataId`, damit alle Events einer Kata in
   **derselben** Partition landen. Erklaere schriftlich, warum das die Reihenfolge
   garantiert — und warum nur *innerhalb* einer Partition.
3. `Acks.All` plus `EnableIdempotence` setzen. Schreib auf, was du damit gegen
   Duplikate bei Producer-Retries gewinnst.
4. Consumer als `BackgroundService` in einer Consumer Group. **`EnableAutoCommit = false`** —
   committe den Offset erst, wenn die Verarbeitung erfolgreich war.
5. **Rebalancing sichtbar machen:** starte eine zweite Instanz und zeig im Log, wie die
   Partitionen neu verteilt werden. Starte eine dritte, dann eine vierte. Erklaere,
   warum die vierte Instanz leer bleibt.
6. Poison Message: eine Nachricht, die immer fehlschlaegt. Sie darf den Consumer **nicht**
   blockieren — Dead-Letter-Topic `attempts.dlq` mit Original-Header und Fehlergrund.
7. Consumer Lag messen und als Metrik aus Kata 11_02 exportieren. Das ist die Zahl, auf die
   im Betrieb alarmiert wird.
8. Replay: setze die Offsets der Consumer Group zurueck und lass die Statistik neu
   aufbauen. Das Ergebnis muss **identisch** sein — der Consumer ist damit nachweislich
   deterministisch.

## Nachweise

- Schreib eine halbe Seite **Kafka vs. RabbitMQ**: Log-Retention vs. Queue, Ordering-Garantien,
  Consumer Group vs. Competing Consumer, Replay-Faehigkeit, Fan-out. Wann was?
- Zeig, dass ein Consumer-Crash **vor** dem Offset-Commit zur erneuten Zustellung fuehrt,
  und dass deine Verarbeitung das aushaelt (Idempotenz aus Kata 11_01).

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test gegen einen echten Broker (Testcontainers oder
das Compose-Setup aus Aufgabe 1). Feste Kata-Ids `K1` = Bowling, `K2` = FizzBuzz.

1. **Gleicher Key, eine Partition, Reihenfolge erhalten.** Publiziere fuenf
   `AttemptRecorded` fuer `K1` (Daten 03-01 bis 03-05) und dazwischen beliebig viele fuer
   `K2`. Ergebnis: alle `K1`-Nachrichten liegen in **derselben** Partition, und ihre Offsets
   steigen in Publikationsreihenfolge. Der Consumer sieht sie in genau dieser Reihenfolge.
   Ueber Partitionsgrenzen hinweg wird **nichts** zugesichert: eine `K2`-Nachricht darf vor
   oder nach den `K1`-Nachrichten auftauchen — der Test darf das nicht pruefen.
2. **Neustart nimmt beim letzten Commit-Offset wieder auf.** Sechs Nachrichten fuer `K1`,
   der Consumer committet nach jeder erfolgreichen Verarbeitung. Toete ihn nach der vierten,
   starte neu. Ergebnis: er beginnt bei Offset 4, verarbeitet 5 und 6, und die Projektion
   entspricht danach exakt der eines ununterbrochenen Durchlaufs. Kein Ereignis fehlt.
3. **Crash vor dem Commit: erneute Zustellung, aber keine doppelte Wirkung.** Toete den
   Consumer **nach** der Verarbeitung der vierten Nachricht und **vor** deren Commit.
   Ergebnis: Nachricht 4 wird erneut zugestellt (das ist at-least-once und in Ordnung), aber
   die Projektion zaehlt den Versuch weiterhin einmal — Anzahl und Zeitsumme sind identisch
   zum Lauf ohne Crash. Nach aussen ist keine Doppelverarbeitung sichtbar.
4. **Rebalance beim zweiten Consumer.** Ein Consumer in der Gruppe haelt alle 3
   Partitionen. Starte einen zweiten: die Zuteilung wird 2/1 oder 1/2, in Summe **genau 3**
   und ohne Ueberschneidung. Der dritte Consumer bekommt eine Partition, der vierte bleibt
   leer. Ergebnis fachlich: waehrend und nach dem Rebalance ist die Projektion ueber die
   gesamte Event-Folge dieselbe wie mit einem einzigen Consumer.
5. **Poison Message blockiert nicht.** Folge: zwei gueltige `K1`-Nachrichten, eine
   unlesbare, zwei weitere gueltige. Ergebnis: die vier gueltigen sind verarbeitet, die
   unlesbare liegt in `attempts.dlq` mit Original-Headern und Fehlergrund, und der
   Consumer-Lag der Partition geht auf 0 zurueck — er steht nicht an Offset 2 fest.
6. **Schema-Aenderung bleibt kompatibel.** Erweitere `AttemptRecorded` um ein **optionales**
   Feld (etwa `Notes`). Ergebnis: der alte Consumer verarbeitet neue Nachrichten weiter und
   ignoriert das Feld; der neue Consumer verarbeitet alte Nachrichten und sieht dort den
   Standardwert. Beide Richtungen als Test, mit denselben Bytes aus dem Topic. Ein
   **Pflicht**feld ohne Standardwert bricht diesen Test — das ist der lehrreiche Teil.
7. **Fenster-Aggregat mit festem Ergebnis.** Fuer `K1` die Versuche 2026-03-01 `00:30:00`,
   2026-03-02 `00:20:00`, 2026-03-04 `00:15:00`, zweiter Versuch am 2026-03-04 `00:05:00`.
   Ergebnis der Projektion: Anzahl `4`, Zeitsumme `01:10:00`, laengste Serie an
   Uebungstagen `2` (01/02, die Luecke am 03 bricht sie; zwei Versuche am selben Tag zaehlen
   als ein Tag). Rechnest du je Kalendertag, sind die Tagessummen `00:30`, `00:20`, `00:20`.
8. **Replay ist deterministisch.** Setze die Offsets der Consumer Group auf den Anfang
   zurueck, verwirf die Projektion und lass sie neu aufbauen. Ergebnis: **byteweise
   dieselben** Aggregate wie in Fall 7 — auch dann, wenn du den Replay mit zwei Consumern
   statt einem fahren laesst.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Ein Ereignis mit einer fachlichen Kennung als
Key genuegt; du kannst es selbst erzeugen statt aus der Outbox zu lesen.
**Empfohlen, nicht erforderlich:** Kata 11_01 (Outbox als Quelle und Idempotenz auf der
Consumer-Seite), Kata 11_02 (Metriken fuer den Consumer Lag).
**Werkzeuge:** Docker Desktop.

## Skills

Kafka, `Confluent.Kafka`, Partitionierung, Consumer Groups, Offset-Management,
Rebalancing, Consumer Lag, Replay

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
