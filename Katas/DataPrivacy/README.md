# Kata 12_02 — Personenbezogene Daten und Loeschbarkeit

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1 Abend

## Ziel

In deutschen Projekten ist das keine Kuer, sondern Abnahmekriterium. Und es ist eine
**Architekturfrage**, nicht eine Rechtsfrage: ein System, in dem personenbezogene Daten
ueberall verstreut liegen, kann Auskunft und Loeschung nicht erfuellen — egal was in der
Datenschutzerklaerung steht.

## Domaene: personenbezogene Daten im Kata-Tracker

Derselbe Kata-Tracker wie ab Kata 09_01, jetzt mit dem Teil, den er bisher verschwiegen hat:
den Menschen dahinter. Neu ist die Entitaet `Trainee` — `TraineeId`, `DisplayName`, `Email`,
`PhoneNumber` und `JoinedOn`. Jeder `Attempt` gehoert ab jetzt zu genau einem `Trainee`,
jeder `TrainingPlan` ebenfalls. Personenbezogen sind `DisplayName`, `Email` und
`PhoneNumber`; `PhoneNumber` ist das besonders sensible Feld aus Aufgabe 8.

Drei Trainierende sind fest verabredet, damit die Faelle unten benennbar bleiben:

- `anna` — Anna Vogt, `anna.vogt@contoso.example`, 40 Versuche ueber zwei Jahre.
- `ben` — Ben Krause, `ben.krause@contoso.example`, 12 Versuche, stellt das Loeschersuchen.
- `clara` — Clara Reis, `clara.reis@contoso.example`, seit 2023 inaktiv und damit der Fall
  fuer die Aufbewahrungsfrist.

Nicht personenbezogen und deshalb erhaltungspflichtig sind die Aggregate: Anzahl der
Versuche pro Kata, Summe der geuebten Zeit und die Streak-Statistik aus Kata 09_01 — sie
duerfen ein Loeschersuchen ueberleben, nur nicht mehr auf eine Person zeigen. Die Fundorte
fuer Punkt 1 sind im Tracker konkret: die Tabellen `Trainees`, `Attempts`, `TrainingPlans`,
die Serilog-Logs, die `AttemptRecorded`-Nachrichten aus Kata 11_01, die Redis-Schluessel aus
Kata 11_06, der monatliche CSV-Export der Trainings-Historie und dessen Backup.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine Entitaet mit einem Namen, einer
E-Mail-Adresse und ein paar abhaengigen Datensaetzen genuegt.
**Empfohlen, nicht erforderlich:** Kata 11_02 (Logs und Traces als Fundort), Kata 11_06 (Cache
als Fundort), Kata 11_01/11_04 (Nachrichten als Fundort).

## Minimalpfad

Punkte 1, 2, 4 und 5.

## Aufgaben

1. **Inventar zuerst.** Geh das System durch und schreib auf, wo personenbezogene Daten
   liegen — nicht nur in Tabellen: Logs, Traces, Metrik-Dimensionen, Message-Payloads,
   Caches, Backups, Exporte, Fehlermeldungen, Analytics. Diese Liste ist meist doppelt so
   lang wie erwartet und ist das eigentliche Ergebnis von Punkt 1.
2. **Kennzeichnung im Code**, damit das Inventar nicht veraltet: ein Attribut
   (`[PersonalData]`) an den betroffenen Eigenschaften, plus ein Test, der es auswertet und
   das Inventar **generiert**. Handpflege stirbt, generierte Listen nicht.
3. **Loeschen vs. Anonymisieren**: baue beides und begruende pro Datensatz, was richtig ist.
   Der Kern: eine Bestellung, die zu einem geloeschten Nutzer gehoert, muss buchhalterisch
   erhalten bleiben — also anonymisieren statt loeschen. Zeig, dass die Statistik danach
   noch stimmt.
4. **Loeschauftrag als Ablauf, nicht als `DELETE`**: der Auftrag geht durch alle Fundorte aus
   Punkt 1 — Datenbank, Cache invalidieren, Logs (die man nicht rueckwirkend aendern kann,
   also von vornherein nicht mitschreiben), Message-Payloads (dasselbe Problem), Suchindex,
   Exportdateien. Was **strukturell** nicht loeschbar ist, gehoert von Anfang an nicht
   hinein — das ist die Lehre der Kata.
5. **Log-Scrubbing**: ein Serilog-`Destructuring`-Policy oder Enricher, der markierte
   Eigenschaften maskiert. Plus ein Test, der eine Exception mit personenbezogenen Daten
   im Payload wirft und beweist, dass im Log nichts davon steht. Ungeschuetzte
   Fehlermeldungen sind der haeufigste Leckweg.
6. **Auskunftsersuchen**: Export aller Daten einer Person in einem maschinenlesbaren Format.
   Wenn du dafuer mehr als eine Abfrage pro Fundort brauchst, war das Modell zu verstreut.
7. **Aufbewahrungsfristen** als Code: ein Job (Kata 11_07), der Daten nach Ablauf entfernt oder
   anonymisiert — mit Protokoll, was wann entfernt wurde. Loeschen ohne Nachweis zaehlt
   nicht.
8. **Verschluesselung**: ein besonders sensibles Feld verschluesselt ablegen (Always
   Encrypted oder `ValueConverter` mit Schluessel aus der Konfiguration). Danach die
   unangenehme Frage: wie durchsuchst du ein verschluesseltes Feld, und was kostet dich das?
