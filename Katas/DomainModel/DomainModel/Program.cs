
namespace DomainModel;

/// <summary>
///     Kata 09_04 — Domaenenmodell mit DDD-Bausteinen (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Aggregate Root mit privater Liste + IReadOnlyList-Zugriff
    // [ ] keine oeffentlichen Konstruktoren, Factory-Methode Plan.Create(...)
    // [ ] Value Objects mit Wertgleichheit
    // [ ] Invarianten in der Domaene, nicht im Validator
    // [ ] Domain Events sammeln, nach SaveChanges dispatchen
    // [ ] Domaenentests ohne Mocking und ohne DB
    static void Main(string[] args)
    {
        Console.WriteLine("Kata 09_04");
    }
}
