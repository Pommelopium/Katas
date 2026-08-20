# Kata 14_01 — Fabrikmethode (Factory Method)

**Erzeugungsmuster** aus dem [Refactoring.Guru-Katalog](https://refactoring.guru/design-patterns/catalog) | [Original-Beschreibung](https://refactoring.guru/design-patterns/factory-method)

**Design-Pattern-Kata** · Zeitrahmen: 2-4 h

## Ziel

Das Muster **erkennen** und **richtig anwenden** — nicht: es ueberall anwenden. Factory Method
ist geuebt, wenn du in fremdem Code die Stelle findest, an der ein fester Ablauf und eine
variable Erzeugung verklebt sind, und wenn du danach eine neue Variante ergaenzen kannst, ohne
eine einzige bestehende Klasse anzufassen. Wer das Muster praeventiv einbaut, hat die Kata
nicht bestanden — auch wenn das Klassendiagramm hinterher lehrbuchmaessig aussieht.

## Woran du das Muster erkennst

- Eine Methode enthaelt eine **`switch`- oder `if`-Kaskade ueber einen Typ-, Format- oder
  Enum-Wert**, und in jedem Zweig steht ein `new` einer anderen Klasse. Jede neue Variante
  bedeutet: dieselbe Methode wieder anfassen.
- Rund um die Kaskade steht **immer derselbe Ablauf** (Kopf schreiben, Zeilen schreiben, Fuss
  schreiben, abschliessen) — nur ein Teilschritt unterscheidet sich. Der Ablauf ist stabil, die
  Erzeugung ist es nicht.
- Der Ablauf ist **kopiert statt geteilt**: drei Methoden, die zu 80 Prozent gleich sind und sich
  nur im erzeugten Objekt unterscheiden. Ein Bugfix im Ablauf muss dreimal gemacht werden.
- Eine Klasse mit fachlicher Logik **kennt konkrete Implementierungen namentlich**, weil sie sie
  selbst per `new` erzeugt. Der Test kann sie deshalb nicht ersetzen.
- Eine Basisklasse oder ein Framework legt einen Ablauf fest, aber **die Unterklasse weiss
  besser, womit** er ausgefuehrt werden soll — und du bist versucht, das per Konstruktorflag
  oder Enum von aussen hereinzureichen.

## Aufgabe: Der Berichtsexport des Kata-Trackers

Der **Kata-Tracker** soll einen Trainingsbericht ausgeben: Kopfzeile mit Zeitraum, eine Zeile
pro Versuch (Kata-Kuerzel, Datum, Dauer), Fusszeile mit Gesamtdauer. Die Formate sind Markdown
und CSV, demnaechst kommt JSON dazu. Der heutige Stand sieht so aus:

```csharp
public sealed class ReportExporter
{
    public string Export(string format, Report report)
    {
        var sb = new StringBuilder();

        switch (format)
        {
            case "markdown":
                sb.AppendLine($"# Training {report.From:d} - {report.To:d}");
                foreach (var a in report.Attempts)
                    sb.AppendLine($"- {a.KataId} | {a.Date:d} | {a.Minutes} min");
                sb.AppendLine($"**Gesamt: {report.TotalMinutes} min**");
                break;

            case "csv":
                sb.AppendLine("kata;datum;minuten");
                foreach (var a in report.Attempts)
                    sb.AppendLine($"{a.KataId};{a.Date:yyyy-MM-dd};{a.Minutes}");
                sb.AppendLine($";;{report.TotalMinutes}");
                break;

            default:
                throw new ArgumentException($"Unbekanntes Format: {format}");
        }

        return sb.ToString();
    }
}
```

Der Schmerz ist bewusst klein und typisch: der **Ablauf** (Kopf, Zeilen, Fuss) ist in beiden
Zweigen identisch, nur das **Schreiben** unterscheidet sich. Er steht zweimal kopiert da, das
Format wird ueber einen String gesteuert, und JSON bedeutet: dieselbe Methode wieder aufmachen.
Genau dieser Codeblock ist das, was du in fremdem Code erkennen sollst.

## Aufgaben

1. Schreib den Ausgangscode oben ab und sichere ihn mit Tests fuer beide Formate ab. Ohne
   gruenes Netz ist alles Folgende Umbau auf Verdacht.
2. Trenne **Ablauf** von **Variation**: markiere im Code, welche Zeilen in beiden Zweigen gleich
   sind und welche nicht. Das Ergebnis dieser Markierung ist der Bauplan fuer alles Weitere.
3. Zieh die Variation hinter eine Schnittstelle: `IReportWriter` mit `WriteHeader(Report)`,
   `WriteLine(Attempt)`, `WriteFooter(Report)`, `string Result()`. Implementiere
   `MarkdownReportWriter` und `CsvReportWriter`.
4. Bau den **Creator**: eine abstrakte Klasse `ReportExporter` mit einem nicht virtuellen
   `Export(Report)`, das den Ablauf genau einmal enthaelt, und einem
   `protected abstract IReportWriter CreateWriter()`. Das ist die Fabrikmethode — nicht das
   `switch`, das du gerade loeschst.
5. Leite `MarkdownReportExporter` und `CsvReportExporter` ab. Jede Unterklasse enthaelt genau
   eine Zeile Code. Das `switch` und der String `format` verschwinden aus dem Ablauf.
6. **Der Beweis:** ergaenze `JsonReportExporter` samt `JsonReportWriter` in neuen Dateien — ohne
   `ReportExporter`, `MarkdownReportExporter`, `CsvReportExporter` oder einen bestehenden Test
   anzufassen. Der Diff dieses Schritts darf nur neue Dateien enthalten. Notier, welche Zeilen du
   im Ausgangscode haettest aendern muessen.
7. Verschieb die Auswahl an den Rand: eine Zuordnung von Formatnamen auf Exporter (Dictionary
   oder DI-Registrierung) lebt in der Konfiguration, nicht im Ablauf. Ein unbekanntes Format
   scheitert dort, nicht mitten im Export.
8. Optional, als Gegenprobe: bau die Loesung noch einmal mit `Func<IReportWriter>` im
   Konstruktor statt mit Vererbung. Halt in drei Saetzen fest, welche Variante du in einem
   echten Projekt nehmen wuerdest und warum — das ist die eigentliche Lernausgabe.

## Beispiele und Testfaelle

Bericht `R1`: Zeitraum `2026-08-01` bis `2026-08-31`, zwei Versuche
(`07_02 / 2026-08-19 / 95`) und (`14_01 / 2026-08-20 / 120`), Gesamtdauer 215.

| Eingabe | Erwartetes Ergebnis |
|---|---|
| `new MarkdownReportExporter().Export(R1)` | 4 Zeilen; erste `# Training 01.08.2026 - 31.08.2026`, letzte `**Gesamt: 215 min**` |
| `new CsvReportExporter().Export(R1)` | 4 Zeilen; erste `kata;datum;minuten`, zweite `07_02;2026-08-19;95` |
| Bericht ohne Versuche | beide Exporter liefern **nur** Kopf- und Fusszeile, Gesamtdauer 0 — kein leerer String, keine Exception |
| Reihenfolge der Aufrufe (Writer-Spion protokolliert) | genau `Header`, `Line`, `Line`, `Footer`, `Result` — fuer **jeden** Exporter identisch |
| `MarkdownReportExporter.CreateWriter()` zweimal aufgerufen | zwei **verschiedene** Instanzen — der Writer ist zustandsbehaftet und darf nicht geteilt werden |
| `new JsonReportExporter().Export(R1)` | gueltiges JSON mit Array aus 2 Elementen und Feld `totalMinutes: 215` |
| **Erweiterungsnachweis:** Testlauf nach Aufgabe 6 | alle Tests von Markdown und CSV bleiben gruen und **unveraendert**; `git diff --stat` des Schritts zeigt ausschliesslich neue Dateien |
| Zuordnung `"xml"` -> Exporter (Aufgabe 7) | Fehler bei der Aufloesung am Rand, mit sprechender Meldung — der Ablauf selbst kennt keine Formatnamen mehr |

## Abgrenzung

- **Abstract Factory:** dort erzeugt eine Fabrik eine ganze **Produktfamilie**, die zusammen
  passen muss (Writer + Escaper + Dateiendung). Factory Method ist **eine** Methode fuer **ein**
  Produkt, und der Creator hat neben dem Erzeugen noch fachliche Arbeit zu tun. Frag: eine
  Methode oder mehrere, die zusammenpassen muessen?
- **Builder:** loest ein anderes Problem — **wie** ein einzelnes komplexes Objekt schrittweise
  zusammengesetzt wird, nicht **welche** von mehreren Klassen entsteht. Bei Builder gibt es einen
  Produkttyp und viele Konfigurationen, bei Factory Method mehrere Produkttypen.
- **Strategy:** sieht im Klassendiagramm fast gleich aus, aber die Unterklasse liefert dort
  **Verhalten**, hier ein **Objekt**. Merkmal: bei Strategy wird die Variante von aussen
  hereingegeben und kann zur Laufzeit wechseln, bei Factory Method entscheidet die Unterklasse
  selbst.
- **Die statische `Create(...)`-Methode** (`Result.Success`, `Task.FromResult`) ist eine
  praktische Konvention, aber **kein** Muster: es gibt keine Vererbung, keinen Erweiterungspunkt,
  keinen Polymorphismus. Sie loest kein Kopplungsproblem und zaehlt in dieser Kata nicht.

## Wann nicht

- **Es gibt nur eine Variante** und keinen konkreten Plan fuer die zweite. Dann ist `new` an der
  Aufrufstelle die richtige Loesung; das Muster kostet zwei Klassen und liefert nichts.
- **Die Variation ist reine Erzeugung ohne festen Ablauf drumherum.** In C# reicht dafuer der
  **DI-Container** (registriere `IReportWriter` pro Format), ein `Func<IReportWriter>` im
  Konstruktor oder ein `Dictionary<string, Func<IReportWriter>>`. Vererbung nur fuer ein `new`
  ist Ueberbau — und `Func<T>` ist im Test in einer Zeile ersetzt.
- **Der Typ steht zur Compilezeit fest.** Dann loest **Generics** (`Exporter<TWriter> where
  TWriter : IReportWriter, new()`) dasselbe ohne Klassenhierarchie, mit Typpruefung statt
  Laufzeitaufloesung.

## Skills

Factory Method, Abhaengigkeitsumkehr, Open-Closed-Prinzip, Template Method als Nachbar,
Erkennen von switch-Kaskaden, Refactoring unter gruenen Tests, Abgrenzung von Erzeugungsmustern

---

Arbeitsweise: Testfaelle zuerst, dann die Implementierung. Die Kata ist fertig, wenn die
Beispiele oben als automatisierte Tests gruen sind -- nicht, wenn die Konsolenausgabe stimmt.

Siehe auch: [KATA-ROADMAP.md](../../KATA-ROADMAP.md) fuer die empfohlene Lernreihenfolge.
