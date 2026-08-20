# Kata 06_01 — CSV Viewer

**Refactoring Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://gist.github.com/ccdschool/c97d3c9f5501bf634618)

## Ziel

Ein vorgegebenes C#-Programm zum seitenweisen Anzeigen von CSV-Dateien refaktorieren -- ohne das Verhalten zu aendern.

## Anforderungen

- Ausgangscode aus dem Gist uebernehmen (Link oben) und zuerst durch Characterization Tests absichern
- Erst danach refaktorieren: ohne Testnetz ist das kein Refactoring, sondern Umschreiben

## Beispiele und Testfaelle

- Verbesserungspotenziale im Ausgangscode:
- - unklare Variablennamen wie `iFirstLineOfLastPage`
- - rekursive CSV-Parsing-Logik statt iterativer
- - vermischte Verantwortlichkeiten: Datenlesen, Formatierung und UI in einem
- - wiederholte Logik bei der Pagination
- Funktionalitaet: CSV einlesen, seitenweise anzeigen, Navigation ueber First, Last, Next, Previous, eXit; Spaltenbreiten passen sich den laengsten Eintraegen an

## Variationen und Randbedingungen

- Am Ende gegen deine Loesung der Function Kata CSV tabellieren vergleichen: dieselbe Formatierungsaufgabe, einmal gewachsen und einmal frisch entworfen

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

