# Kata 02_06 — Ordered Jobs

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/ordered-jobs/)

## Ziel

Eine Klasse entwickeln, die Jobs mit gegenseitigen Abhaengigkeiten verwaltet und in eine gueltige Abarbeitungsreihenfolge bringt (topologische Sortierung).

## Anforderungen

- Zwei Register-Methoden: eine fuer einzelne Jobs, eine fuer Abhaengigkeitspaare
- `Sort()` gibt die geordnete Jobreihenfolge als String zurueck
- Doppelt registrierte Jobs erscheinen nur einmal in der Ausgabe
- Unabhaengige Jobs duerfen in beliebiger Reihenfolge stehen, solange alle Abhaengigkeiten erfuellt sind
- Zirkulaere Abhaengigkeiten muessen per Exception signalisiert werden

## Beispiele und Testfaelle

- Registrierungen `c`, `b -> a`, `c -> b` ergeben `abc`

## Variationen und Randbedingungen

- `Sort(string)` mit mehrzeiligem Registrierungsformat als Eingabe

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

