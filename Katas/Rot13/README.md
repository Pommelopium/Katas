# Kata 01_08 — ROT-13

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/rot-13/)

## Ziel

Eine Funktion schreiben, die Text mit dem ROT-13-Verfahren verschluesselt: jeder Buchstabe wird durch den ersetzt, der 13 Stellen weiter hinten im Alphabet liegt.

## Anforderungen

- Buchstaben um 13 Positionen verschieben, mit Wrap-Around am Alphabetende
- Alle Buchstaben in Grossbuchstaben umwandeln
- Umlaute ersetzen: OE, AE, UE, SS
- Nicht-Buchstaben unveraendert lassen

## Beispiele und Testfaelle

- `Hello, World` -> `URYYB, JBEYQ`
- Mit Variation 2: `0` mit Versatz 13 -> `D`; `Z` -> `C`

## Variationen und Randbedingungen

- Den Versatz variabel gestalten statt fest 13
- Ziffern mitverschluesseln: 0-9 und A-Z als ein einheitliches Alphabet behandeln

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

