# Kata 10_02 — Ausfuehrungsplaene und Indizes

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende · baut auf Kata 09_02/10_01 auf

## Ziel

Datenbankoptimierung ist im Kern Produktionsdenken: eine langsame Query finden, im Plan
begruenden **warum** sie langsam ist, und die Verbesserung messen. Nicht raten, nicht
"mal einen Index drauf".

## Domaene: Kata-Tracker

Dasselbe Schema wie in Kata 09_02, nur mit realistischer Datenmenge: `Katas`
(`KataId`, `Titel`, `Level`, `IsDeleted`), `Attempts` (`AttemptId`, `KataId`, `SolvedOn`,
`Duration_Minutes`, `Duration_Seconds`) und `TrainingPlans` mit den zugeordneten Katas.

Die Abfragen, die du optimierst, sind die des Trainingsplans: "alle Versuche zu einer
Kata", "meine Versuche im Jahr 2026", "Titelsuche", "Statistik pro Kata" und der
Streak-Bericht. Ungleich verteilte Daten sind Absicht — ein paar Lieblingskatas mit
Hunderttausenden Versuchen, der lange Rest mit einer Handvoll. Genau diese Schieflage
macht Parameter Sniffing sichtbar.

## Vorbereitung

Erzeuge eine Testdatenbank mit **mindestens 2 Millionen** `Attempt`-Zeilen (Kata 10_04 liefert
das Werkzeug dafuer). Auf 100 Zeilen sieht jeder Plan gleich schnell aus — das ist der
Grund, warum die meisten Entwickler dieses Thema nie geuebt haben.

## Aufgaben

1. Miss zuerst, dann optimiere. `SET STATISTICS IO, TIME ON` als Grundlage: **Logical
   Reads** ist die Kennzahl, nicht die Wanduhr. Notier fuer jede Query den Ausgangswert.
2. Lies den Ausfuehrungsplan: finde Clustered Index Scan, Key Lookup, Sort mit Spill,
   Hash Match und den Warnhinweis zu fehlenden Indizes. Benenne pro Plan den **einen**
   teuersten Operator.
3. **Clustered vs. Nonclustered**: setz einen passenden nonclustered Index und zeig den
   Wechsel von Scan zu Seek. Danach `INCLUDE` fuer einen Covering Index — der Key Lookup
   muss aus dem Plan verschwinden.
4. **Spaltenreihenfolge im Index** ist keine Geschmacksfrage: bau `(A, B)` und `(B, A)` und
   zeig eine Query, die nur einen der beiden nutzen kann.
5. **SARGability**: schreib drei Queries, die einen vorhandenen Index *nicht* nutzen
   koennen — `YEAR(Datum) = 2026`, `LIKE '%text'`, implizite Konvertierung
   `nvarchar` gegen `varchar`. Formuliere jede so um, dass ein Seek moeglich wird.
6. **Parameter Sniffing** reproduzieren: eine Prozedur, die fuer einen haeufigen und einen
   seltenen Parameterwert grundverschiedene Plaene braucht. Zeig den falschen Plan, dann
   loese es (`OPTIMIZE FOR UNKNOWN`, `RECOMPILE`, lokale Variable) und begruende die Wahl.
7. **Filtered Index** und **Index auf berechnete, persistierte Spalte** je einmal
   sinnvoll einsetzen.
8. Die Kosten der Indizes: miss die Schreibdauer eines Bulk-Inserts **vor und nach** deinen
   Indizes. Ein Index ist immer ein Handel — beziffere ihn.
9. Finde die Top-5-Queries des Systems ueber `sys.dm_exec_query_stats` /
   Query Store statt ueber Gefuehl. Beschreib den Weg, den du dabei gegangen bist.

## Beispiele und Testfaelle

Die Zahlen unten sind Groessenordnungen aus einem Lauf mit 2 Mio `Attempt`-Zeilen und
sollen dir zeigen, wonach du suchst — deine eigenen Werte werden abweichen. Nachpruefbar
ist jeweils die *Richtung*: Operator im Plan und Logical Reads vorher gegen nachher. Jeder
Fall gehoert als Messung in die Tabelle unter `## Nachweise`.

1. **Scan wird Seek.** `SELECT AttemptId, SolvedOn FROM Attempts WHERE KataId = @id` fuer
   eine Kata mit 40 Versuchen: ohne Index ein Clustered Index Scan ueber alle 2 Mio Zeilen,
   ca. **11.400 Logical Reads**. Nach `CREATE NONCLUSTERED INDEX IX_Attempts_KataId ON
   Attempts (KataId)` ein Index Seek mit **ca. 130 Reads** — davon der groessere Teil aus
   dem Key Lookup auf `SolvedOn`.
