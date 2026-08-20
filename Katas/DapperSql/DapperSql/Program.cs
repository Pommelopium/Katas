
namespace DapperSql;

/// <summary>
///     Kata 10_01 — Dapper neben EF Core (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Die drei teuersten Lesequeries aus Kata 09_02 mit Dapper neu geschrieben
    // [ ] Ergebnis direkt in record-DTOs, keine Entitaeten im Lesepfad
    // [ ] Test beweist Parametrisierung; die konkatenierte Gegenprobe ist wieder entfernt
    // [ ] QueryMultipleAsync fuer Liste + Gesamtzahl in einem Roundtrip
    // [ ] Multi-Mapping mit splitOn fuer Kata -> Attempt
    // [ ] DynamicParameters fuer optionale Filter, SARGable geblieben
    // [ ] Stored Procedure mit OUTPUT-Parameter aufgerufen
    // [ ] Gemeinsame Transaktion EF Core + Dapper, Rollback per Test nachgewiesen
    // [ ] Halbe Seite: Wann Dapper, wann EF Core?
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 10_01");
        await Task.CompletedTask;
    }
}
