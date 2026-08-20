# Kata 14_21 — Schablonenmethode (Template Method)

**Verhaltensmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/template-method)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und dann **richtig anwenden** — nicht: es ueberall anwenden. Template
Method loest genau ein Problem: mehrere Klassen durchlaufen denselben Ablauf in derselben
Reihenfolge und unterscheiden sich nur in einzelnen Schritten. Die Schablone haelt die
Reihenfolge fest, die Unterklassen fuellen die Luecken. Wer das Muster verstanden hat, sieht
auch die Grenze dahinter: die Vererbungsfalle, in der jede neue Anforderung eine weitere
Ebene in die Hierarchie zwingt und am Ende niemand mehr sagen kann, welche Methode wann
laeuft.

## Woran du das Muster erkennst

- **Zwei oder drei Klassen mit fast identischem Ablauf:** stellst du die Methoden
  nebeneinander, stimmen sie Zeile fuer Zeile ueberein — bis auf zwei Schritte in der Mitte.
- **Copy-Paste zwischen Importern oder Reports:** die zweite Klasse ist erkennbar aus der
  ersten entstanden, inklusive derselben Kommentare und desselben Tippfehlers in der
  Log-Meldung.
- **Ein Bugfix muss doppelt gemacht werden:** die Korrektur am Zaehlen der abgelehnten
  Datensaetze gehoert in beide Klassen, und beim zweiten Mal vergisst sie jemand.
- **Der Ablauf selbst ist die Regel:** "erst pruefen, dann abbilden, dann speichern, dann
  protokollieren" ist fachlich vorgeschrieben und soll ausdruecklich **nicht** veraenderbar
  sein — auch nicht von einer Unterklasse.
- Die Unterschiede sind **benennbar und wenige:** man kann in einem Satz sagen, was die
  Varianten trennt ("die eine liest CSV, die andere JSON"), und der Rest ist gleich.

## Aufgabe: der Versuchsimport im Kata-Tracker

Der **Kata-Tracker** soll erfasste Uebungsversuche aus Fremdsystemen einlesen. Fachlich ist der
Ablauf fuer jedes Format gleich und in dieser Reihenfolge vorgeschrieben:

1. Quelle oeffnen und Kopf pruefen (leere Quelle ist ein Fehler, kein leerer Import).
2. Rohdatensaetze lesen.
3. Jeden Datensatz auf `Attempt` abbilden (Kata-Kuerzel, Datum, Dauer in Minuten).
4. Jeden `Attempt` gegen die Regeln pruefen (Kuerzel bekannt, Dauer 1 bis 480, Datum nicht in
   der Zukunft).
5. Gueltige speichern, abgelehnte mit Grund sammeln.
6. Ergebnisbericht schreiben: gelesen, uebernommen, abgelehnt.

Es gibt zwei Formate: **CSV** (Semikolon, Kopfzeile) und **JSON** (Array von Objekten). Spaeter
kommt ein drittes hinzu. Heute steht der Ablauf zweimal im Code:

```csharp
public sealed class CsvAttemptImporter
{
    public ImportReport Import(Stream quelle)
    {
        var zeilen = new StreamReader(quelle).ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (zeilen.Length == 0) throw new ImportException("import.empty_source");

        var uebernommen = 0;
        var abgelehnt = new List<string>();
        foreach (var zeile in zeilen.Skip(1))                  // Kopfzeile ueberspringen
        {
            var felder = zeile.Split(';');                     // --- Unterschied ---
            var versuch = new Attempt(felder[0].Trim().ToUpperInvariant(),
                DateOnly.Parse(felder[1]), int.Parse(felder[2]));
            var fehler = _regeln.Pruefe(versuch);
            if (fehler is not null) { abgelehnt.Add(fehler); continue; }
            _speicher.Add(versuch);
            uebernommen++;
        }

        _protokoll.Info($"CSV: {zeilen.Length - 1} gelesen, {uebernommen} uebernommen");
        return new ImportReport(zeilen.Length - 1, uebernommen, abgelehnt);
    }
}

public sealed class JsonAttemptImporter
{
    public ImportReport Import(Stream quelle)
    {
        var knoten = JsonDocument.Parse(quelle).RootElement;
        if (knoten.GetArrayLength() == 0) throw new ImportException("import.empty_source");

        var uebernommen = 0;
        var abgelehnt = new List<string>();
        foreach (var eintrag in knoten.EnumerateArray())
        {
            var versuch = new Attempt(                          // --- Unterschied ---
                eintrag.GetProperty("kata").GetString()!.Trim().ToUpperInvariant(),
                DateOnly.Parse(eintrag.GetProperty("datum").GetString()!),
                eintrag.GetProperty("dauer").GetInt32());
            var fehler = _regeln.Pruefe(versuch);
            if (fehler is not null) { abgelehnt.Add(fehler); continue; }
            _speicher.Add(versuch);
            uebernommen++;
        }

        _protokoll.Info($"JSON: {knoten.GetArrayLength()} gelesen, {uebernommen} uebernommen");
        return new ImportReport(knoten.GetArrayLength(), uebernommen, abgelehnt);
    }
}
```

