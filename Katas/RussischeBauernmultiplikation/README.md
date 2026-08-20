# Kata 01_09 — Russische Bauernmultiplikation

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/russische-bauernmultiplikation/)

## Ziel

Eine Funktion implementieren, die zwei ganze Zahlen mit dem Algorithmus der russischen Bauernmultiplikation multipliziert.

## Anforderungen

- Signatur: `int Mul(int x, int y)`
- Die linke Zahl solange halbieren (Nachkommastellen abschneiden), bis 1 erreicht ist
- Die rechte Zahl parallel dazu verdoppeln
- Alle Zeilen streichen, in denen links eine gerade Zahl steht
- Die verbleibenden rechten Zahlen addieren -> Ergebnis

## Beispiele und Testfaelle

- Nachvollziehbar am Beispiel auf der Kata-Seite; teste gegen `x * y` fuer viele Zufallspaare

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

