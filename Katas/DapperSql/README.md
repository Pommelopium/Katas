# Kata 10_01 — Dapper neben EF Core

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1 Abend · baut auf Kata 09_02 auf

## Ziel

Der zweite Datenzugriffsweg neben EF Core. Dapper ist kein ORM-Ersatz, sondern das
Werkzeug fuer Lesepfade, bei denen du das SQL selbst kontrollieren willst.

## Domaene: Kata-Tracker

Dasselbe Schema wie in Kata 09_02 — `Katas`, `Attempts`, `TrainingPlans` samt
Verknuepfungstabelle. Neu ist nur der Weg dorthin. Die drei Lesepfade, die hier auf Dapper
umgezogen werden, sind die teuersten der Anwendung:

- **Kata-Uebersicht** — Liste aus `Katas` mit Anzahl der Versuche, letztem Loesungsdatum und
  Bestzeit, gefiltert nach Tag und Level, seitenweise plus Gesamtzahl.
- **Trainingsplan-Detail** — ein `TrainingPlan` mit seinen Katas und deren Versuchen, ueber
  Multi-Mapping zu einem Objektbaum aggregiert.
- **Trainingsstatistik** — Versuche pro Woche und laengste Serie, reine Aggregation ohne
  Entitaeten.

Geschrieben wird weiterhin ausschliesslich ueber EF Core; Dapper sieht das Schema nur
lesend — mit der einen Ausnahme der gemeinsamen Transaktion in Aufgabe 8.

## Aufgaben

1. Nimm die drei teuersten Lesequeries aus Kata 09_02 und schreib sie mit Dapper neu.
   Beide Wege bleiben im Projekt: **EF Core fuer Schreiben, Dapper fuer Lesen.**
2. Handgeschriebenes SQL in einer eigenen Klasse pro Query, Ergebnis direkt in ein
   `record`-DTO gemappt — **keine** Entitaeten aus dem Lesepfad heraus reichen.
3. Parametrisierung: Ein Test muss beweisen, dass `'; DROP TABLE` als Parameterwert
   harmlos ist. Danach baue bewusst eine Query mit String-Konkatenation und zeig, dass
   dieselbe Eingabe dort trifft. Loesch sie wieder.
4. `QueryMultipleAsync` fuer eine Seite, die Liste **und** Gesamtzahl braucht — ein
   Roundtrip statt zwei.
5. Multi-Mapping (`splitOn`) fuer einen Join `Kata` -> `Attempt`, aggregiert zu einem
   Objektbaum. Beobachte, was dabei mit doppelten Elternzeilen passiert.
6. `DynamicParameters` fuer eine Suche mit **optionalen** Filtern. Kein
   `WHERE 1=1 AND ...`-String-Wildwuchs: baue die Bedingungen kontrolliert auf und
   halte die Query SARGable.
7. Eine Stored Procedure aufrufen (`CommandType.StoredProcedure`) inklusive
   `OUTPUT`-Parameter und Rueckgabewert.
8. **Transaktion ueber beide Wege**: eine Schreiboperation, die EF Core und Dapper in
   *derselben* `DbTransaction` nutzt (`dbContext.Database.GetDbConnection()` und
   `UseTransaction`). Beweise mit einem Test, dass ein Rollback beides zuruecknimmt.

## Entscheidung dokumentieren

Schreib eine halbe Seite: **Wann Dapper, wann EF Core?** Kriterien statt Geschmack —
Anzahl der Roundtrips, Kontrolle ueber den Plan, Change Tracking, Wartbarkeit,
Testbarkeit. Genau das ist die entscheidende Frage.

## Tests

Integrationstests gegen echtes SQL Server via Testcontainers. Kein Mock einer
`IDbConnection` — der Wert dieser Kata liegt im echten SQL.

## Beispiele und Testfaelle

Ausgangsbestand fuer alle Faelle: drei Katas — `Bowling` (Tag `tdd`, zwei Versuche: 42 und
35 Minuten), `FizzBuzz` (Tag `tdd`, ein Versuch: 12 Minuten) und `Taxi` (Tag `architecture`,
kein Versuch) — sowie ein Trainingsplan `Einstieg` mit `Bowling` und `FizzBuzz`.