Das ist der Zustand, den du erkennen sollst: zwei Klassen, die zu etwa 80 Prozent Zeile fuer
Zeile identisch sind. Der echte Unterschied sind **zwei** Schritte — Rohdatensaetze lesen und
einen Rohdatensatz auf `Attempt` abbilden. Alles andere ist Kopie, und die Kopie ist schon
auseinandergelaufen: `ToUpperInvariant` steht in beiden, `Trim` beim Datum nur in einer.

## Aufgaben

1. Bau den Ausgangszustand nach und schreib eine **gemeinsame Testsuite**, die beide Importer
   gegen dieselben fachlichen Faelle prueft (dieselben Testmethoden, nur ein anderes
   Eingabeformat). Sie muss nach dem Umbau unveraendert gruen bleiben.
2. Zieh die Basisklasse `AttemptImporter` und mach `Import` zur **Schablone**: die Reihenfolge
   der sechs Schritte steht genau einmal, und eine Unterklasse kann sie nicht aendern. In C#
   heisst das: `Import` ist **nicht** `virtual` (bei einer Ueberschreibung tiefer in der
   Hierarchie: `sealed override`). Halte in einem Kommentar fest, warum `new` in einer
   Unterklasse das Muster bricht, statt es zu erweitern.
3. Bestimme die **abstrakten Schritte** — genau die, die jede Variante liefern *muss*:
   `ReadRecords(Stream)` und `MapRecord(RawRecord)`. Wird eine Variante hinzugefuegt, die einen
   davon nicht braucht, hast du den Schnitt falsch gelegt.
4. Bestimme die **Hooks** — Schritte mit Standardverhalten, die eine Variante ueberschreiben
   *kann*: `NormalizeKataCode` (Standard: `Trim` + Grossschreibung), `OnRecordRejected`
   (Standard: sammeln), `AfterImport` (Standard: Bericht ins Protokoll). Ein Hook ohne
   sinnvollen Standard ist kein Hook, sondern ein abstrakter Schritt.
5. Setz die **Sichtbarkeit** bewusst: `public` nur fuer `Import`, alle Schritte `protected`
   (abstrakt) bzw. `protected virtual` (Hooks), Hilfsmethoden `private`. Ein Test darf einen
   Schritt nicht direkt aufrufen koennen — geprueft wird der Ablauf, nicht das Einzelteil.
6. Ergaenze eine **dritte Variante**: Import aus einer **Markdown-Tabelle**
   (`| 14_21 | 2026-08-19 | 95 |`). Regel: die beiden bestehenden Importer und die Basisklasse
   werden dafuer **nicht** angefasst, und die Testsuite aus Schritt 1 laeuft unveraendert
   durch — nur mit der neuen Eingabe.
7. Lass die dritte Variante **einen Hook** ueberschreiben (die Markdown-Quelle liefert Kuerzel
   mit Rahmenzeichen) und einen **nicht** (der Bericht bleibt der Standard). Zeig per Test,
   dass beides greift.
8. **Gegenprobe:** loese dieselbe Aufgabe ein zweites Mal mit **Strategy** statt Vererbung —
   ein `AttemptImportPipeline` mit `IRecordReader` und `IRecordMapper` als eingehaengte
   Abhaengigkeiten, ohne eine einzige Unterklasse. Halte schriftlich fest: Zeilen Code,
   Anzahl Typen, wo die Reihenfolge jeweils festgenagelt ist, was zur Kompilierzeit gegen was
   zur Laufzeit gebunden wird, und welche Loesung du bei einem vierten Format nimmst. Zwei
   Absaetze, mit Entscheidung am Ende.

## Beispiele und Testfaelle

