# Kata 02_09 — Stack

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/stack/)

## Ziel

Einen generischen Stack mit LIFO-Semantik implementieren, der Push und Pop unterstuetzt.

## Anforderungen

- Generischer Typ `IStack<TElement>` mit zwei Methoden
- `Push` legt ein Element auf den Stack
- `Pop` liefert das oberste Element zurueck und entfernt es
- `Pop` auf leerem Stack wirft `InvalidOperationException`

## Beispiele und Testfaelle

- Push ohne Pop testen, Pop ohne Push testen, danach das Zusammenspiel

## Variationen und Randbedingungen

- Isolierte Unit-Tests: Push-Tests duerfen kein Pop verwenden und umgekehrt. Das erzwingt einen Testzugang, der die Implementierung nicht durch sich selbst prueft -- der eigentliche Lernwert dieser Kata.

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

