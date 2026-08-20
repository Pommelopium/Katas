# Kata 11_08 — Diagnose im laufenden Betrieb

**Stufe 4: Verteilte Systeme und Betrieb** · Zeitrahmen: 1–2 Abende

## Ziel

Kata 11_02 macht das System von aussen erklaerbar. Diese Kata geht einen Schritt weiter: der
Prozess laeuft, verhaelt sich falsch, und du hast **keinen Debugger**. Nur die Werkzeuge,
die von aussen an einen laufenden Prozess herankommen.

## Domaene: der Kata-Tracker in Produktion

Geuebt wird am eigenen Dienst: der **Kata-Tracker** aus Kata 09_01 laeuft seit Tagen, und die
Beschwerden kommen nicht als Exception, sondern als Gefuehl — "die Streak-Seite braucht ewig",
"der Container wurde nachts neu gestartet". Kein Stacktrace, kein Breakpoint, nur ein Prozess
mit einer Prozess-ID.

Damit es etwas zu finden gibt, baust du die Fehler unten **selbst** in den Tracker ein, jeden
an einer plausiblen Stelle: das Leck in einen Cache der Kata-Liste, die Starvation in den
Aufruf, der die Versuche laedt, den Hotspot in die Streak-Berechnung, den Deadlock zwischen
Schreiben und Lesen eines Versuchs, den GC-Druck in die Serialisierung der Seitenantwort. Wer
Kata 09_01 nicht hat, nimmt eine Konsolenanwendung mit einer Lastschleife statt HTTP — die
Fehlerbilder und die Werkzeuge bleiben dieselben.

Der Ablauf ist immer gleich und immer in dieser Richtung: Last drauf, Kennzahl beobachten,
Hypothese formulieren, **erst dann** in den Code sehen. Notiere die Hypothese, bevor du sie
pruefst — sonst weisst du hinterher nicht, ob du diagnostiziert oder geraten hast.

## Voraussetzung

**Muss zuvor erledigt sein:** keine Kata. Du brauchst nur einen Prozess, in den du die
Fehler unten absichtlich einbaust — eine Konsolenanwendung reicht.
**Empfohlen, nicht erforderlich:** Kata 11_02 (Metriken), Kata 11_03 (Container).
**Werkzeuge:** `dotnet-counters`, `dotnet-dump`, `dotnet-trace`, `dotnet-gcdump`
(alle per `dotnet tool install`), optional Docker Desktop.

## Die Regel dieser Kata

Jeder der fuenf Fehler wird **absichtlich gebaut**, dann **ohne Debugger** gefunden. Erst
messen, dann eine Hypothese, dann der Blick in den Code — nicht umgekehrt.

## Minimalpfad

Fehler 1 und 2, plus Punkt 7.

## Die fuenf Fehler

1. **Speicherleck**: ein `static Dictionary`, das nie geleert wird, oder ein nicht
   abgemeldetes Event. Finde es ueber zwei `dotnet-gcdump`-Aufnahmen im Abstand von
   Minuten und den Vergleich der Objektzahlen. Nenne den Pfad, der das Objekt am Leben
   haelt.
2. **Thread-Pool-Starvation**: irgendwo ein `.Result` oder `.Wait()` auf einem async-Aufruf
   unter Last. Symptom sind Antwortzeiten, die mit der Last explodieren, obwohl die CPU
   langweilt. Zeig es an `threadpool-queue-length` und `threadpool-thread-count` in
   `dotnet-counters`, dann behebe es. **Das ist der haeufigste echte .NET-Produktionsfehler.**
3. **CPU-Last ohne Grund**: eine heisse Schleife oder eine teure Serialisierung pro Request.
   Finde sie mit `dotnet-trace` und lies das Ergebnis als Flame Graph (Speedscope oder
   PerfView).
4. **Deadlock** zwischen zwei Locks. Nimm einen Dump mit `dotnet-dump`, dann in `dotnet-dump
   analyze`: `clrstack` und `syncblk`. Zeig die zwei Threads, die aufeinander warten.
