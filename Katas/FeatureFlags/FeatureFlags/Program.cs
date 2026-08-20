
namespace FeatureFlags;

/// <summary>
///     Kata 11_10 — Feature Flags und Progressive Delivery
///     (Stufe 4: Verteilte Systeme und Betrieb)
///     Vollstaendige Aufgabenbeschreibung: siehe README.md in diesem Projekt.
/// </summary>
class Program
{
    // Checkliste:
    // [ ] Flag ohne Neustart wirksam; Unterschied zu IOptions gezeigt
    // [ ] Release-, Experiment-, Ops- und Permission-Toggle je einmal gebaut
    // [ ] Zielgruppen-Filter mit stabiler Zuordnung, per Test bewiesen
    // [ ] Unfertiges Feature hinter ausgeschaltetem Flag ausgeliefert
    // [ ] Rollout 1 % -> 10 % -> 100 % mit vorher definiertem Abbruchkriterium
    // [ ] Kill Switch ohne Deployment, Wirkungsdauer gemessen
    // [ ] Testregel fuer Flag-Kombinationen; Tests setzen Flags explizit
    // [ ] Ablaufdatum je Flag, roter Test bei abgelaufenem Release Toggle
    // [ ] Ein Flag restlos entfernt, toter Zweig mit
    // [ ] Flag-Zustand in Logs und Traces
    // [ ] Abgrenzung Flag vs. Konfiguration vs. Berechtigung vs. Verzweigung
    static async Task Main(string[] args)
    {
        Console.WriteLine("Kata 11_10");
        await Task.CompletedTask;
    }
}
