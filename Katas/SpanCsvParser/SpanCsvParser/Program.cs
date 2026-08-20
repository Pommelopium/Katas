
namespace SpanCsvParser;

/// <summary>
///     Kata 07_03 — Span-basierter CSV-Parser (Stufe 1: Modernes C# und Testbarkeit)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] RFC-4180 vollstaendig: Quoting, escaped Quotes, CRLF im Feld
    // [ ] Version A: string.Split / Substring
    // [ ] Version B: ReadOnlySpan<char>, null Allokationen pro Feld
    // [ ] BenchmarkDotNet ueber eine 50-MB-Datei
    // [ ] IAsyncEnumerable<CsvRow> fuer Streaming
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 07_03");
    }
}