2. **Covering Index raeumt den Key Lookup weg.** Derselbe Index mit
   `INCLUDE (SolvedOn, Duration_Minutes, Duration_Seconds)`: der Key-Lookup-Operator
   verschwindet vollstaendig aus dem Plan, die Reads fallen auf **ca. 5**. Der Test dazu
   prueft beides — Reads gesunken *und* kein `Key Lookup` mehr im XML des Plans.
3. **Spaltenreihenfolge entscheidet.** Fuer
   `WHERE KataId = @id ORDER BY SolvedOn DESC` nutzt `(KataId, SolvedOn)` einen Seek **ohne**
   Sort-Operator. `(SolvedOn, KataId)` kann fuer dieselbe Query nur scannen und braucht
   zusaetzlich einen Sort. Umgekehrt gilt fuer `WHERE SolvedOn >= @von` das Gegenteil.
   Zwei Indizes, zwei Queries, vier Plaene — und keine Geschmacksfrage mehr.
4. **`YEAR()` macht den Index unbrauchbar.** `WHERE YEAR(SolvedOn) = 2026` erzwingt einen
   Scan mit **ca. 11.400 Reads**, obwohl `(SolvedOn)` indiziert ist. Umformuliert zu
   `WHERE SolvedOn >= '2026-01-01' AND SolvedOn < '2027-01-01'` wird es ein Range Seek mit
   **ca. 380 Reads**. Gleiches Ergebnis-Set, im Test verglichen.
5. **`LIKE` und implizite Konvertierung.** `WHERE Titel LIKE '%owling'` scannt (der
   Praefix ist unbekannt), `LIKE 'Bowling%'` sucht. Und bei
   `Titel varchar(200)` gegen einen `nvarchar`-Parameter zeigt der Plan ein
   `CONVERT_IMPLICIT(nvarchar…)` auf der **Spaltenseite** plus Scan — der Datentyp des
   Parameters richtig gesetzt, und derselbe Index seekt wieder.
6. **Parameter Sniffing sichtbar gemacht.** `CREATE PROCEDURE GetAttempts @KataId ...`.
   Ruf sie zuerst mit einer seltenen Kata (12 Versuche) auf: der Plan wird Seek + Key
   Lookup, ca. 40 Reads. Derselbe zwischengespeicherte Plan fuer eine Lieblingskata mit
   **900.000** Versuchen kostet **ueber 2,8 Mio Reads**; im Plan steht *Estimated Rows 12*
   gegen *Actual Rows 900.000*. Nach `DBCC FREEPROCCACHE` und Erstaufruf mit dem haeufigen
   Wert entsteht ein Scan-Plan mit ca. 11.400 Reads — der ist fuer die seltene Kata dann
   200-mal zu teuer. Erst `OPTION (RECOMPILE)` bzw. `OPTIMIZE FOR UNKNOWN` bringt beide
   Faelle in einen vertretbaren Bereich; welche der drei Varianten du nimmst, begruendest
   du mit diesen Zahlen.
7. **Filtered Index und persistierte Spalte.** `WHERE IsDeleted = 0` bei 3 % geloeschten
   Katas: der Filtered Index ist ein Bruchteil der Groesse des vollen Index (Seitenzahl
   ueber `sys.dm_db_partition_stats` vergleichen) und wird nur genutzt, wenn das Praedikat
   der Query das des Index enthaelt — zeig auch den Fall, in dem er *nicht* greift.
   Fuer die Duration-Statistik: eine persistierte berechnete Spalte
   `TotalSeconds AS (Duration_Minutes * 60 + Duration_Seconds) PERSISTED` mit Index macht
   aus `WHERE Duration_Minutes * 60 + Duration_Seconds > 3600` einen Seek.
8. **Der Preis der Indizes.** Bulk-Insert von 200.000 Versuchen: ohne die neuen Indizes
   ca. **4,2 s**, mit allen ca. **9,6 s** — plus die zusaetzlichen Datenseiten. Diese Zahl
   gehoert in dieselbe Tabelle wie die Lese-Gewinne, sonst ist der Handel nicht beziffert.

## Fertig, wenn

Du fuer jede der Queries aus Kata 09_02 sagen kannst: welcher Index wird benutzt, wie viele
Logical Reads kostet sie, und woran du eine Regression erkennen wuerdest.

## Nachweise

Eine Tabelle **Query | Logical Reads vorher | nachher | geaenderte Struktur | Begruendung.**
Ohne Messwerte ist die Kata nicht bestanden.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine Tabelle mit vielen Zeilen und ein paar
Abfragen darauf genuegen — der Generator aus Kata 10_04 nimmt dir die Datenerzeugung ab,
ist aber nicht erforderlich.
**Empfohlen, nicht erforderlich:** Kata 09_02 oder 10_01 als Quelle realistischer Queries.
**Werkzeuge:** Docker Desktop (SQL Server), SSMS oder Azure Data Studio fuer die
Plananzeige.

## Skills

Ausfuehrungsplaene lesen, Index-Design, Covering Index, SARGability, Parameter Sniffing,
DMVs, Query Store, Messen statt Raten

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
