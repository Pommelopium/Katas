
namespace MultiTenancy;

/// <summary>
///     Kata 09_07 — Mandantenfaehigkeit (Stufe 3: API, Persistenz, Architektur)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Isolationsmodell begruendet gewaehlt (DB / Schema / TenantId) und gebaut
    // [ ] Mandantenermittlung als ITenantContext, unbekannter Mandant bricht ab
    // [ ] Globaler Query-Filter und TenantId-Vergabe in SaveChanges, nicht im Handler
    // [ ] Querzugriffstest: Lesen, Aendern, Loeschen ueber Id, Join und IgnoreQueryFilters
    // [ ] Loecher geschlossen: rohes SQL, Bulk, Cache-Keys, Hintergrundjobs
    // [ ] Mandantenbezogene Konfiguration und Rate Limits
    // [ ] TenantId als Dimension in Logs, Traces, Metriken
    // [ ] Verfahren fuer Migrationen ueber N Mandanten beschrieben
    // [ ] Onboarding, Export und restlose Loeschung eines Mandanten
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 09_07");
        await Task.CompletedTask;
    }
}
