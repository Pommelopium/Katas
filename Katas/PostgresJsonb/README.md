# Kata 10_05 — Zweiter Provider: PostgreSQL und JSONB

**Stufe 3: API, Persistenz, Architektur** · Zeitrahmen: 1–2 Abende · baut auf Kata 09_02 auf

## Ziel

PostgreSQL steht mittlerweile in fast jeder .NET-Anzeige neben SQL Server. Der Wert dieser
Kata ist nicht "noch eine Datenbank", sondern der Beweis, dass deine Persistenzschicht
austauschbar ist — und das Gefuehl dafuer, wo sie es *nicht* ist.

## Domaene: Kata-Tracker

Dasselbe Modell wie in Kata 09_02 — `Kata`, `Attempt`, `TrainingPlan` — jetzt auf einem
zweiten Provider. Neu hinzu kommt genau ein Feld: `Attempt.Notes`, die frei geformten
Notizen zu einem Versuch, gespeichert als `jsonb`. Die Notizen sind bewusst nicht
schematisiert, weil sich ihre Form mit der Trainingspraxis aendert; ein Dokument der
**Version 1** sieht so aus:

```json
{ "schemaVersion": 1, "language": "csharp", "mood": "gut",
  "tags": ["tdd", "refactoring"], "obstacles": ["Namensgebung"] }
```

Ab **Version 2** wird `mood` durch einen numerischen `rating` (1–5) ersetzt und `language`
zu einem Objekt `{ "name": ..., "version": ... }` ausgebaut. Beide Versionen liegen
parallel in derselben Spalte und muessen beide lesbar bleiben — das ist der Fall, an dem
sich Schema-Evolution in JSONB gegen Schema-Evolution in Spalten (Kata 09_02,
Expand/Contract) messen laesst. Die Volltextsuche aus Aufgabe 6 sucht in den `obstacles`
und im Kata-Titel; die Grenzziehung aus Aufgabe 5 entscheidet, welches dieser Felder
eine echte Spalte verdient.

## Aufgaben

1. Bring das Modell aus Kata 09_02 auf PostgreSQL (Npgsql). **Beide** Provider bleiben
   lauffaehig, umschaltbar per Konfiguration.
2. Fuehr eine Liste der Stellen, die du anfassen musstest. Erwartbare Kandidaten:
   Bezeichner-Casing (`snake_case` vs. Quoting), `rowversion` gegen `xmin`,
   `DateTime`/`timestamptz` und Zeitzonen, `IDENTITY` vs. Sequenz, Standard-Collation und
   Gross-/Kleinschreibung bei Vergleichen. **Diese Liste ist das Ergebnis der Kata.**
3. Migrationen fuer zwei Provider: getrennte Migrations-Assemblies. Erklaere, warum ein
   gemeinsamer Migrationsordner hier nicht funktioniert.
4. **JSONB**: speichere die frei geformten Notizen eines Attempts als `jsonb`. Abfragen
   ueber die Npgsql-Operatoren (`@>`, `->>`), plus ein **GIN-Index**, dessen Wirkung du mit
   `EXPLAIN ANALYZE` vorher/nachher belegst.
5. Ziehe die Grenze schriftlich: **was gehoert in Spalten, was in JSONB?** Kriterien —
   Abfragbarkeit, Constraints, Migrierbarkeit, Schema-Evolution.
6. Volltextsuche mit `tsvector` als generierte Spalte plus Index. Vergleich zu
   `LIKE '%...%'` aus Kata 10_02 in Aufwand und Laufzeit.
7. `EXPLAIN (ANALYZE, BUFFERS)` lesen: Seq Scan, Bitmap Heap Scan, Nested Loop. Ordne die
   Operatoren denen aus Kata 10_02 zu — dieselben Konzepte, andere Namen.
8. `INSERT ... ON CONFLICT DO UPDATE` als Upsert, verglichen mit dem `MERGE` aus Kata 10_04.
9. Ein Provider-uebergreifender Integrationstest-Lauf: dieselbe Testsuite laeuft ueber
   Testcontainers **zweimal**, einmal gegen SQL Server, einmal gegen PostgreSQL. Gruen in
   beiden Faellen — oder ein dokumentierter, begruendeter Unterschied.

## Beispiele und Testfaelle

Alle Faelle laufen als Integrationstests gegen den Testcontainers-PostgreSQL. Wo von
Ausfuehrungsplaenen die Rede ist: `EXPLAIN (ANALYZE, BUFFERS)` im Test absetzen und den
Plantext pruefen, nicht per Augenschein im Werkzeug.

1. **Dokument rein, Dokument raus.** Ein `Attempt` mit den Notizen
   `{ "schemaVersion": 1, "language": "csharp", "mood": "gut", "tags": ["tdd"] }` wird
   gespeichert und wieder geladen; die Spalte ist `jsonb` (nicht `json` und nicht `text`,
   Nachweis ueber `information_schema.columns`), und die Schluesselreihenfolge im
   Rueckgabewert darf abweichen — JSONB normalisiert. Doppelte Schluessel im Eingabetext
   werden auf einen reduziert.
2. **Containment mit `@>`.** Bei drei Versuchen, davon zwei mit `"language": "csharp"`,
   liefert `WHERE notes @> '{"language":"csharp"}'::jsonb` genau diese zwei.
   `notes @> '{"tags":["tdd"]}'::jsonb` findet Versuche mit `tdd` **unter anderen** Tags
   mit — der Unterschied zur Gleichheit auf dem ganzen Dokument, der einen eigenen Test
   verdient.
