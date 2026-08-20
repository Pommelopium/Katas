# Kata 02_01 — Bounded Queue

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/bounded-queue/)

## Ziel

Eine threadsichere Warteschlange mit maximaler Kapazitaet entwickeln, bei der lesende und schreibende Threads sich gegenseitig blockieren, wenn die Queue leer bzw. voll ist.

## Anforderungen

- Klasse `BoundedQueue<T>` mit Konstruktor fuer die Groesse
- `Enqueue()` fuegt ein Element hinzu und blockiert bei voller Queue
- `Dequeue()` entnimmt ein Element und blockiert bei leerer Queue
- `Count()` liefert die aktuelle Elementanzahl
- `Size()` liefert die maximale Kapazitaet

## Beispiele und Testfaelle

- Testfall: N Producer und M Consumer parallel laufen lassen und pruefen, dass kein Element verloren geht oder doppelt entnommen wird

## Variationen und Randbedingungen

- `TryEnqueue()` und `TryDequeue()` mit Timeout-Parameter: `true` bei Erfolg innerhalb der Zeitspanne, sonst `false`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