| Fall | Erwartetes Ergebnis |
|---|---|
| Schrittfolge protokollieren: eine Testunterklasse schreibt jeden Schritt in eine Liste | fuer **alle drei** Varianten dieselbe Liste: `[CheckSource, ReadRecords, NormalizeKataCode, MapRecord, Validate, Store, AfterImport]` — MapRecord-bis-Store je Datensatz |
| CSV, JSON und Markdown mit **denselben drei** gueltigen Versuchen | drei identische `ImportReport` (3 gelesen, 3 uebernommen, 0 abgelehnt) und drei identische Speicherinhalte |
| Datensatz mit Dauer `0` und einer mit `481`, dazwischen ein gueltiger | 3 gelesen, 1 uebernommen, 2 abgelehnt mit Code `attempt.duration_out_of_range`; die Grenzen `1` und `480` sind gueltig |
| Variante ueberschreibt `AfterImport` **nicht** (Aufgabe 7) | das Standardverhalten greift: der Bericht landet im Protokoll, Wortlaut identisch zu den anderen Varianten — nachgewiesen mit einem Fake-Protokoll |
| Markdown-Variante ueberschreibt `NormalizeKataCode` | `\| 14_21 \|` wird zu `14_21`; die CSV-Variante erhaelt dieselbe Eingabe ohne Rahmenzeichen und bleibt beim Standard |
| Leere Quelle (0 Bytes, leeres JSON-Array, Tabelle nur mit Kopf) | fuer alle Varianten `ImportException` mit `import.empty_source` — kein Bericht mit 0 Datensaetzen |
| Unterklasse versucht, den Ablauf umzustellen (Speichern vor Pruefen) | geht nicht: `Import` ist nicht ueberschreibbar, der Versuch **kompiliert nicht**. Der Fall gehoert als Kommentar mit der Compiler-Meldung (`CS0239`/`CS0506`) in die Testklasse, nicht als Test. Wo eine Sprache das nicht verhindert, wird die Grenze dokumentiert und geprueft |
| Dritte Variante gegen die gemeinsame Testsuite (Aufgabe 6) | jeder Testfall besteht **unveraendert**; `git diff` beruehrt nur die neue Klasse und die Testdaten, nicht Basisklasse und nicht die beiden alten Importer |
| Strategy-Gegenprobe (Aufgabe 8) | dieselbe Testsuite laeuft auch gegen `AttemptImportPipeline`; Reader und Mapper sind zur **Laufzeit** austauschbar (ein Test tauscht den Mapper am fertigen Objekt) — bei der Schablone ist genau das nicht moeglich |

## Abgrenzung

- **Strategy** (Kata 14_20) loest dasselbe Problem mit Komposition: dort wird ein *ganzer*
  Algorithmus zur **Laufzeit** ausgetauscht, hier werden *Schritte* eines feststehenden
  Ablaufs zur **Kompilierzeit** durch Vererbung gebunden. Template Method ist billiger,
  solange die Varianten fest sind; Strategy gewinnt, sobald etwas zur Laufzeit wechseln oder
  mehrfach kombiniert werden soll.
- **Factory Method** (Kata 14_02) ist haeufig **ein Schritt in einer Schablone**: die
  abstrakte Methode, die das passende Objekt erzeugt. Wer nur `MapRecord` betrachtet, sieht
  eine Factory Method; wer `Import` betrachtet, sieht die Schablone. Beide Namen fuer denselben
  Code sind kein Widerspruch, sondern verschiedene Ausschnitte.
- **Decorator** (Kata 14_09) erweitert Verhalten **um** einen Aufruf herum, ohne die Klasse zu
  kennen, und ist beliebig stapelbar. Die Schablone erweitert **innerhalb** eines Ablaufs an
  vorgegebenen Stellen und ist nicht stapelbar — zwei Unterklassen kann man nicht
  uebereinanderlegen.

## Wann nicht

- **Tiefe Hierarchien:** ab der dritten Ebene weiss niemand mehr, welche Ueberschreibung
  greift, und das "Hollywood-Prinzip" ("ruf uns nicht an, wir rufen dich an") kippt von einer
  Zusage in ein Raetsel. Eine Ebene abstrakt, eine Ebene konkret — mehr nicht.
- **Wenn sich die Varianten in mehr als zwei Schritten unterscheiden**, ist die
  Gemeinsamkeit nur noch die Reihenfolge. Dann ist Strategy besser: eingehaengte Teile statt
  einer Klasse pro Kombination — bei drei variablen Schritten mit je zwei Auspraegungen
  braeuchte Vererbung acht Unterklassen.
- **Im Zweifel schlaegt Komposition die Vererbung:** die Basisklasse ist eine Kopplung, die man
  nicht mehr los wird, und sie verbraucht in C# den einzigen Basisklassen-Platz der
  Unterklasse. Fang mit eingehaengten Abhaengigkeiten an und zieh die Schablone erst, wenn die
  Duplikation belegt ist.

## Skills

Verhaltensmuster erkennen, Duplikation in fast identischen Ablaeufen aufloesen, invariante
Reihenfolge festnageln (`sealed`, nicht `virtual`), abstrakte Schritte gegen optionale Hooks
abgrenzen, Sichtbarkeit als Vertrag (`protected`), Schrittfolge beobachtbar testen, gemeinsame
Testsuite fuer mehrere Varianten, Vererbung gegen Komposition abwaegen, Abgrenzung zu Strategy,
Factory Method und Decorator

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
