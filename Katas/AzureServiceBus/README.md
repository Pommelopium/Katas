# Kata 11_05 — Azure Service Bus

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende · baut auf Kata 11_01 auf

## Ziel

Der Standard-Broker im Azure-Umfeld und damit der, dem du in .NET-Systemen am haeufigsten
begegnest. Interessant sind nicht die drei Zeilen SDK-Code, sondern die Betriebsfeatures, die RabbitMQ und Kafka so
nicht haben: Sessions, Scheduled Messages, Duplicate Detection, die eingebaute
Dead-Letter-Queue.

## Domaene: Kata-Tracker

Dieselbe Codebase wie ab Kata 09_01. Der Tracker nimmt ueber
`POST /api/v1/katas/{id}/attempts` einen geloesten Versuch auf und schiebt `AttemptRecorded`
durch die Outbox aus Kata 11_01 — nur laeuft der Transport ab jetzt ueber Service Bus. Auf
der anderen Seite haengen drei Interessenten an derselben Fachlichkeit:

- **Statistik** liest die Queue `attempts` und fuehrt Streak, Versuchszahl und geuebte Zeit
  pro Kata fort. Diese Sicht ist reihenfolgeabhaengig: kommen zwei Versuche derselben Kata
  verdreht an, faellt die Streak falsch aus. Darum wird `SessionId = KataId` gesetzt — die
  Kata ist der Fachschluessel, nicht der Nutzer und nicht die Nachricht.
- **Erinnerungsdienst** plant beim Erfassen eines Versuchs eine Nachricht
  `PracticeReminderDue` sieben Tage in die Zukunft ("Bowling seit 7 Tagen nicht geuebt") und
  storniert sie wieder, sobald ein neuer Versuch fuer dieselbe Kata eintrifft.
- **Auswertung** haengt am Topic `attempt-events` mit zwei Subscriptions:
  `long-sessions` (SQL-Filter `durationMinutes > 45`) und `all-attempts` (ohne Filter). Ein
  langer Versuch erscheint in beiden, ein kurzer nur in einer.

## Aufgabe

Tausche den Broker im Outbox-Publisher aus Kata 11_01 gegen Azure Service Bus — **ohne** die
Outbox-Logik anzufassen. Betroffen sein darf genau die Implementierung hinter dem
Publisher-Interface aus Kata 11_01; `OutboxMessage`, der Poller und die Fachhandler bleiben
unveraendert. Wenn du dafuer mehr als das Adapter-Interface aendern musst, war
die Abstraktion in Kata 11_01 zu duenn. Notier das.

## Aufgaben

1. Lokal gegen den **Service Bus Emulator** im Container arbeiten (kein Azure-Abo noetig).
   Topologie als Code oder als Emulator-Config: Queue `attempts`,
   Topic `attempt-events` mit zwei Subscriptions und je einem SQL-Filter.
2. `ServiceBusClient` / `ServiceBusProcessor` als `BackgroundService`, mit
   `MaxConcurrentCalls` und `PrefetchCount` bewusst gesetzt und begruendet.
3. **PeekLock statt ReceiveAndDelete.** Zeig, was bei einem Crash zwischen Verarbeitung
   und `CompleteMessageAsync` passiert. Erklaere `MaxAutoLockRenewalDuration` und was
   passiert, wenn die Verarbeitung laenger dauert als der Lock haelt.
4. `DeliveryCount`, `MaxDeliveryCount` und die **eingebaute** Dead-Letter-Queue: eine
   Poison Message muss dort landen, mit `DeadLetterReason`. Schreib einen Handler, der
   die DLQ leert und Nachrichten gezielt zurueckspielt.
5. **Sessions**: alle Nachrichten einer `KataId` mit `SessionId` versehen und mit einem
   `ServiceBusSessionProcessor` streng in Reihenfolge verarbeiten. Vergleich zur
   Partitionierung aus Kata 11_04.
6. **Scheduled Messages**: eine Erinnerung "Kata seit 7 Tagen nicht geuebt" per
   `ScheduleMessageAsync` einplanen und wieder stornieren koennen.
7. **Duplicate Detection** auf der Queue aktivieren, `MessageId` sinnvoll setzen
   (die Outbox-`Id` aus Kata 11_01). Schreib auf, warum das trotzdem *kein* exactly-once ist.
8. Transportfehler und Drosselung: `ServiceBusException` auswerten,
   `Reason == ServiceBusFailureReason.ServiceBusy` mit Backoff behandeln.

## Nachweise

- Tabelle **RabbitMQ vs. Kafka vs. Service Bus**: eine Zeile pro Kriterium
  (Ordering, Retention, Fan-out, Scheduling, Dedup, Betriebsaufwand, Kosten).
- Der Wechsel des Brokers hat genau eine Klasse betroffen — oder du erklaerst, warum nicht.

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test gegen den Emulator: Nachricht hinein,
beobachtbares Ergebnis geprueft. Nicht "im Log stand das Richtige".

