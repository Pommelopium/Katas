# Kata 08_02 — Producer/Consumer-Pipeline mit Channels

**Stufe 2: Async und Nebenlaeufigkeit** · Zeitrahmen: 3–4 h

## Ziel

Bounded Queues und Backpressure — die Grundlage jeder Ingest-Pipeline. Wer das kann, baut
Verarbeitungsstrecken, die unter Last nicht den Speicher sprengen.

## Aufgabe: Jahresimport der Trainingsprotokolle

Der Kata-Tracker bekommt einmal im Jahr die gesammelten Trainingsprotokolle einer ganzen
Uebungsgruppe: ein Verzeichnis `import/` mit einer CSV-Datei pro Teilnehmer
(`lasse.csv`, `mira.csv`, ...), Spalten `KataId,Datum,Dauer,Notiz` wie in Kata 07_03.
Zusammen sind das ein paar hundert Megabyte — mehr, als in den Speicher passt, wenn man
naiv alles einliest.

Der Reader ist schnell (er liest nur Zeilen), der Parser ist langsam (RFC 4180, Quotes,
mehrzeilige Notizen), der Writer ist wieder langsam (er schreibt in die
Tracker-Datenbank). Genau diese Ungleichheit ist die Uebung: Ohne Bremse laeuft der
Reader dem Parser davon und die Queue frisst den Speicher.

Verarbeite das Verzeichnis (nutzt den Parser aus Kata 07_03) in drei Stufen:

```
Reader (1 Task) --> Channel<RawLine> --> Parser (N Tasks) --> Channel<Record> --> Writer (1 Task)
```

## Anforderungen

1. `BoundedChannelOptions` mit fester Kapazitaet und `BoundedChannelFullMode.Wait` —
   ein schneller Reader muss von einem langsamen Parser **ausgebremst** werden.
2. `SingleReader` / `SingleWriter` korrekt setzen, wo es zutrifft.
3. Sauberes Shutdown: `channel.Writer.Complete()` am Ende jeder Stufe, Exceptions ueber
   `Complete(ex)` an die naechste Stufe propagieren.
4. `CancellationToken` beendet alle Stufen — kein verwaister Task, kein Deadlock.
5. `IProgress<T>` meldet verarbeitete Zeilen an die Konsole.

## Nachweise

- Beweise die Backpressure: baue eine kuenstliche Verzoegerung in den Parser und miss den
  Speicherverbrauch mit und ohne Kapazitaetsgrenze.
- Wirf eine Exception im Parser und zeige, dass Reader und Writer trotzdem terminieren.
- Brich mitten in der Verarbeitung ab und zeige, dass alle Tasks sauber enden.

## Beispiele und Testfaelle

Die Nachweise oben in Zahlen — jeder Fall ist ein automatisierter Test, keine
Konsolenbeobachtung:

- **Backpressure greift:** Kapazitaet 10, der Parser liest nichts (blockiert an einem
  `TaskCompletionSource`), der Reader will 1000 Zeilen schreiben. Nach dem Freigeben des
  Parsers hat der Reader vor dem Warten genau **11** Zeilen produziert (10 in der Queue,
  1 in `WriteAsync` haengend) — nicht 1000.
- **Speicher bleibt flach:** 500.000 Zeilen, Parser mit 1 ms kuenstlicher Verzoegerung.
  Mit Kapazitaet 100 bleibt der Peak konstant; mit `UnboundedChannel` waechst er in der
  Groessenordnung der Eingabe. Beide Werte notiert.
- **Completion propagiert:** `import/` mit drei Dateien mit 1000, 500 und 250 Zeilen. Der
  Writer schreibt genau **1750** Records, danach kehrt `await pipeline` zurueck und alle
  Stufen-Tasks sind `RanToCompletion` — kein Task bleibt uebrig.
- **Reihenfolge ist nicht garantiert:** Mit 4 Parser-Tasks kommen die 1750 Records in
  beliebiger Reihenfolge beim Writer an. Der Test vergleicht die **Menge** (jeder Record
  genau einmal), nicht die Sequenz.
- **Fehler in einer Stage:** Zeile 137 enthaelt ein unbalanciertes Quote. Der Parser
  ruft `Complete(ex)`, `await pipeline` wirft **genau diese** Exception (nicht
  `ChannelClosedException`, nicht `TaskCanceledException`), der Writer hat die 136 Records
  davor geschrieben, und Reader und Writer sind beide beendet.
- **Cancellation mitten im Durchlauf:** 100.000 Zeilen, Abbruch nach 50 ms.
  `OperationCanceledException`, alle Stufen-Tasks sind innerhalb von 200 ms abgeschlossen,
  die Zahl geschriebener Records liegt zwischen 1 und 99.999, und der letzte geschriebene
  Record ist vollstaendig (kein halber Datensatz).
- **Cancellation bei voller Queue:** Kapazitaet 10, der Writer haengt, der Reader steht in
  `WriteAsync`. Der Token loest die blockierte Schreiboperation aus — der Test laeuft in
  unter einer Sekunde durch und nicht in sein Timeout (Deadlock-Nachweis).
- **Fortschritt ist monoton:** Bei den 1750 Zeilen sind die `IProgress<T>`-Meldungen
  monoton steigend und der letzte Wert ist exakt 1750.

## Skills

`System.Threading.Channels`, Backpressure, strukturierte Nebenlaeufigkeit, Fehlerpropagation

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
