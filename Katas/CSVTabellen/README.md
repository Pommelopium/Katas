# Kata 01_01 — CSV tabellieren

**Function Kata** aus dem [CCD Coding Dojo](https://ccd-school.de/coding-dojo/) | [Original-Aufgabenstellung](https://ccd-school.de/coding-dojo/function-katas/csv-tabellieren/)

## Ziel

Eine Funktion entwickeln, die CSV-Zeilen in eine formatierte Texttabelle umwandelt.

## Anforderungen

- Eingabe: `IEnumerable<string>` mit CSV-Zeilen, Semikolon als Trennzeichen
- Erste Zeile ist die Ueberschrift
- Ueberschrift durch eine Trennzeile von den Daten separieren
- Spaltenbreite richtet sich nach dem breitesten Wert der Spalte (inklusive Ueberschrift)
- Keine komplexen CSV-Mechanismen noetig (kein Semikolon in Daten)
- Keine Fehlerbehandlung erforderlich

## Beispiele und Testfaelle

- Eingabe: 4 Zeilen mit Name, Strasse, Ort, Alter
- Ausgabe: Tabellenformat mit `|` als Spaltentrenner und einer Kopfzeilen-Trennlinie aus `-` und `+`

---

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.