1. **Reihenfolge pro Fachschluessel.** Drei Versuche der Kata `Bowling`
   (`SessionId = "Bowling"`) werden in der Reihenfolge A, B, C gesendet. Der
   `ServiceBusSessionProcessor` verarbeitet sie als A, B, C — auch bei
   `MaxConcurrentSessions = 4`. Werden parallel Versuche der Kata `Roman Numerals`
   gesendet, laufen die beiden Sessions gleichzeitig, aber jede fuer sich in Reihenfolge.
   Ohne `SessionId` faellt derselbe Test unzuverlaessig aus — genau das ist der Nachweis.
2. **Poison Message landet in der DLQ.** Bei `MaxDeliveryCount = 3` wirft der Handler fuer
   eine praeparierte Nachricht immer. Nach dem dritten Versuch liegt sie in
   `attempts/$deadletterqueue`, mit gesetztem `DeadLetterReason` und
   `DeadLetterErrorDescription`; `DeliveryCount` ist 3. Die Hauptqueue ist leer, und die
   nachfolgenden Nachrichten wurden trotzdem verarbeitet — die Poison Message hat den
   Processor nicht blockiert.
3. **DLQ gezielt zurueckspielen.** Der DLQ-Handler liest die Nachricht aus Fall 2, sendet
   sie erneut in `attempts` und schliesst das DLQ-Original ab. Der Handler glueckt diesmal;
   danach ist die DLQ leer und die Statistik enthaelt den Versuch **einmal**.
4. **Abandon fuehrt zur Wiederauslieferung.** Der Handler ruft `AbandonMessageAsync` statt
   `CompleteMessageAsync`. Dieselbe Nachricht kommt sofort wieder, `DeliveryCount` ist um 1
   hoeher. Beim zweiten Mal wird abgeschlossen: die Statistik hat den Versuch einmal
   gezaehlt (Idempotenz aus Kata 11_01), die Queue ist leer.
5. **Crash im PeekLock-Fenster.** Der Prozess wird zwischen Verarbeitung und
   `CompleteMessageAsync` beendet. Nach Ablauf des Locks — bzw. nach Neustart des
   Processors — wird die Nachricht erneut zugestellt, sie ist nicht verloren. Mit
   `ReceiveAndDelete` derselbe Ablauf: die Nachricht ist weg. Beides als Test festhalten.
6. **Scheduled Message liefert erst spaeter aus.** Eine `PracticeReminderDue` mit
   `ScheduledEnqueueTime = jetzt + 20 s` ist unmittelbar danach nicht empfangbar
   (`ReceiveMessageAsync` mit kurzem Timeout liefert `null`), aber ueber `PeekMessagesAsync`
   sichtbar. Nach Ablauf der Zeit kommt sie an. Wird stattdessen
   `CancelScheduledMessageAsync` mit der `SequenceNumber` aufgerufen, kommt sie nie —
   auch nicht nach 20 Sekunden.
7. **Duplikaterkennung verwirft die zweite Nachricht.** Bei aktivierter Duplicate Detection
   (Fenster z. B. 10 Minuten) wird derselbe Versuch zweimal mit identischer
   `MessageId` (= Outbox-`Id`) gesendet. Der Consumer sieht ihn genau einmal, ohne dass die
   `ProcessedMessage`-Tabelle greifen musste. Mit **verschiedenen** `MessageId`s zu gleichem
   Inhalt kommen beide an — und erst die Consumer-Idempotenz rettet die Statistik. Das ist
   die Testfassung der Begruendung, warum Dedup kein exactly-once ist.
8. **Filter der Subscription greift.** Ein Versuch mit `durationMinutes = 60` erreicht
   `long-sessions` **und** `all-attempts`; einer mit `durationMinutes = 12` nur
   `all-attempts`. Auf `long-sessions` ist danach genau eine Nachricht angekommen.
9. **Drosselung wird nicht zum Datenverlust.** Der Publisher bekommt einen
   `ServiceBusException` mit `Reason == ServiceBusFailureReason.ServiceBusy` (simuliert am
   Adapter). Die Outbox-Nachricht bleibt unverarbeitet und wird nach Backoff erneut
   versucht; nach dem erfolgreichen Versuch existiert sie beim Consumer genau einmal.

## Voraussetzung

**Muss zuvor erledigt sein:** Kata 11_01 (Transactional Outbox) — der Kern dieser Kata ist der
Brokerwechsel an einem bestehenden Publisher. Ohne ihn faellt die Probe aufs Exempel weg,
ob die Abstraktion trug; dann bleibt eine reine SDK-Uebung.
**Empfohlen, nicht erforderlich:** Kata 11_04 (Kafka) fuer die Vergleichstabelle.
**Werkzeuge:** Docker Desktop (Service Bus Emulator, SQL Edge).

## Skills

Azure Service Bus, PeekLock, Dead-Letter-Queue, Sessions, Scheduled Messages,
Duplicate Detection, Broker-Abstraktion

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
