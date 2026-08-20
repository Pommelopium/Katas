# Kata 09_02 — EF Core richtig

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende · baut auf Kata 09_01 auf

## Ziel

Persistenz jenseits von "`DbSet` und fertig". Genau die Punkte, die in echten Projekten
ueber Korrektheit und Performance entscheiden.

## Domaene: Kata-Tracker

Dieselbe Fachlichkeit wie in Kata 09_01, jetzt aber echt persistiert. Das Modell besteht
aus drei Dingen:

- `Kata` — `KataId`, Titel, Tags, `IsDeleted` (Soft Delete), `RowVersion`.
- `Attempt` — `AttemptId`, gehoert zu genau einer `Kata`, `SolvedOn` und eine `Duration`
  als Owned Type (`Minutes` + `Seconds`).
- `TrainingPlan` — benannter Trainingsplan mit einer geordneten Liste von `Kata`-Verweisen;
  liefert die mehrfachen `Include`-Pfade, an denen `AsSplitQuery()` und N+1 sichtbar werden.

Das Feld, das nach Expand/Contract umgebaut wird, ist `Kata.Level`: heute ein freier
`string` (`"Anfaenger"`, `"Fortgeschritten"`, `"Profi"`), kuenftig ein Enum `KataLevel`,
in der Datenbank als `int`. Der In-Memory-Store aus Kata 09_01 wird ersetzt, die
Integrationstests von dort muessen unveraendert weiter gruen sein.

## Aufgaben

1. `DbContext` mit `IEntityTypeConfiguration<T>` pro Entitaet — **keine** EF-Attribute im
   Domaenenmodell.
2. Strongly-typed IDs: `readonly record struct KataId(Guid Value)` mit `ValueConverter` und
   `ValueComparer`.
3. Owned Types fuer Value Objects (`Duration`), globaler `HasQueryFilter` fuer Soft Delete.
4. Optimistic Concurrency ueber `rowversion`. `DbUpdateConcurrencyException` wird auf
   `409 Conflict` mit ProblemDetails gemappt.
5. **Migration ohne Downtime**: baue eine `string`-Spalte in ein Enum um, nach dem
   Expand/Contract-Muster:
   `neue Spalte anlegen -> Backfill -> Doppelschreiben -> Cutover -> alte Spalte droppen`.
   Teste Up **und** Down.
6. Lesepfade: `AsNoTracking()`, Projektion direkt in DTOs (`Select` vor `ToListAsync`),
   `AsSplitQuery()` gegen das kartesische Produkt bei mehreren `Include`.
7. Erzeuge bewusst ein **N+1-Problem**, weise es im generierten SQL nach (EF-Logging auf
   `Information`), dann fixe es. Schreib auf, woran du es erkannt hast.
8. Integrationstests gegen echtes SQL Server via **Testcontainers** — nicht der
   InMemory-Provider. Begruende, warum InMemory hier die falsche Wahl ist.

## Beispiele und Testfaelle

Alle Faelle laufen als Integrationstests gegen den Testcontainers-SQL-Server. Wo vom
generierten SQL die Rede ist: EF-Logging auf `Information` mitschreiben und im Test gegen
den Log-Text pruefen, nicht per Augenschein.

1. **Strongly-typed ID kommt als `uniqueidentifier` an.** `KataId` wird ueber
   `ValueConverter` gespeichert; ein `Where(k => k.Id == id)` erzeugt einen
   parametrisierten Vergleich auf `[Id]` — kein `.ToString()`, kein Client-seitiges
   Filtern. Ein Test, der zwei Katas anlegt und eine per Id laedt, findet genau eine.
2. **Owned Type liegt in derselben Tabelle.** `Attempt.Duration` erzeugt die Spalten
   `Duration_Minutes` und `Duration_Seconds` in `Attempts` — keine zweite Tabelle und
   kein Join. Nachweis ueber das Migrations-Skript oder `INFORMATION_SCHEMA.COLUMNS`.