3. **Feldzugriff mit `->>`.** `WHERE notes ->> 'language' = 'csharp'` liefert dieselbe
   Menge wie Fall 2, `notes -> 'language'` dagegen den JSONB-Wert `"csharp"` mit
   Anfuehrungszeichen. Ein Test, der `->>` mit `->` verwechselt, ist rot — genau darum
   geht es hier. Zusaetzlich: `(notes ->> 'schemaVersion')::int = 1` als Cast auf einen
   Zahlenwert.
4. **GIN-Index macht aus Seq Scan einen Index Scan.** Fuer 200.000 Versuche (aus Kata
   10_04 erzeugt) enthaelt der Plan von `notes @> '{"language":"fsharp"}'::jsonb` **ohne**
   Index `Seq Scan on attempts`, **mit** `CREATE INDEX ... USING gin (notes jsonb_path_ops)`
   einen `Bitmap Index Scan` plus `Bitmap Heap Scan`. Beide Plantexte werden im Test
   geprueft, und die gelesenen Bloecke (`BUFFERS`) sinken um mindestens eine
   Groessenordnung. Zweiter Teil: mit `jsonb_path_ops` funktioniert `@>`, aber die
   Existenzabfrage `notes ? 'mood'` nutzt den Index **nicht** — dafuer braucht es
   `jsonb_ops`. Halte fest, welche Operatorklasse du gewaehlt hast und warum.
5. **Teilaktualisierung ohne das ganze Dokument.** `jsonb_set(notes, '{mood}', '"mies"')`
   aendert genau ein Feld; alle uebrigen Schluessel sind danach unveraendert (im Test Feld
   fuer Feld verglichen), und die Anwendung hat das Dokument nie vollstaendig zum Server
   geschickt. Gegenprobe: derselbe Vorgang als Read-Modify-Write aus zwei parallelen
   Transaktionen verliert eine Aenderung, das serverseitige `jsonb_set` nicht. Ergaenzend
   `notes - 'mood'` zum Entfernen eines Schluessels und `notes || '{"reviewed":true}'`
   zum Hinzufuegen.
6. **Schema-Wechsel im Dokument.** In der Tabelle liegen Versuche mit `schemaVersion 1`
   (`"mood": "gut"`, `"language": "csharp"`) und mit `schemaVersion 2` (`"rating": 4`,
   `"language": {"name":"csharp","version":"13"}`). Eine Abfrage liest **beide** korrekt:
   `COALESCE(notes ->> 'language', notes #>> '{language,name}')` liefert fuer alle Zeilen
   `csharp`, und das Mapping alt->neu (`"gut"` -> `4`) ist als Test fixiert. Kein
   `ALTER TABLE`, keine Migration — das ist der Vergleichspunkt zum Expand/Contract aus
   Kata 09_02, und der Preis dafuer (die Lesestelle muss beide Formen kennen) gehoert in
   die Grenzziehung aus Aufgabe 5.
7. **Grenzfall: Feld fehlt, Feld ist `null`, Dokument ist `null`.** Drei Zeilen: eine ohne
   Schluessel `mood`, eine mit `"mood": null`, eine mit `notes IS NULL`. Dann gilt:
   `notes ->> 'mood'` ergibt in den ersten beiden Faellen SQL-`NULL` — nicht
   unterscheidbar; `notes ? 'mood'` trennt sie (`false` gegen `true`);
   `notes -> 'mood' = 'null'::jsonb` findet nur die zweite. Ein `WHERE notes ->> 'mood'
   <> 'gut'` liefert **keine** dieser drei Zeilen — die klassische Dreiwertlogik-Falle, die
   der Test festnagelt. Und `notes @> '{"mood":null}'::jsonb` verhaelt sich anders als
   erwartet: probier es aus und schreib das Ergebnis hin.
8. **Volltextsuche gegen `LIKE`.** Die generierte `tsvector`-Spalte ueber Titel und
   `obstacles` findet den Versuch mit `"obstacles": ["Namensgebung"]` bei der Suche nach
   `Namensgebung`; der Plan zeigt einen Index Scan auf den GIN-Index, waehrend
   `LIKE '%Namensgebung%'` bei derselben Datenmenge einen Seq Scan erzeugt. Beide
   Laufzeiten werden notiert.
9. **Dieselbe Suite, zwei Provider.** Jeder provider-neutrale Fall aus Kata 09_02 laeuft
   unveraendert gegen PostgreSQL gruen. Die JSONB-Faelle 1 bis 8 laufen ausschliesslich
   gegen PostgreSQL und werden dort explizit uebersprungen, wo SQL Server das Konzept
   nicht hat — mit einem Kommentar, der genau das begruendet. Ein stiller Skip ist kein
   dokumentierter Unterschied.

## Fertig, wenn

Die Testsuite gegen beide Provider laeuft und du die Unterschiedsliste aus Punkt 2 aus dem
Kopf aufsagen kannst.

## Voraussetzung

**Muss zuvor erledigt sein:** Kata 09_02 (EF Core) — der Providerwechsel braucht ein
bestehendes Modell samt Migrationen und Testsuite. Ohne die ist nichts zum Umstellen da.
**Werkzeuge:** Docker Desktop (PostgreSQL, SQL Server).

## Skills

PostgreSQL, Npgsql, provider-neutrales Modell, JSONB, GIN-Index, `EXPLAIN ANALYZE`,
Volltextsuche, Upsert-Varianten, Multi-Provider-Tests

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
