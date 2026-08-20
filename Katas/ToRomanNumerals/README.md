# Kata 01_13 — To Roman Numerals

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/to-roman-numerals/)

## Ziel

Eine Funktion schreiben, die Dezimalzahlen in roemische Zahlen uebersetzt. Gegenstueck zu deinem bestehenden Projekt `RomanNumerals` (From Roman Numerals).

## Anforderungen

- Wertebereich 1 bis 3000 korrekt konvertieren
- Subtraktionsregel korrekt anwenden (IV, IX, XL, XC, CD, CM)

## Beispiele und Testfaelle

- 1 -> `I`
- 2 -> `II`
- 4 -> `IV`
- 5 -> `V`
- 9 -> `IX`
- 10 -> `X`
- 42 -> `XLII`
- 99 -> `XCIX`
- 2013 -> `MMXIII`

## Variationen und Randbedingungen

- Round-Trip-Test gegen `RomanNumerals.Parse`: fuer jedes n in 1..3000 muss `Parse(ToRoman(n)) == n` gelten

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

