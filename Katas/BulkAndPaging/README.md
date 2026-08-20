# Kata 10_04 — Massendaten: Bulk-Import und Keyset-Pagination

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1 Abend · baut auf Kata 10_01 auf

## Ziel

Zwei Dinge, die jedes wachsende System irgendwann braucht und die mit `SaveChanges()` und
`Skip/Take` beide falsch geloest sind. Nebenbei entsteht der Datengenerator fuer Kata 10_02.

## Domaene: Kata-Tracker

Dieselbe Fachlichkeit wie in Kata 09_01 bis 10_03, nur in der Groesse, in der sie weh tut:
die **Trainings-Historie**. Aus einer Exportdatei eines alten Trackers werden
`Attempts`-Zeilen eingelesen — `(KataId, SolvedOn, Duration, Source)`, 2 Mio. Versuche ueber
mehrere Jahre. Der Import laeuft nachts und wird erneut angestossen, wenn ein neuer Export
kommt; dieselbe Datei zweimal einzulesen darf keine Versuche verdoppeln. Auf derselben
Tabelle blaettert die Oberflaeche die Historie absteigend nach Datum durch — und zwar bis
zur Seite 5000, waehrend nebenher weiter Versuche erfasst werden.

## Teil 1 — Import von 2 Millionen Zeilen

1. Baseline: importiere 100.000 Zeilen mit EF Core und `SaveChangesAsync()` pro Entitaet.
   Miss die Dauer. Das ist die Zahl, gegen die du optimierst.
2. Stufe fuer Stufe verbessern und **jede Stufe messen**:
   Batching (`SaveChanges` alle N), `ChangeTracker.AutoDetectChangesEnabled = false`,
   `ExecuteUpdate`/`ExecuteDelete` statt Load-Modify-Save, schliesslich `SqlBulkCopy`.
3. `SqlBulkCopy` richtig: `BatchSize`, `SqlBulkCopyOptions.TableLock`, Streaming ueber einen
   `IDataReader` statt einer vorher gefuellten `DataTable` — der Speicherverbrauch darf
   nicht mit der Zeilenzahl wachsen.
4. **Table-Valued Parameter** fuer den Fall "500 Zeilen an eine Prozedur uebergeben".
   Vergleich mit 500 Einzelaufrufen und mit einem `IN (...)`-Konstrukt aus 500 Parametern
   (und stoss dabei auf das Parameterlimit).
5. **Upsert**: `MERGE` gegen eine Staging-Tabelle plus Insert/Update-Trennung. Schreib auf,
   welche Race Conditions `MERGE` hat und wie du sie absicherst.
6. Wiederaufsetzbarkeit: der Import muss nach einem Abbruch mitten im Lauf ohne Duplikate
   fortsetzbar sein.

## Teil 2 — Blaettern, das auch auf Seite 5000 funktioniert

7. `OFFSET 100000 FETCH NEXT 20` messen und im Plan zeigen, warum die Zeit mit dem Offset
   **waechst**.
8. **Keyset-Pagination** (`WHERE (Datum, Id) < (@datum, @id) ORDER BY Datum DESC, Id DESC`)
   mit stabilem Tiebreaker. Miss die Dauer fuer Seite 1 und Seite 5000 — sie muss gleich
   bleiben.
9. Cursor **opak** nach aussen geben (Base64 des Sortierschluessels), nicht als
   Seitennummer. Erklaere, warum Offset-Pagination bei gleichzeitigen Inserts Zeilen
   doppelt oder gar nicht zeigt.
10. Gesamtzahl: `COUNT(*)` ueber 2 Mio. Zeilen kostet. Entscheide begruendet zwischen exakt,
    geschaetzt (`sys.dm_db_partition_stats`) und "gar nicht anzeigen".

## Nachweise

Eine Messtabelle mit Zeilen pro Sekunde je Importstufe, und eine mit Antwortzeit je
Seitentiefe fuer Offset vs. Keyset. Die Kata besteht aus den Zahlen.

## Beispiele und Testfaelle

Jeder Fall unten ist ein automatisierter Test gegen die echte Datenbank im Container —
Zeiten und Reads gehoeren als Assertion in den Test, nicht nur in die Messtabelle.

