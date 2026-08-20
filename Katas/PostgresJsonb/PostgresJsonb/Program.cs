
namespace PostgresJsonb;

/// <summary>
///     Kata 10_05 — Zweiter Provider: PostgreSQL und JSONB
///     (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Modell aus Kata 09_02 laeuft auf PostgreSQL, Provider per Konfiguration umschaltbar
    // [ ] Liste aller Stellen, die der Providerwechsel angefasst hat
    // [ ] Getrennte Migrations-Assemblies pro Provider, Begruendung notiert
    // [ ] Notizen als jsonb, Abfragen ueber @> und ->>
    // [ ] GIN-Index mit EXPLAIN ANALYZE vorher/nachher belegt
    // [ ] Schriftliche Grenze: Spalte vs. JSONB
    // [ ] Volltextsuche per tsvector-Spalte, verglichen mit LIKE '%...%'
    // [ ] EXPLAIN (ANALYZE, BUFFERS) gelesen, Operatoren zu Kata 10_02 zugeordnet
    // [ ] ON CONFLICT DO UPDATE gegen MERGE aus Kata 10_04 verglichen
    // [ ] Dieselbe Testsuite gruen gegen SQL Server und PostgreSQL
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 10_05");
        await Task.CompletedTask;
    }
}
