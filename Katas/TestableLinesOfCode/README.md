# Kata 07_01 — LinesOfCode testbar machen

**Stufe 1: Modernes C# und Testbarkeit** · Zeitrahmen: 2–4 h

## Ausgangslage

`Katas/LinesOfCode/Program.cs` mischt I/O, Parsing und Ausgabe, hat einen relativen Pfad
hart verdrahtet und liefert ein namenloses `(int, int, int)`-Tupel zurueck.

## Aufgabe: der Zeilenzaehler des Kata-Trackers

Der Zaehler ist das erste Werkzeug des **Kata-Trackers**, der dich durch die spaeteren Stufen
begleitet (ab Kata 09_01 als Dienst). Fachlich beantwortet er eine einzige Frage: *Wie gross
ist meine Loesung dieser Kata?* Zu einem eingelesenen Quelltext liefert er die Aufteilung in
Codezeilen, Kommentarzeilen und Leerzeilen — spaeter wird dieser Wert neben Kata-Name und
Dauer als Kennzahl eines Versuchs erfasst. Genau deshalb darf die Analyse nicht mehr am
Dateisystem haengen: Der Tracker uebergibt Quelltext, keine Pfade.

## Ziel

Vom Console-Skript zur unit-getesteten Bibliothek.

## Aufgaben

1. Zwei Projekte: `LinesOfCode.Core` (Klassenbibliothek) und `LinesOfCode.Tests` (xUnit).
2. `record CodeStatistics(int Code, int Comments, int Blank)` statt des Tupels.
3. Interface `ISourceReader` fuer den Dateizugriff. Die Analyse-Klasse bekommt nur einen
   `string` oder `TextReader` — niemals einen Pfad.
4. Tests **zuerst** schreiben, dann die Bugs fixen.
5. `[Theory]` mit `[InlineData]` und `[MemberData]` verwenden.

## Testfaelle, die aktuell fehlschlagen

| Eingabe | Erwartet | Aktuelles Verhalten |
|---|---|---|
| `/* Kommentar */` (einzeilig) | 1 Kommentarzeile | wird als Code gezaehlt |
| `int x = 1; // Hinweis` | 1 Codezeile | ok, aber pruefen |
| `var s = "http://x";` | 1 Codezeile | wird als Kommentar gezaehlt |
| Leerzeile | 1 Blank-Zeile | zaehlt als Blank **und** als Code |
| Datei ohne abschliessendes `*/` | definiertes Verhalten | undefiniert |

## Beispiele und Testfaelle

Zusaetzlich zur Tabelle oben — jeder Fall als `[Fact]` oder `[InlineData]`, der Quelltext
kommt aus einem `string`, nie aus einer Datei:

| Eingabe | Erwartet (`Code`, `Comments`, `Blank`) |
|---|---|
| leerer Quelltext (`""`) | `(0, 0, 0)` |
| `int x = 1;` | `(1, 0, 0)` |
| dreizeiliger Block `/*`, ` * Text`, `*/` | `(0, 3, 0)` |
| zwei Zeilen `/// <summary>` und `/// Text` | `(0, 2, 0)` |
| Zeile nur aus Tabs und Leerzeichen | `(0, 0, 1)` |
| `} /* Ende */` (Code mit angehaengtem Blockkommentar) | `(1, 0, 0)` |
| Datei mit `/*` ohne `*/` und drei Folgezeilen | alle Restzeilen als `Comments`, keine Exception |

Zwei Eigenschaften gelten fuer **jede** Eingabe und gehoeren als eigene Tests dazu:

- `Code + Comments + Blank` ist gleich der Zeilenzahl der Eingabe — keine Zeile wird
  doppelt oder gar nicht gezaehlt.
- Dieselbe Eingabe liefert bei mehrfachem Aufruf dasselbe Ergebnis (kein Zustand zwischen
  Analysen, z. B. ein haengengebliebenes `isMultiLineComment`).

Als Ende-zu-Ende-Nachweis analysierst du `Katas/RomanNumerals/Program.cs` ueber einen
Test-Doppel des `ISourceReader` und haeltst die drei Zahlen als erwarteten Wert fest.

## Fertig, wenn

Alle Tests gruen sind und die Analyse-Klasse **keinen** `using System.IO`-Dateizugriff
mehr enthaelt.

## Skills

xUnit, TDD, Dependency Inversion, Records, Nullable Reference Types

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