1. **Baseline gegen Bulk.** 100.000 Versuche mit `SaveChangesAsync()` pro Entitaet
   importieren, Dauer notieren. Derselbe Datensatz ueber `SqlBulkCopy` muss **mindestens
   50x schneller** sein. Beide Laeufe enden mit `SELECT COUNT(*) = 100000` und identischen
   Pruefsummen ueber `Duration`.
2. **2 Mio. Zeilen im Budget.** Der Vollimport laeuft in **unter 60 Sekunden** durch und der
   Prozess bleibt dabei unter **200 MB** Arbeitsspeicher. Der zweite Teil ist der
   eigentliche Test: derselbe Lauf mit 200.000 Zeilen darf **nicht** messbar weniger
   Speicher brauchen — sonst haengt noch eine `DataTable` im Pfad statt eines
   `IDataReader`.
3. **500 Zeilen an eine Prozedur.** TVP mit 500 Versuchen gegen 500 Einzelaufrufe gegen ein
   `IN (...)` aus 500 Parametern. Erwartung: ein Roundtrip statt 500, und die
   Parameter-Variante scheitert reproduzierbar am Limit von 2100 Parametern — der Test
   erwartet diese Exception, statt sie zu umgehen.
4. **Upsert ist idempotent.** Die Exportdatei zweimal einlesen: nach dem zweiten Lauf
   stehen genau 2.000.000 Zeilen in der Tabelle, keine 4.000.000 und keine 1.999.999.
   Aendert sich in der Datei eine `Duration` von `00:42:00` auf `00:38:00`, ist danach nur
   der neue Wert gespeichert und die Zeilenzahl unveraendert.
5. **Abbruch laesst keine halben Daten zurueck.** Import nach 1,2 Mio. Zeilen hart
   abbrechen (Token `Cancel`, Verbindung kappen oder Prozess killen). Danach ist der Zustand
   **entweder** 0 importierte Zeilen **oder** eine vollstaendige Menge abgeschlossener
   Batches — nie ein angebrochener Batch. Der Wiederaufsetzlauf fuehrt anschliessend zu
   genau 2.000.000 Zeilen.
6. **Offset gegen Keyset.** `OFFSET 0 FETCH NEXT 20` und `OFFSET 100000 FETCH NEXT 20`
   messen: die Zeit und die `logical reads` (ueber `SET STATISTICS IO ON`) wachsen mit dem
   Offset um mehr als eine Groessenordnung. Dieselben beiden Seiten per Keyset: Dauer und
   Reads sind fuer Seite 1 und Seite 5000 **gleich** (Abweichung unter 20 %) und die Reads
   liegen in der Groesse der Seitenbreite, nicht der Tabelle.
7. **Stabile Sortierung bei gleichzeitigem Insert.** Seite 1 abrufen, dann 50 neue Versuche
   mit heutigem Datum einfuegen, dann Seite 2 abrufen. Per Keyset-Cursor enthaelt die
   Vereinigung beider Seiten **keine doppelte Id** und ueberspringt keine Zeile; mit
   `OFFSET 20` zeigt derselbe Ablauf nachweisbar Duplikate. Zwei Versuche mit identischem
   `SolvedOn` muessen ueber den Tiebreaker `Id` in stabiler Reihenfolge kommen — derselbe
   Cursor zweimal abgerufen ergibt dasselbe Ergebnis.
8. **Letzte Seite und leeres Ergebnis.** Die letzte Seite liefert die restlichen Zeilen und
   `nextCursor: null`; ein Abruf darueber hinaus ergibt eine **leere** Liste mit
   `nextCursor: null` und keinen Fehler. Ein Filter ohne Treffer (`KataId` ohne Versuche)
   ergibt ebenfalls eine leere Liste, und ein manipulierter oder fremd erzeugter Cursor
   wird abgelehnt, statt still auf Seite 1 zurueckzufallen.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Eine Tabelle und ein EF-Core-Kontext darauf
genuegen als Ausgangspunkt.
**Empfohlen, nicht erforderlich:** Kata 09_02 (Modell), Kata 10_01 (Dapper fuer die
Lesepfade in Teil 2).
**Werkzeuge:** Docker Desktop (SQL Server).

## Skills

`SqlBulkCopy`, Table-Valued Parameter, `MERGE`/Upsert, `ExecuteUpdate`,
Keyset-Pagination, opake Cursor, Messen unter realistischer Datenmenge

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
