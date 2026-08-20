
namespace TemplateMethodPattern;

/// <summary>
///     Kata 14_21 — Schablonenmethode (Template Method) (Verhaltensmuster aus dem Refactoring.Guru-Katalog)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Ausgangszustand: CsvAttemptImporter und JsonAttemptImporter, zu 80 Prozent identisch
    // [ ] AttemptImporter mit Import als Schablone — sealed, Reihenfolge nicht ueberschreibbar
    // [ ] abstrakte Schritte: ReadRecords, MapRecord
    // [ ] Hooks mit Standardverhalten: NormalizeKataCode, OnRecordRejected, AfterImport
    // [ ] Sichtbarkeit: protected fuer Schritte, public nur fuer Import
    // [ ] dritte Variante (Markdown-Tabelle) besteht die gemeinsame Testsuite unveraendert
    // [ ] Protokoll der Schrittfolge als beobachtbare Liste, fuer alle Varianten gleich
    // [ ] Gegenprobe: dieselbe Aufgabe mit Strategy statt Vererbung, Vergleich schriftlich
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 14_21");
    }
}
