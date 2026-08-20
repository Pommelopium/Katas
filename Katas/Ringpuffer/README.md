# Kata 02_08 — Ringpuffer

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/ringpuffer/)

## Ziel

Eine generische Klasse entwickeln, die einen Ringpuffer implementiert: eine Datenstruktur fester Kapazitaet, in der neue Werte alte ueberschreiben.

## Anforderungen

- Konstruktor mit Groessenparameter
- `Add(T value)` fuegt hinten an
- `Take()` entnimmt vorne
- `Count()` liefert die Anzahl ungelesener Elemente
- `Size()` liefert die Kapazitaet
- Bei vollem Puffer werden die aeltesten Werte ueberschrieben

## Beispiele und Testfaelle

- Puffer der Groesse 3, `Add(1) Add(2) Add(3) Add(4)` -> `Take()` liefert 2, nicht 1

## Variationen und Randbedingungen

- Der Benutzer waehlt, ob still ueberschrieben oder eine Exception geworfen wird -- ueberlege, wie sich diese Option am saubersten abbilden laesst

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