9. **Pseudonymisierung fuer Testdaten**: aus einem Produktionsabzug einen brauchbaren
   Testdatensatz erzeugen, bei dem Verteilungen und Beziehungen erhalten bleiben, die
   Personen aber nicht. Das ist die Alternative zu "wir testen auf der Kopie von
   Produktion".

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test gegen den Tracker — Ersuchen hinein, Fundorte
danach geprueft.

1. **Auskunftsersuchen liefert genau eine Person.** Auskunft fuer `ben` -> ein Dokument mit
   Stammdaten (`DisplayName`, `Email`, `PhoneNumber`, `JoinedOn`), seinen 12 Versuchen und
   seinen Trainingsplaenen, in einem festgelegten maschinenlesbaren Format. Der Test prueft
   beides: dass jedes Feld aus dem generierten Inventar enthalten ist **und** dass kein
   Datum von `anna` oder `clara` darin vorkommt.
2. **Loeschersuchen anonymisiert und laesst Aggregate stehen.** Vor dem Lauf hat die Kata
   "Bowling" 30 Versuche und 14 h geuebte Zeit. Loeschersuchen fuer `ben` -> danach ist
   `Trainees` fuer `ben` leer oder auf einen Platzhalter gesetzt, seine 12 Versuche existieren
   weiter mit `TraineeId = null` bzw. einem Pseudonym, und Anzahl und Zeitsumme pro Kata sind
   **unveraendert** 30 und 14 h. Ein zweites Auskunftsersuchen fuer `ben` liefert danach
   keinen Treffer und keine Exception.
3. **Kein Log-Eintrag enthaelt eine E-Mail-Adresse.** Ein Anwendungsfall wirft eine Exception,
   deren Payload den vollstaendigen `Trainee` traegt. Der Test liest die geschriebenen
   Log-Ereignisse (In-Memory-Sink) und schlaegt fehl, sobald irgendein Feld
   `anna.vogt@contoso.example`, den Klarnamen oder die Telefonnummer enthaelt; erlaubt ist
   nur die maskierte Form (`a***@contoso.example`) und die `TraineeId`. Dasselbe gilt fuer
   die `AttemptRecorded`-Nachricht und den Redis-Schluessel.
4. **Verschluesseltes Feld ist in der Datenbank nicht lesbar.** Nach dem Anlegen von `anna`
   liest der Test `PhoneNumber` mit rohem SQL an EF Core vorbei: das Ergebnis ist kein
   Klartext. Ueber den `DbContext` gelesen kommt dieselbe Nummer wieder korrekt heraus. Und
   eine Suche `WHERE PhoneNumber LIKE '%1234%'` findet nichts — das ist der Preis aus
   Aufgabe 8 und gehoert als Test festgehalten, nicht als Randnotiz.
5. **Abgelaufene Aufbewahrungsfrist entfernt den Datensatz von allein.** Frist: 24 Monate
   ohne Versuch. `clara` ist seit 2023 inaktiv, `anna` nicht. Der Job aus Kata 11_07 laeuft mit
   fester Testzeit -> `clara` ist entfernt bzw. anonymisiert, `anna` unberuehrt, und im
   Protokoll steht ein Eintrag mit Zeitpunkt, Entitaet, `TraineeId` und Anzahl betroffener
   Zeilen. Zweiter Lauf am selben Tag: keine weitere Aenderung, kein zweiter Protokolleintrag.
6. **Export und Backup enthalten keine unmaskierten Klardaten.** Der monatliche CSV-Export
   und der daraus gezogene Backup-Abzug werden Zeile fuer Zeile geprueft: keine Spalte
   enthaelt eine E-Mail-Adresse oder Telefonnummer im Klartext. Ein Export, der vor einem
   Loeschersuchen entstanden ist, wird beim Loeschlauf mit erfasst — der Test sucht `ben` in
   **allen** vorhandenen Exportdateien und findet ihn nicht.
7. **Das Inventar bleibt aktuell.** Der Test aus Aufgabe 2 generiert die Fundortliste aus den
   `[PersonalData]`-Attributen. Wird dem Modell ein Feld `EmergencyContact` ohne Attribut
   hinzugefuegt, schlaegt der Test fehl (Heuristik ueber Feldnamen bzw. abgelegte
   Erwartungsliste) — ein nicht klassifiziertes Feld ist ein roter Test, keine offene
   Aufgabe im Ticketsystem.
8. **Pseudonymisierter Testdatensatz bleibt brauchbar.** Aus dem Abzug mit `anna`, `ben` und
   `clara` entsteht ein Testdatensatz mit derselben Anzahl Trainierender, derselben Anzahl
   Versuche pro Person und denselben Zeitraeumen — aber keiner echten E-Mail-Adresse und
   keinem echten Namen. Zwei Laeufe mit demselben Seed ergeben denselben Datensatz, damit
   Tests darauf reproduzierbar sind.

## Nachweise

Das generierte Inventar aus Punkt 2, ein gruener Log-Scrubbing-Test, und ein Loeschlauf,
nach dem du in **jedem** Fundort aus Punkt 1 nachweisen kannst, dass nichts uebrig ist.

## Skills

Datenklassifizierung, Anonymisierung vs. Loeschung, Log-Scrubbing, Auskunft und
Loeschbarkeit als Architektur, Aufbewahrungsfristen, Feldverschluesselung,
Testdaten-Pseudonymisierung

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
