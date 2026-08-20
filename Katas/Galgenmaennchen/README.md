# Kata 02_04 — Galgenmaennchen

**Class Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/class-katas/galgenmaennchen/)

## Ziel

Eine Klasse implementieren, die das Ratespiel Galgenmaennchen abbildet und nach jedem geratenen Buchstaben den aktuellen Loesungsstand zurueckgibt.

## Anforderungen

- Der Konstruktor nimmt das zu ratende Wort als String entgegen
- `RateBuchstabe()` akzeptiert ein einzelnes Zeichen
- Prueft, an welchen Positionen der Buchstabe im Wort vorkommt
- Gibt den Loesungsstand zurueck: erkannte Buchstaben im Klartext, unbekannte als Bindestrich

## Beispiele und Testfaelle

- Gesuchtes Wort `Developer`:
- `RateBuchstabe('e')` -> `-e-e---e-`
- `RateBuchstabe('o')` -> `-e-e-o-e-`
- `RateBuchstabe('d')` -> `De-e-o-er`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