5. **GC-Druck**: hohe Allokationsrate mit Gen-2-Sammlungen und Large Object Heap. Zeig
   `gen-2-gc-count`, `alloc-rate` und `time-in-gc`, dann senke die Allokationen (hier zahlt
   sich Kata 07_03 aus) und miss den Unterschied.

## Weitere Aufgaben

6. **Diagnose im Container**: dieselbe Analyse an einem Prozess in Docker/Kubernetes —
   Tools per Sidecar oder `dotnet-monitor`, Dump herausholen, ohne den Prozess zu killen.
   Das ist der Teil, der in der Praxis scheitert.
7. **Runbook schreiben.** Pro Symptom eine halbe Seite: *Antwortzeiten steigen*,
   *Speicher waechst monoton*, *CPU bei 100 %*, *Prozess haengt*, *Container wird
   ge-OOMKillt*. Je Symptom: erste Kennzahl, erstes Werkzeug, erster Verdacht. Dieses
   Runbook ist das eigentliche Ergebnis der Kata — es ist das, was beim naechsten Vorfall
   zaehlt.
8. `EventCounter`/`Meter`-Werte und Logs so setzen, dass der jeweilige Fehler beim
   **naechsten** Mal ohne Dump auffaellt. Diagnose ist einmalig, Instrumentierung bleibt.
9. Der Grenzfall: `OutOfMemoryException` im Container bei gesetztem Memory-Limit. Erklaere,
   warum die .NET-Heap-Grenze das Container-Limit kennen muss
   (`DOTNET_GCHeapHardLimitPercent`) und was ohne diese Einstellung passiert.

## Beispiele und Testfaelle

Jeder Fall hat dieselbe Form: **Symptom** erzeugen, mit **einem** Werkzeug an **einem** Befund
festmachen, Fix, **Gegenprobe** mit derselben Messung. Ein Fall gilt erst als bestanden, wenn
die Zahl vor dem Fix reproduzierbar schlecht und nach dem Fix reproduzierbar gut ist — beide
Messwerte aufschreiben.

1. **Speicher waechst monoton.** 20 Minuten Last auf `GET /api/v1/katas`, dabei
   `dotnet-counters monitor --counters System.Runtime`. Befund: `gc-heap-size` steigt ueber
   drei Gen-2-Sammlungen hinweg und faellt nie zurueck. Zwei `dotnet-gcdump` im Abstand von
   zehn Minuten, verglichen: die Zahl der `Kata`-Objekte waechst linear mit der Zahl der
   Requests, und der Haltepfad endet im `static Dictionary`. Nachweis der Diagnose: den Pfad
   benennen koennen, ohne den Code offen zu haben. Gegenprobe: derselbe Lauf, `gc-heap-size`
   pendelt um einen konstanten Wert, die Objektzahl im zweiten gcdump liegt im Rahmen des
   ersten.
2. **Requests haengen, CPU langweilt.** 50 gleichzeitige Aufrufe auf den Endpunkt mit dem
   `.Result`. Befund in `dotnet-counters`: `threadpool-queue-length` steigt in die Hunderte,
   `threadpool-thread-count` waechst im Sekundentakt um genau einen Thread (die Injektionsrate
   des Pools), `cpu-usage` bleibt unter 20 %. Genau diese Kombination — Queue lang, CPU
   niedrig — ist der Fingerabdruck von Starvation und von nichts anderem. Gegenprobe nach
   `await`: Queue-Length bleibt einstellig, Thread-Count konstant, die p99-Antwortzeit faellt
   um eine Groessenordnung.
3. **Der Unterschied zwischen langsam und blockiert.** Denselben Lastlauf einmal mit dem
   `.Result`-Fehler und einmal mit einem kuenstlichen `await Task.Delay(200)` fahren. Beide
   sind langsam, nur einer zeigt die wachsende Queue. Wer die zwei Faelle an der Kennzahl
   unterscheiden kann, hat den Punkt der Kata verstanden.
