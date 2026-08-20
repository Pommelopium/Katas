# Kata 02_07 — Priority Queue

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/priority-queue/)

## Ziel

Eine generische Warteschlange implementieren, in der Elemente nach Prioritaet sortiert sind und hoeher priorisierte vor niedriger priorisierten entnommen werden.

## Anforderungen

- `Enqueue(T element, int priority)` fuegt ein Element mit Prioritaet hinzu
- `Dequeue()` entfernt und liefert das Element mit der hoechsten Prioritaet
- `Count()` liefert die Anzahl der Elemente
- Elemente gleicher Prioritaet werden in Eingangsreihenfolge entnommen (stabile FIFO-Semantik)

## Beispiele und Testfaelle

- Stabilitaet ist der interessante Testfall: drei Elemente mit gleicher Prioritaet muessen in der Reihenfolge herauskommen, in der sie hineingegangen sind

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

