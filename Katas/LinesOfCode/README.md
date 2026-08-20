# Kata 01_05 — LOC

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/loc/)

## Ziel

Eine Funktion entwickeln, die die Anzahl der Codezeilen in C#-Quelltext zaehlt. Zeilen mit nur Kommentar oder nur Whitespace werden ausgeschlossen.

## Anforderungen

- C# kennt keine geschachtelten Kommentare
- Kommentarzeichen (`//`, `/* */`) oeffnen und schliessen keinen Kommentar innerhalb von Strings
- Strings innerhalb von Kommentaren werden nicht erkannt
- Ausfuehrbarer Code kann in derselben Zeile wie ein Kommentar stehen

## Beispiele und Testfaelle

- `/*a"*/"b...` : der Kommentar endet vor dem `b`, weil `*/` das Ende markiert, obwohl ein stringaehnliches Zeichen darin steht

## Variationen und Randbedingungen

- Zusaetzlich die Anzahl reiner Kommentarzeilen und reiner Whitespace-Zeilen zurueckgeben

---

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

