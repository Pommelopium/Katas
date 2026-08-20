# Kata 01_11 — Textumbruch

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/textumbruch/)

## Ziel

Eine Funktion implementieren, die Text an Wortgrenzen umbricht, sodass eine maximale Zeilenlaenge nicht ueberschritten wird.

## Anforderungen

- Woerter mit Leerzeichen verbinden, bis die maximale Zeilenlaenge erreicht ist
- Passt ein Wort nicht mehr in die Zeile, kommt es in die naechste
- Woerter, die laenger als die maximale Zeilenlaenge sind, werden abgeschnitten und der Rest in die naechste Zeile uebernommen

## Beispiele und Testfaelle

- Gedichttext von Loriot, maximale Zeilenlaenge 9: Umbruch u. a. bei `Schneefloe/cklein`
- Derselbe Text mit maximaler Zeilenlaenge 14: `Es blaut die / Nacht, die / Sternlein...`

## Variationen und Randbedingungen

- Blocksatz: die Leerzeichen gleichmaessig ueber die Zeile verteilen, z. B. `Es  blaut  die  Nacht,`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

