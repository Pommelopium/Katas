# Kata 02_02 — Bowling

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/bowling/)

## Ziel

Eine Klasse entwickeln, die ein Bowling-Spiel verwaltet und die Punkte nach den Spielregeln inklusive Strikes, Spares und Bonuspunkten berechnet.

## Anforderungen

- Methoden: `AddRoll()`, `Frames()`, `TotalScore()`, `Over()`
- Ein Spiel hat 10 Frames; pro Frame maximal 2 Wuerfe (Ausnahme: Strike und der 10. Frame)
- Punkte normal = Summe der Pins
- Spare = 10 + naechster Wurf
- Strike = 10 + naechste zwei Wuerfe
- Wuerfe nach Spielende loesen eine Exception aus

## Beispiele und Testfaelle

- Perfektes Spiel (12 Strikes) = 300 Punkte
- Alle Wuerfe 0 = 0 Punkte
- Alle Wuerfe 1 = 20 Punkte

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