4. **CPU bei 100 %.** Last auf `GET /api/v1/stats/streak`, `dotnet-trace collect --profile
   cpu-sampling`, Ergebnis als Flame Graph. Befund: ein einziger Rahmen haelt ueber die
   Haelfte der Samples — die Streak-Schleife oder die Serialisierung, mit Name und
   prozentualem Anteil. Gegenprobe: nach dem Fix liegt derselbe Rahmen unter 5 % der Samples,
   und der breiteste Balken ist ein anderer. Die Kennzahl ist der Anteil, nicht das Gefuehl.
5. **Prozess haengt vollstaendig.** Zwei Locks in umgekehrter Reihenfolge nehmen, bis kein
   Request mehr antwortet. `dotnet-dump collect`, dann in `analyze`: `clrstack -all` zeigt
   zwei Threads, beide in einem `Monitor.Wait`, `syncblk` nennt fuer jeden Sync-Block den
   besitzenden Thread. Befund: Thread A haelt den Block, auf den B wartet, und umgekehrt —
   beide Thread-IDs nennen. Gegenprobe nach dem Fix (feste Lock-Reihenfolge oder ein Lock
   weniger): der Lastlauf laeuft durch, ein Dump im Betrieb zeigt in `syncblk` keine
   wartenden Threads mehr.
6. **Allokationsrate statt Heap-Groesse.** Der GC-Druck-Fehler zeigt sich *nicht* als
   wachsender Heap — der Speicher bleibt stabil. Befund: `alloc-rate` im hohen MB/s-Bereich,
   `gen-2-gc-count` steigt im Minutentakt, `time-in-gc` deutlich ueber 10 %, und im gcdump
   liegen die grossen Arrays auf dem Large Object Heap. Gegenprobe nach der
   Allokationsreduktion (Kata 07_03): `time-in-gc` im niedrigen einstelligen Prozentbereich,
   `gen-2-gc-count` waehrend des ganzen Laufs nahezu unveraendert, Durchsatz messbar hoeher.
7. **OOMKill im Container.** Denselben Speicherleck-Prozess in Docker mit
   `--memory=256m` starten. Ohne `DOTNET_GCHeapHardLimitPercent` stirbt der Container per
   Exit Code 137, **ohne** dass eine `OutOfMemoryException` im Log steht — das ist der Befund:
   der Kill kommt von aussen, nicht aus der Laufzeit. Mit gesetzter Grenze erscheint statt des
   137 eine `OutOfMemoryException` mit Stacktrace im Log. Gegenprobe: der Dump wird per
   `dotnet-monitor` aus dem laufenden Container geholt, ohne den Prozess zu beenden — vorher
   und nachher antwortet derselbe Health-Endpunkt.
8. **Das Runbook wird geprueft, nicht geglaubt.** Lass jemanden (oder dich selbst nach einer
   Woche) einen der fuenf Fehler blind einbauen. Dann nur mit dem Runbook aus Punkt 7 in der
   Hand diagnostizieren: erste Kennzahl, erstes Werkzeug, erster Verdacht. Bestanden, wenn die
   richtige Ursache in unter zehn Minuten benannt ist und kein Schritt improvisiert werden
   musste. Jeder Schritt, der gefehlt hat, wird ins Runbook nachgetragen.
9. **Die Instrumentierung faengt den Rueckfall.** Nach Punkt 8 den Fehler erneut einbauen —
   diesmal muss die Instrumentierung aus Aufgabe 8 ihn melden, bevor du irgendein
   Diagnosewerkzeug startest: der `Meter`-Wert schlaegt aus oder das Log enthaelt die Zeile.
   Wenn du wieder zum Dump greifen musst, war die Instrumentierung nicht ausreichend.

## Fertig, wenn

Du fuer jedes der fuenf Symptome den Werkzeugbefehl aus dem Kopf tippen kannst.

## Skills

`dotnet-counters`, `dotnet-dump`, `dotnet-trace`, `dotnet-gcdump`, `dotnet-monitor`,
Thread-Pool-Starvation, GC-Verhalten, Dump-Analyse, Runbooks

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
