
namespace EfCore;

/// <summary>
///     Kata 09_02 — EF Core richtig (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Baut auf Kata 09_01 auf.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] IEntityTypeConfiguration<T> pro Entitaet, keine Attribute im Modell
    // [ ] Strongly-typed IDs mit ValueConverter
    // [ ] Owned Types fuer Value Objects, HasQueryFilter fuer Soft Delete
    // [ ] Optimistic Concurrency via rowversion -> 409 Conflict
    // [ ] Expand/Contract-Migration string -> enum, beide Richtungen getestet
    // [ ] AsNoTracking, Projektion in DTOs, AsSplitQuery
    // [ ] N+1 erzeugen, im SQL-Log nachweisen, fixen
    // [ ] Integrationstests mit Testcontainers gegen echtes SQL Server
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 09_02");
    }
}