1. **Uebersicht liefert die Aggregate.** Die Dapper-Query auf `Katas` liefert drei Zeilen;
   `Bowling` mit `AttemptCount = 2` und `BestMinutes = 35`, `Taxi` mit `AttemptCount = 0`
   und `BestMinutes = null` — die Kata ohne Versuch fehlt also **nicht** (`LEFT JOIN`, nicht
   `INNER JOIN`). Der Rueckgabetyp ist das `record`-DTO, keine EF-Entitaet: ein Test prueft,
   dass der `DbContext.ChangeTracker` danach leer ist.
2. **Parametrisiert ist harmlos.** Der Filter `tag` wird mit dem Wert
   `tdd'; DROP TABLE Katas;--` aufgerufen. Ergebnis: `200`-Pfad, **leere** Liste, kein
   Fehler — und `Katas` existiert danach noch mit drei Zeilen. Derselbe Wert gegen die
   bewusst zusammengebaute Variante aus Aufgabe 3 laesst die Tabelle verschwinden; der Test
   dazu belegt den Unterschied und wird mit der Query wieder geloescht.
3. **Leeres Ergebnis ist kein Fehler.** `tag = "haskell"` liefert eine leere Liste, nicht
   `null` und keine Exception. `QuerySingleOrDefaultAsync` fuer eine unbekannte `KataId`
   liefert `null`; `QuerySingleAsync` fuer dieselbe Id wirft — beide Faelle als Test, damit
   die Wahl der Methode eine Entscheidung ist.
4. **`QueryMultipleAsync` in einem Roundtrip.** Seite 1 mit `pageSize = 2` liefert zwei
   Zeilen **und** `TotalCount = 3` aus einem einzigen Kommando. Seite 2 liefert eine Zeile
   und dasselbe `TotalCount`; Seite 3 liefert eine leere Liste und weiterhin `TotalCount = 3`.
   Ueber `SqlConnection.StatisticsEnabled` bzw. einen Kommando-Zaehler wird geprueft: **ein**
   Roundtrip, nicht zwei.
5. **Multi-Mapping ueber zwei Tabellen.** Der Join `TrainingPlans -> Katas -> Attempts` mit
   `splitOn: "KataId,AttemptId"` liefert *vier* SQL-Zeilen (Bowling zweimal, FizzBuzz
   einmal, Taxi gar nicht), aggregiert aber zu **einem** Plan mit **zwei** Katas — `Bowling`
   mit zwei Versuchen, `FizzBuzz` mit einem. Ohne Aggregation entstehen zwei Bowling-Objekte:
   schreib beide Varianten als Test und halte die doppelte Elternzeile fest.
6. **Optionale Filter ohne String-Wildwuchs.** Mit `DynamicParameters`: kein Filter -> drei
   Zeilen; nur `tag = "tdd"` -> zwei; `tag = "tdd"` und `level = "Anfaenger"` -> die
   erwartete Teilmenge. Fuer jede Kombination enthaelt das abgesetzte SQL **genau** die
   Parameter, die gesetzt sind — keine unbenutzten Platzhalter, keine Funktion um die
   gefilterte Spalte (SARGable).
7. **Stored Procedure mit `OUTPUT`.** `usp_KataStats @Tag` liefert das Resultset und
   zusaetzlich `@TotalAttempts = 3` als `OUTPUT`-Parameter sowie den Rueckgabewert `0`. Ein
   Tag ohne Treffer liefert ein leeres Resultset und `@TotalAttempts = 0`.
8. **Transaktion ueber beide Wege.** In *einer* `DbTransaction`: EF Core legt eine vierte
   Kata an, Dapper schreibt im selben Kommandostrom einen Versuch dazu. Nach `Commit` finden
   beide Wege beides. Im Rollback-Fall — Dapper-Kommando wirft nach dem
   `SaveChangesAsync` — existieren danach **weder** Kata **noch** Versuch: die
   Dapper-Zaehlung liefert wieder drei Katas. Der Test, der beim Rollback nur eine der
   beiden Seiten prueft, ist der, der den Fehler durchlaesst.

## Voraussetzung

**Muss zuvor erledigt sein:** Kata 09_02 (EF Core) — du brauchst ein Modell mit Lesequeries
und einen `DbContext` fuer die gemeinsame Transaktion in Punkt 8. Alternativ ein eigenes
Schema mit drei Abfragen und einem EF-Core-Kontext darauf.
**Werkzeuge:** Docker Desktop (SQL Server via Testcontainers).

## Skills

Dapper, handgeschriebenes SQL, Multi-Mapping, `QueryMultiple`, Stored Procedures,
gemeinsame Transaktionen mit EF Core

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