3. **Soft Delete ist unsichtbar.** Nach `Delete` einer Kata liefert
   `context.Katas.ToListAsync()` sie nicht mehr, das SQL enthaelt
   `WHERE [k].[IsDeleted] = CAST(0 AS bit)`. `IgnoreQueryFilters()` liefert sie wieder —
   und die Zeile existiert in der Datenbank noch.
4. **Concurrency-Konflikt endet als 409.** Zwei `DbContext`-Instanzen laden dieselbe Kata,
   beide aendern den Titel, beide speichern. Das zweite `SaveChangesAsync` wirft
   `DbUpdateConcurrencyException`; der API-Aufruf darueber antwortet mit `409 Conflict`
   und ProblemDetails (`title`, `status: 409`). Der erste Schreibvorgang bleibt
   gewinnend erhalten.
5. **Expand/Contract ohne Downtime.** Der harte Fall: alte und neue Anwendungsversion
   laufen gleichzeitig gegen dasselbe Schema und beide Testsuiten sind gruen.
   - Nach *Expand* (`Level` bleibt, `LevelCode int NULL` kommt hinzu): alte Version
     liest und schreibt weiter `Level`, neue Version laeuft schon.
   - Nach *Backfill*: fuer jede Zeile gilt `LevelCode` passt zu `Level`; kein `NULL` mehr.
   - Waehrend *Doppelschreiben*: eine Kata, die die **alte** Version mit
     `Level = "Profi"` anlegt, liest die neue Version als `KataLevel.Profi`; eine Kata,
     die die **neue** Version mit `KataLevel.Anfaenger` anlegt, liest die alte Version als
     `"Anfaenger"`.
   - Nach *Contract* (`Level` gedroppt): nur noch die neue Version laeuft, das Enum ist
     die einzige Wahrheit.
   - `Down` jeder einzelnen Migration wird ausgefuehrt und fuehrt zurueck auf ein Schema,
     gegen das die vorige Version wieder gruen ist.
6. **N+1 nachgewiesen und behoben.** Ein Trainingsplan mit 10 Katas, jede mit Versuchen.
   Die naive Fassung (Plan laden, dann pro Kata die Versuche nachladen) erzeugt
   **11 `SELECT`-Statements** — im Test gezaehlt, nicht geschaetzt. Die behobene Fassung
   erzeugt hoechstens **2**. Notiere in einem Kommentar, woran du es im Log erkannt hast.
7. **Projektion laedt nur, was sie braucht.** `Select` in ein DTO **vor** `ToListAsync`:
   das SQL enthaelt nur die projizierten Spalten, kein `SELECT *`, und der ChangeTracker
   ist nach `AsNoTracking()` leer (`context.ChangeTracker.Entries()` ist leer).
8. **`AsSplitQuery()` schlaegt das kartesische Produkt.** Plan mit 2 Includes
   (Katas + Attempts): die Single-Query-Variante liefert Zeilenzahl `Katas × Attempts`,
   die Split-Variante setzt drei getrennte `SELECT` ab und liefert dasselbe
   Objektergebnis. Beide Ergebnisse werden im Test verglichen — identisch.
9. **InMemory haette es nicht gefunden.** Nimm mindestens einen der Faelle oben
   (`rowversion`, Owned-Type-Spalten oder eine `unique`-Verletzung) und zeig, dass er
   gegen den InMemory-Provider gruen wird, obwohl er es nicht sein duerfte. Das ist die
   Begruendung aus Aufgabe 8, nur eben als Test.

## Fertig, wenn

Du fuer jede Query im System das generierte SQL kennst und erklaeren kannst.

## Voraussetzung

Docker Desktop (fuer Testcontainers).

## Skills

EF Core 9/10, Migrations-Strategie, Value Converters, Testcontainers, SQL-Verstaendnis

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
