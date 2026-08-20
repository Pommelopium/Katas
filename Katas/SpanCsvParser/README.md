# Kata 07_03 — Span-basierter CSV-Parser

**Stufe 1: Modernes C# und Testbarkeit** · Zeitrahmen: 3–4 h

## Ziel

Performance-Bewusstsein. Der Sprung besteht darin, die Frage zu wechseln: nicht
"ist es schnell?", sondern "wie viel allokiert es und woher weiss ich das?".

## Aufgabe: Trainingsprotokoll importieren

Der Kata-Tracker soll Trainingsprotokolle einlesen, die andere Teilnehmer als CSV
exportiert haben — eine Datei `trainings.csv` mit den Spalten
`KataId,Datum,Dauer,Notiz`:

```
KataId,Datum,Dauer,Notiz
07_03,2026-08-20,00:45,Erster Versuch
07_03,2026-08-21,01:10,"Split, Substring, dann Span"
```

Die drei ersten Spalten sind harmlos, die `Notiz` ist es nicht: Teilnehmer schreiben dort
Kommas, Anfuehrungszeichen und mehrzeiligen Text hinein. Genau daran haengt der
RFC-4180-Teil. Und weil ein Jahresexport aus einer grossen Gruppe schnell im
dreistelligen Megabyte-Bereich liegt, haengt daran auch der Performance-Teil: Der Import
soll die Datei zeilenweise verarbeiten, nicht komplett in den Speicher ziehen.

## Aufgaben

1. Erweitere `Katas/CSVTabellen` zu einem vollstaendigen RFC-4180-Parser:
   - Quoting: `a,"b,c",d` sind drei Felder
   - escaped Quotes: `"er sagte ""hallo"""`
   - CRLF **innerhalb** eines gequoteten Feldes
   - leeres Feld vs. fehlendes Feld
2. **Version A** — geradeaus mit `string.Split` und `Substring`.
3. **Version B** — `ReadOnlySpan<char>`, `stackalloc`, `ArrayPool<T>`. Ziel: null
   Allokationen pro Feld beim Durchlaufen.
4. Beide mit **BenchmarkDotNet** vergleichen (`[MemoryDiagnoser]`) ueber eine generierte
   50-MB-Datei. Miss Zeit **und** allokierte Bytes.
5. `IAsyncEnumerable<CsvRow>` fuer Streaming-Verarbeitung, damit die Datei nie komplett im
   Speicher liegt. Speicherverbrauch gegen die naive Variante messen.

## Beispiele und Testfaelle

Parsen (gelten fuer Version A und Version B identisch — dieselbe Testsuite gegen beide
Implementierungen):

- `07_03,2026-08-20,00:45,Erster Versuch` -> 4 Felder, viertes Feld `Erster Versuch`
- `07_03,2026-08-21,01:10,"Split, Substring, dann Span"` -> 4 Felder, viertes Feld
  `Split, Substring, dann Span` (das Komma im Quote trennt nicht)
- `07_03,2026-08-21,01:10,"er sagte ""allokationsfrei"""` -> viertes Feld
  `er sagte "allokationsfrei"`
- `07_03,2026-08-22,00:30,"Zeile 1<CRLF>Zeile 2"` -> **eine** Zeile mit 4 Feldern, das
  vierte enthaelt den Umbruch; der Parser darf hier nicht in die naechste Zeile springen
- `07_03,,00:45,` -> 4 Felder, Feld 2 und Feld 4 sind leere Strings (nicht `null`) —
  abgrenzen gegen `07_03,2026-08-20` mit nur 2 Feldern, das als fehlende Spalte scheitert
- `07_03,2026-08-20,00:45,"unbalanciert` -> Fehler mit Zeilen- und Spaltennummer, kein
  stillschweigend abgeschnittenes Feld

Messung:

- Version B: `Allocated` im `[MemoryDiagnoser]` ist **0 B** fuer das Durchlaufen aller
  Felder einer Zeile (kein `Substring`, kein `string.Split`-Array)
- Version A gegen Version B auf derselben generierten 50-MB-Datei: beide Laufzeiten und
  beide Allokationswerte notiert — inklusive der Faelle, in denen B *nicht* gewinnt
- Streaming ueber `IAsyncEnumerable<CsvRow>`: der Peak-Speicherverbrauch bleibt bei der
  50-MB-Datei konstant und waechst nicht mit der Dateigroesse, waehrend die naive
  Variante in der Groessenordnung der Datei liegt

## Fertig, wenn

Du die Zahlen aus dem Benchmark erklaeren kannst — warum Version B schneller ist und wo
sie es *nicht* ist.

## Skills

`Span<T>` / `Memory<T>`, `ArrayPool<T>`, BenchmarkDotNet, `IAsyncEnumerable`, `yield return`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
