# Kata 01_02 — File Path Helper

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/en/coding-dojo/function-katas/file-path-helper/)

## Ziel

Eine Funktion entwickeln, die relative Dateipfade in absolute Pfade umwandelt und dabei die Platzhalter `~`, `.` und `..` aufloest.

## Anforderungen

- Relative Pfade erkennen und zu absoluten Pfaden umwandeln
- `~` = Heimverzeichnis, `.` = aktuelles Verzeichnis, `..` = uebergeordnetes Verzeichnis
- Betriebssystemspezifische Trennzeichen beruecksichtigen (`/` unter Linux/macOS, `\` unter Windows)
- Windows-Laufwerksbuchstaben (z. B. `c:`) beibehalten
- Im Ergebnis duerfen keine Platzhalter mehr vorkommen

## Beispiele und Testfaelle

- `~/Downloads/mountains.jpg` -> `/Users/brucew/Downloads/mountains.jpg`
- `./bin/debug/samples/config.json` -> `/Users/brucew/Projects/ETF/bin/debug/samples/config.json`
- `/Users/brucew/Projects/ETF/bin/../program.cs` -> `/Users/brucew/Projects/ETF/program.cs`

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

