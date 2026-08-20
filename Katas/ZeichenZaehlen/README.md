# Kata 01_14 — Zeichen zaehlen

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/zeichen-zaehlen/)

## Ziel

Eine Funktion entwickeln, die die Haeufigkeit jedes Zeichens in einem String zaehlt und als Dictionary zurueckgibt.

## Anforderungen

- Eingabe: String, Ausgabe: `IDictionary<char, int>`
- Gross- und Kleinbuchstaben werden unterschieden
- Leerzeichen werden mitgezaehlt

## Beispiele und Testfaelle

- Eingabe `Das darf nicht sein` -> D:1, a:2, s:2, ' ':3, d:1, r:1, f:1, n:2, i:2, c:1, h:1, e:1, t:1

## Variationen und Randbedingungen

- Gross- und Kleinschreibung vereinheitlichen -> d:2, a:2, s:2, ' ':3, r:1, f:1, n:2, i:2, c:1, h:1, e:1, t:1

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

